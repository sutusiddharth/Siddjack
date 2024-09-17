using CLAPi.Core.DBHelper;
using CLAPi.Core.Extensions;
using CLAPi.Core.FileStorage;
using CLAPi.Core.GenericServices;
using CLAPi.Core.Settings;
using CLAPi.ExcelEngine.Api.BackGroundJob;
using CLAPi.ExcelEngine.Api.DTOs;
using CLAPi.ExcelEngine.Api.Models;
using CLAPi.ExcelEngine.Api.Responses;
using CLAPi.ExcelEngine.Api.Services;
using CLAPi.ExcelEngine.Middleware;
using ConfigurationUtility.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Syncfusion.XlsIO;
using Syncfusion.XlsIORenderer;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CLAPi.ExcelEngine.Api.Controllers;

[Authorize]
[Route(ConstantValues.escalator)]
[ApiController]
////public class ExcelToApiController(ILogger<ExcelToApiController> logger, IMongoDBContext mongoDBContext, IFileService fileStorage, IMemoryCache memoryCache, IBackgroundTaskQueue taskQueue, Api Service apiService, ISecretsManagerService secretsManager) : BaseController
public class ExcelToApiController(ILogger<ExcelToApiController> logger, IMongoDBContext mongoDbContext, IFileService fileStorage, IMemoryCache memoryCache, IBackgroundTaskQueue taskQueue, ITokenDetail token) : BaseController
{
    ////private readonly ApiService apiService = apiService;
    ////private readonly ISecretsManagerService secretsManager = secretsManager;

    #region GenerateApi
    [HttpPost]
    [ActionFromHeader]
    public async Task<IActionResult> GenerateApi([FromBody] GenerateApiModel model)
    {
        if (!ConstantValues.ExcelExtentsions.Contains(Path.GetExtension(model.File_Nm)))
            ErrorFormats.ThrowValidationException(errorMessage: ErrorMessages.InvalidExcel, propertyName: nameof(model.File_Nm));

        ExcelToApiResponseObj apiResponse = new();
        var singleQuote = "'";
        if (model.File_Nm.Trim().Contains(singleQuote))
        {
            model.File_Nm = model.File_Nm.Trim().Replace("'", "_");
        }
        if (model.File_Nm.Contains(' '))
        {
            model.File_Nm = model.File_Nm.Replace(" ", string.Empty);
        }

        var templateFileDto = Configuration.AutoMapper<GenerateApiModel, TemplateFileDto>(model);
        templateFileDto.Crtd_Usr = token.UserDetail().Key;
        templateFileDto.Crtd_Dt = Configuration.DefaultDate();
        templateFileDto.Actv_Ind = ConstantValues.Actv_Ind_1;
        templateFileDto.Template_Cd = Path.GetFileNameWithoutExtension(model.File_Nm);
        templateFileDto.Correlation_Id = string.IsNullOrEmpty(model.Correlation_Id) ? Guid.NewGuid().ToString() : model.Correlation_Id;

        var getTemplateDocument = new GetTemplateDocument
        {
            Folder_Nm = model.Folder_Nm,
            SubFolder_Nm = model.SubFolder_Nm,
            Template_Cd = templateFileDto.Template_Cd,
            Actv_Ind = 1
        };
        var templateFiles = await mongoDbContext.GetAsync<TemplateFileResponseObj>(getTemplateDocument.ToDictionary(), ConstantValues.TemplateFile);
        var latestTemplate = templateFiles.FirstOrDefault();
        templateFileDto.Record_Version = latestTemplate != null ? ExcelToApiConvertor.GetChangedVersioned(latestTemplate.Record_Version, model.Upload_Type) : ConstantValues.InitialVersion;

        var templateDocumentDto = Configuration.AutoMapper<TemplateFileDto, TemplateDocumentDto>(templateFileDto);
        templateDocumentDto.Filter_Year = Convert.ToDateTime(templateDocumentDto.Crtd_Dt).Year;
        templateDocumentDto.Folder_Path_Nm = Guid.NewGuid().ToString();
        templateDocumentDto.Template_Nm = model.File_Nm;
        templateDocumentDto.Template_Type = ConstantValues.Excel;
        templateDocumentDto.Folder_Path = $"{templateDocumentDto.Filter_Year}/{templateDocumentDto.Template_Cd}/{templateDocumentDto.Folder_Path_Nm}";

        var basePath = Path.Combine(AppSettings.ContentRootPath, ConstantValues.wwwroot, ConstantValues.TempDoc, templateDocumentDto.Folder_Path_Nm);
        if (!Directory.Exists(basePath))
        {
            Directory.CreateDirectory(basePath);
        }
        var bytes = Convert.FromBase64String(model.File_Stream);
        memoryCache.Set($"{templateDocumentDto.Template_Nm},{templateDocumentDto.Folder_Path}", bytes);
        templateFileDto.Size = GetFileSize(bytes);
        MemoryStream fileStream = new(bytes);

        FileStorageInfo info = new()
        {
            File_Nm = templateDocumentDto.Template_Nm,
            Folder_Path = templateDocumentDto.Folder_Path
        };

        using (Syncfusion.XlsIO.ExcelEngine excelEngine = new())
        {
            var application = excelEngine.Excel;
            application.DefaultVersion = ExcelVersion.Xlsx;
            var workbook = application.Workbooks.Open(fileStream, ExcelOpenType.Automatic);
            List<(string, dynamic)> dataValidationValuePairs = [];
            //Get All the input and output field with type

            var inputData = ExcelToApiConvertor.GetFieldWithType(workbook, ConstantValues.Excel_Input, ref dataValidationValuePairs);
            var outputData = ExcelToApiConvertor.GetFieldWithType(workbook, ConstantValues.Excel_Output, ref dataValidationValuePairs);

            //Calling to generate json with I/O fields
            templateFileDto.Input_Data = Configuration.JsonFromList(inputData);
            templateFileDto.Output_Data = Configuration.JsonFromList(outputData);
            templateFileDto.List_Data = Configuration.JsonFromList(dataValidationValuePairs);
            templateFileDto.Sheet_Count = workbook.Worksheets.Count;
            templateFileDto.Hidden_Sheet_Count = workbook.Worksheets.Count(a => a.Visibility == WorksheetVisibility.Hidden);
            templateFileDto.Doc_Count = workbook.Worksheets.Count(a => a.Name.Contains(ConstantValues.Print));

            await mongoDbContext.InsertAsync(templateDocumentDto, ConstantValues.TemplateDocument);
            await mongoDbContext.InsertAsync(templateDocumentDto, ConstantValues.TemplateTransactionHistory);

            fileStorage.UploadFile(fileStream, info);

            if (latestTemplate != null)
            {
                var updateTemplateFileDto = Configuration.AutoMapper<TemplateFileDto, UpdateTemplateFileDto>(templateFileDto);
                updateTemplateFileDto.Id = latestTemplate.Id;
                await mongoDbContext.UpdateAsync(updateTemplateFileDto, ConstantValues.TemplateFile);
            }
            else
            {
                await mongoDbContext.InsertAsync(templateFileDto, ConstantValues.TemplateFile);
            }
            await mongoDbContext.InsertAsync(templateFileDto, ConstantValues.TemplateFileHistory);
            memoryCache.Set($"{templateFileDto.Folder_Nm},{templateFileDto.SubFolder_Nm},{templateFileDto.Template_Cd},{templateFileDto.Record_Version}", templateFileDto);
            memoryCache.Set($"{templateFileDto.Folder_Nm},{templateFileDto.SubFolder_Nm},{templateFileDto.Template_Cd}", templateFileDto);

            //print sheets which are having to print
            foreach (var worksheet in workbook.Worksheets.Where(a => a.Name.Contains(ConstantValues.Print)))
            {
                //Initialize XlsIO renderer.
                XlsIORenderer renderer = new();
                //Convert Excel document into PDF document
                var pdfDocument = renderer.ConvertToPDF(worksheet);
                var pdfName = $"{worksheet.Name.Replace(ConstantValues.Print, ConstantValues.EmptyString, StringComparison.OrdinalIgnoreCase)}.pdf";
                var pdfPath = $"{basePath}/{pdfName}";
                await using (Stream stream = new FileStream(pdfPath, FileMode.Create, FileAccess.ReadWrite))
                {
                    pdfDocument.Save(stream);
                }
                templateDocumentDto.Template_Type = ConstantValues.Pdf;
                templateDocumentDto.Template_Nm = pdfName;
                var pdfFileStream = Convert.ToBase64String(Configuration.ConvertFileToByte(pdfPath));
                bytes = Convert.FromBase64String(pdfFileStream);
                info.File_Nm = templateDocumentDto.Template_Nm;
                fileStream = new(bytes);
                fileStorage.UploadFile(fileStream, info);
                await mongoDbContext.InsertAsync(templateDocumentDto, ConstantValues.TemplateDocument);
                await mongoDbContext.InsertAsync(templateDocumentDto, ConstantValues.TemplateTransactionHistory);
            }
            workbook.Close();
        }
        //Delete all the files inside that directory
        var files = Directory.GetFiles(basePath);
        foreach (var file in files)
        {
            System.IO.File.Delete(file);
        }
        // Delete the directory itself
        Directory.Delete(basePath);

        apiResponse.Message = "Api generated successfully";
        apiResponse.Correlation_Id = templateDocumentDto.Correlation_Id;
        logger.Log(LogLevel.Information, "Api generated successfully");
        return Ok(apiResponse);
    }
    #endregion
    [HttpPost]
    [ActionFromHeader]
    public async Task<IActionResult> CopyApi([FromBody] CopyApiModel model)
    {
        model.Actv_Ind = 1;
        var fileResponses = await mongoDbContext.GetAsync<TemplateFileResponseObj>(model.ToDictionary(), ConstantValues.TemplateFile);
        var docResponses = await mongoDbContext.GetAsync<TemplateDocumentResponseObj>(model.ToDictionary(), ConstantValues.TemplateDocument);
        if (fileResponses.Count == 0 && docResponses.Count == 0)
        {
            ErrorFormats.ThrowValidationException(errorMessage: ErrorMessages.NotFound, propertyName: nameof(model.Template_Cd));
        }
        ExcelToApiResponseObj apiResponse = new();
        var templateFileObj = fileResponses[0];
        var templateFile = Configuration.AutoMapper<TemplateFileResponseObj, TemplateFileDto>(templateFileObj);
        var templateDocuments = Configuration.AutoMapper<List<TemplateDocumentResponseObj>, List<TemplateDocumentDto>>(docResponses);
        templateFile.Release_Note = $"Restored from version {templateFile.Record_Version}";

        model.Record_Version = null;
        var templateFiles = await mongoDbContext.GetAsync<TemplateFileResponseObj>(model.ToDictionary(), ConstantValues.TemplateFile);
        var latestTemplate = templateFiles.FirstOrDefault();
        templateFile.Record_Version = latestTemplate != null ? ExcelToApiConvertor.GetChangedVersioned(latestTemplate.Record_Version, ConstantValues.Patch) : ConstantValues.InitialVersion;

        templateFile.Crtd_Usr = token.UserDetail().Key;
        templateFile.Crtd_Dt = Configuration.DefaultDate();

        await mongoDbContext.InsertAsync(templateFile, ConstantValues.TemplateFile);
        memoryCache.Set($"{templateFile.Folder_Nm},{templateFile.SubFolder_Nm},{templateFile.Template_Cd},{templateFile.Record_Version}", templateFile);
        memoryCache.Set($"{templateFile.Folder_Nm},{templateFile.SubFolder_Nm},{templateFile.Template_Cd}", templateFile);
        foreach (var templateDocument in templateDocuments)
        {
            templateDocument.Record_Version = templateFile.Record_Version;
            templateDocument.Crtd_Dt = templateFile.Crtd_Dt;
            templateDocument.Crtd_Usr = templateFile.Crtd_Usr;

            await mongoDbContext.InsertAsync(templateDocument, ConstantValues.TemplateDocument);
        }

        apiResponse.Message = "Version restored successfully";
        return Ok(apiResponse);
    }

    //Get all the file data based on Particular collection
    [HttpGet]
    [ActionFromHeader]
    public async Task<IActionResult> GetTemplateFile([FromQuery] GetTemplateFileModel getTemplate)
    {
        getTemplate.Actv_Ind = 1;

        var fileResponses = await mongoDbContext.GetAsync<TemplateFileResponseObj>(getTemplate.ToDictionary(), ConstantValues.TemplateFile);
        if (fileResponses.Count == 0)
            ErrorFormats.ThrowValidationException(ErrorMessages.NotFound, nameof(getTemplate.Folder_Nm));
        var latestData = fileResponses.GroupBy(item => item.Template_Cd)
        .Select(group => group.OrderByDescending(item => item.Record_Version).First()).ToList();
        return Ok(latestData);
    }
    //Get all the file data based on Particular collection
    [HttpGet]
    [ActionFromHeader]
    public async Task<IActionResult> GetTemplateVersions([FromQuery] GetTemplateVersionModel getTemplate)
    {
        getTemplate.Actv_Ind = 1;
        var fileResponses = await mongoDbContext.GetAsync<TemplateFileResponseObj>(getTemplate.ToDictionary(), ConstantValues.TemplateFileHistory);

        return Ok(fileResponses);
    }
    //Get Template document detail based on template code
    [HttpGet]
    [ActionFromHeader]
    public async Task<IActionResult> GetTemplateDocument([FromQuery] GetTemplateDocument getTemplate)
    {
        getTemplate.Actv_Ind = 1;
        var templateFileResponses = await mongoDbContext.GetAsync<TemplateFileResponseObj>(getTemplate.ToDictionary(), ConstantValues.TemplateFile);
        return Ok(templateFileResponses);
    }
    [HttpGet]
    [ActionFromHeader]
    public async Task<IActionResult> GetTemplateDocumentList([FromQuery] GetTemplateDocument getTemplate)
    {
        getTemplate.Actv_Ind = 1;
        var templateDocumentResponses = await mongoDbContext.GetAsync<TemplateDocumentResponseObj>(getTemplate.ToDictionary(), ConstantValues.TemplateDocument);
        foreach (var item in templateDocumentResponses)
        {
            FileStorageInfo info = new()
            {
                File_Nm = item.Template_Nm,
                Folder_Path = item.Folder_Path
            };
            item.File_Url = fileStorage.GetFileUrl(info);
        }
        return Ok(templateDocumentResponses);
    }
    [HttpGet]
    [ActionFromHeader]
    public async Task<IActionResult> GetTemplateTransactionHistory([FromQuery] GetTemplateTransHistoryModel getTemplate)
    {
        var templateDocumentResponses = await mongoDbContext.GetAsync<TemplateDocumentResponseObj>(getTemplate.ToDictionary(), ConstantValues.TemplateTransactionHistory);
        if (templateDocumentResponses.Count == 0)
            ErrorFormats.ThrowValidationException(ErrorMessages.NoDoc, nameof(getTemplate.Correlation_Id));
        foreach (var item in templateDocumentResponses)
        {
            FileStorageInfo info = new()
            {
                File_Nm = item.Template_Nm,
                Folder_Path = item.Folder_Path
            };
            item.File_Url = fileStorage.GetFileUrl(info);
        }
        return Ok(templateDocumentResponses);
    }

    #region Calculate Premium Api
    //Calculate premium based on json parameter along with TemplateDocument_Id sent through Header
    [HttpPost]
    [ActionFromHeader]
    public async Task<IActionResult> CalculatePremium([FromBody] CalculatePremiumModel calculatePremium)
    {
        PremiumResponseObj premiumResponse = new();
        var getTemplate = Configuration.AutoMapper<CalculatePremiumModel, GetTemplateDocument>(calculatePremium);
        getTemplate.Actv_Ind = 1;

        var templateFileResponse = await GetTemplateFileData(getTemplate);

        var templateDocumentResponses = await mongoDbContext.GetAsync<TemplateDocumentResponseObj>(getTemplate.ToDictionary(), ConstantValues.TemplateDocument);

        var templateDocumentResponse = string.IsNullOrEmpty(calculatePremium.Record_Version) ? templateDocumentResponses.Where(a => a.Template_Type == ConstantValues.Excel).OrderByDescending(a => a.Record_Version).FirstOrDefault() : templateDocumentResponses.Find(a => a.Record_Version == calculatePremium.Record_Version && a.Template_Type == ConstantValues.Excel);

        if (!IsValidJson(calculatePremium.Json))
            ErrorFormats.ThrowValidationException(errorMessage: ErrorMessages.JsonInvalid, propertyName: nameof(calculatePremium.Json));
        calculatePremium.Correlation_Id = string.IsNullOrEmpty(calculatePremium.Correlation_Id) ? Guid.NewGuid().ToString() : calculatePremium.Correlation_Id;
        if (templateDocumentResponse != null && templateFileResponse != null)
        {
            string updatedJson = GetPremiumJson(calculatePremium, templateFileResponse.Input_Data);

            dynamic? modelData = ExcelToApiConvertor.DeserializeExpando(updatedJson);

            var bytes = GetFileStream($"{templateDocumentResponse.Template_Nm},{templateDocumentResponse.Folder_Path}");
            if (bytes is null)
                ErrorFormats.ThrowValidationException(errorMessage: ErrorMessages.SomethingWentWrong, propertyName: nameof(bytes));
            MemoryStream fileStream = new(bytes!);
            ////outputString = ExcelApiConvertor.CallExcelEngine(modelData, fileStream, templateFileResponse.Is_Api, apiService, secretsManager);
            string outputString = ExcelToApiConvertor.CallExcelEngine(modelData, fileStream);

            var templateAccessHistory = Configuration.AutoMapper<TemplateFileDto, TemplateAccessHistoryDto>(templateFileResponse);
            templateAccessHistory.Request_Json = JsonConvert.SerializeObject(calculatePremium);
            templateAccessHistory.Input_Json = updatedJson;
            templateAccessHistory.Response_Json = outputString;
            templateAccessHistory.Crtd_Dt = Configuration.DefaultDate();
            templateAccessHistory.Crtd_Usr = token.UserDetail().Key;
            templateAccessHistory.Correlation_Id = calculatePremium.Correlation_Id;
            await mongoDbContext.InsertAsync(templateAccessHistory, ConstantValues.TemplateAccessHistory);

            premiumResponse.ResponseJson = outputString;
            premiumResponse.Correlation_Id = calculatePremium.Correlation_Id;
        }
        else
        {
            ErrorFormats.ThrowValidationException(errorMessage: ErrorMessages.JsonInvalid, propertyName: nameof(calculatePremium.Json));
        }
        if (calculatePremium.Is_Doc)
        {
            taskQueue.QueueBackgroundWorkItem(async token =>
            {
                _ = await PostModifiedFiles(new PostModifiedFileModel() { Correlation_Id = calculatePremium.Correlation_Id });
            });
        }
        return Ok(premiumResponse);
    }

    private static string GetPremiumJson(CalculatePremiumModel calculatePremium, string Input_Data)
    {
        if (string.IsNullOrEmpty(calculatePremium.Source_System_Nm))
        {
            // Deserialize the JSON strings
            var data1 = JsonConvert.DeserializeObject<JObject>(Input_Data);
            var data2 = JsonConvert.DeserializeObject<JObject>(calculatePremium.Json);

            // Update the first JSON with values from the second JSON
            if (data1 != null && data2 != null)
            {
                UpdateJson(data1, data2);

                // Serialize the updated JSON back to string
                return JsonConvert.SerializeObject(data1, Formatting.Indented);
            }
            else
            {
                return calculatePremium.Json;
            }
        }
        else
        {
            return calculatePremium.Json;
        }
    }
    #endregion

    [HttpPost]
    [ActionFromHeader]
    public async Task<IActionResult> PostModifiedFiles([FromBody] PostModifiedFileModel getModifiedFileModel)
    {
        var templateAccessHistories = await mongoDbContext.GetAsync<TemplateAccessHistoryObj>(getModifiedFileModel.ToDictionary(), ConstantValues.TemplateAccessHistory);

        if (templateAccessHistories.Count == 0)
            ErrorFormats.ThrowValidationException(ErrorMessages.NotFound, nameof(getModifiedFileModel.Correlation_Id));

        var getTemplate = Configuration.AutoMapper<TemplateAccessHistoryDto, GetTemplateDocument>(templateAccessHistories[0]);
        getTemplate.Actv_Ind = 1;

        var templateFileResponse = await GetTemplateFileData(getTemplate);

        if (templateFileResponse is { Doc_Count: 0 })
            ErrorFormats.ThrowValidationException(ErrorMessages.NoDoc, nameof(getModifiedFileModel.Correlation_Id));

        var templateDocumentResponses = await mongoDbContext.GetAsync<TemplateDocumentResponseObj>(getTemplate.ToDictionary(), ConstantValues.TemplateDocument);

        var templateDocumentResponse = templateDocumentResponses.Find(a => a.Template_Type == ConstantValues.Excel);
        if (templateDocumentResponse != null && templateFileResponse != null)
        {
            var basePath = Path.Combine(AppSettings.ContentRootPath, ConstantValues.wwwroot, ConstantValues.TempDoc, getModifiedFileModel.Correlation_Id);
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }
            dynamic? requestData = ExcelToApiConvertor.DeserializeExpando(templateAccessHistories[0].Input_Json);
            dynamic? responseData = ExcelToApiConvertor.DeserializeExpando(templateAccessHistories[0].Response_Json);

            var bytes = GetFileStream($"{templateDocumentResponse.Template_Nm},{templateDocumentResponse.Folder_Path}");
            MemoryStream fileStream = new(bytes);

            List<Tuple<byte[], string>> pdfFileStreams = ExcelToApiConvertor.CallDocExcelEngine(requestData, responseData, fileStream, basePath);

            if (pdfFileStreams.Count > 0)
            {
                var templateDocumentDto = Configuration.AutoMapper<TemplateDocumentResponseObj, TemplateDocumentDto>(templateDocumentResponse);
                templateDocumentDto.Crtd_Dt = Configuration.DefaultDate();
                templateDocumentDto.Crtd_Usr = token.UserDetail().Key;
                templateDocumentDto.Filter_Year = Convert.ToDateTime(templateDocumentDto.Crtd_Dt).Year;
                templateDocumentDto.Folder_Path_Nm = Guid.NewGuid().ToString();
                templateDocumentDto.Correlation_Id = getModifiedFileModel.Correlation_Id;

                FileStorageInfo info = new();
                foreach (var item in pdfFileStreams)
                {
                    templateDocumentDto.Template_Type = ConstantValues.Pdf;
                    templateDocumentDto.Template_Nm = item.Item2;
                    templateDocumentDto.Folder_Path = $"{templateDocumentDto.Filter_Year}/{templateDocumentDto.Template_Cd}/{templateDocumentDto.Folder_Path_Nm}";

                    info.File_Nm = templateDocumentDto.Template_Nm;
                    info.Folder_Path = templateDocumentDto.Folder_Path;
                    MemoryStream ms = new(item.Item1);
                    fileStorage.UploadFile(ms, info);
                    await mongoDbContext.InsertAsync(templateDocumentDto, ConstantValues.TemplateTransactionHistory);
                }
            }
            //Delete all the files inside that directory
            var files = Directory.GetFiles(basePath);
            foreach (var file in files)
            {
                System.IO.File.Delete(file);
            }
            // Delete the directory itself
            Directory.Delete(basePath);

            return Ok(pdfFileStreams);
        }
        else
        {
            ErrorFormats.ThrowValidationException(errorMessage: ErrorMessages.NotFound, propertyName: nameof(getModifiedFileModel.Correlation_Id));
            return NotFound();
        }
    }
    //Get Master Data based on name and template Code and Version
    [HttpGet]
    [ActionFromHeader]
    public async Task<IActionResult> GetTemplateMaster([FromQuery] GetTemplateMasterModel getTemplateMaster)
    {
        MasterResponseObj masterResponse = new();
        getTemplateMaster.Actv_Ind = 1;

        var getTemplateDocument = Configuration.AutoMapper<GetTemplateMasterModel, GetTemplateDocument>(getTemplateMaster);

        var templateFileResponses = await mongoDbContext.GetAsync<TemplateFileResponseObj>(getTemplateDocument.ToDictionary(), ConstantValues.TemplateFile);
        var templateData = templateFileResponses.FirstOrDefault();

        if (templateData is null)
            ErrorFormats.ThrowValidationException(errorMessage: ErrorMessages.NotFound, propertyName: nameof(getTemplateMaster.Template_Cd));
        else
        {
            var data = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(templateData.List_Data);

            // Filter the data by key (from filterRequest)
            if (data != null && data.TryGetValue(getTemplateMaster.Master_Key, out var filteredData))
            {
                // Parse the second element of the list as JSON array (since it's a string)
                var values = JsonConvert.DeserializeObject<List<string>>(filteredData[1]);

                // Return the filtered data
                masterResponse.Master_Key = getTemplateMaster.Master_Key;
                masterResponse.Json = values!;
                return Ok(masterResponse);
            }
            else
            {
                ErrorFormats.ThrowValidationException(errorMessage: ErrorMessages.NotFound, propertyName: nameof(getTemplateMaster.Template_Cd));
            }
        }
        return Ok(masterResponse);
    }
    [HttpGet]
    [ActionFromHeader]
    public async Task<IActionResult> GetTemplateAccessHistory([FromQuery] GetTemplateAccessHistoryModel getTemplate)
    {
        var fileResponses = await mongoDbContext.GetAsync<TemplateAccessHistoryObj>(getTemplate.ToDictionary(), ConstantValues.TemplateAccessHistory);
        if (fileResponses.Count == 0)
            ErrorFormats.ThrowValidationException(ErrorMessages.NoDoc, nameof(getTemplate.Template_Cd));
        return Ok(fileResponses);
    }
    [HttpGet]
    [ActionFromHeader]
    public async Task<IActionResult> TemplateFileDownload([FromQuery] TemplateFileDownloadModel getTemplate)
    {
        getTemplate.Actv_Ind = 1;
        var downloadType = getTemplate.Download_Type;
        var password = getTemplate.Password;
        getTemplate.Download_Type = string.Empty;
        getTemplate.Password = string.Empty;
        var templateDocumentResponses = await mongoDbContext.GetAsync<TemplateDocumentResponseObj>(getTemplate.ToDictionary(), ConstantValues.TemplateDocument);
        var templateDoc = templateDocumentResponses.Find(a => a.Template_Type == ConstantValues.Excel);

        if (templateDoc == null)
            ErrorFormats.ThrowValidationException(errorMessage: ErrorMessages.NotFound, propertyName: nameof(getTemplate.Template_Cd));

        TemplateDownloadObj templateDownloadObj = new();
        FileStorageInfo info = new()
        {
            File_Nm = templateDoc?.Template_Nm,
            Folder_Path = templateDoc?.Folder_Path
        };
        templateDownloadObj.Stream = fileStorage.GetFileStream(info);
        if (downloadType == "Enc")
        {
            templateDownloadObj.Stream = AddPasswordProtection(templateDownloadObj.Stream, password);
        }
        return Ok(templateDownloadObj);
    }
    private static byte[] AddPasswordProtection(byte[] fileBytes, string password)
    {
        using var excelEngine = new Syncfusion.XlsIO.ExcelEngine();
        var application = excelEngine.Excel;
        application.DefaultVersion = ExcelVersion.Excel2016;

        using var stream = new MemoryStream(fileBytes);
        var workbook = application.Workbooks.Open(stream);
        workbook.PasswordToOpen = password;

        using var outputStream = new MemoryStream();
        workbook.SaveAs(outputStream);
        return outputStream.ToArray();
    }
    [HttpPost]
    [ActionFromHeader]
    public async Task<IActionResult> PostBusinessType([FromBody] PostBusinessTypeModel model)
    {
        IEnumerable<BusinessTypeResponseObj> businessTypeResponses = await mongoDbContext.GetAsync<BusinessTypeResponseObj>(model.ToDictionary(), ConstantValues.BusinessType);
        ExcelToApiResponseObj apiResponse = new();
        if (businessTypeResponses.Any())
            ErrorFormats.ThrowValidationException(errorMessage: ErrorMessages.Exists, propertyName: nameof(model.Folder_Nm));

        PostBusinessTypeDto postBusinessType = new()
        {
            Actv_Ind = ConstantValues.Actv_Ind_1,
            Crtd_Usr = token.UserDetail().Key,
            Crtd_Dt = Configuration.DefaultDate(),
            Folder_Nm = model.Folder_Nm
        };
        await mongoDbContext.InsertAsync(postBusinessType, ConstantValues.BusinessType);
        apiResponse.Message = "Folder Added Successfully";
        return Ok(apiResponse);
    }
    [HttpGet]
    [ActionFromHeader]
    public async Task<IActionResult> GetBusinessType()
    {
        var fileResponses = await mongoDbContext.GetAllAsync<BusinessTypeResponseObj>(ConstantValues.BusinessType);
        if (fileResponses.Count == 0)
            ErrorFormats.ThrowValidationException(ErrorMessages.NotFound, ConstantValues.BusinessType);
        return Ok(fileResponses);
    }
    [HttpPost]
    [ActionFromHeader]
    public async Task<IActionResult> PostBusinessSubType([FromBody] PostBusinessSubTypeModel model)
    {
        var businessSubTypeResponses = await mongoDbContext.GetAsync<BusinessSubTypeResponseObj>(model.ToDictionary(), ConstantValues.BusinessSubType);
        if (businessSubTypeResponses.Count > 0)
            ErrorFormats.ThrowValidationException(errorMessage: ErrorMessages.Exists, propertyName: nameof(model.SubFolder_Nm));

        ExcelToApiResponseObj apiResponse = new();
        PostBusinessSubTypeDto postBusinessSubType = new()
        {
            Actv_Ind = ConstantValues.Actv_Ind_1,
            Crtd_Usr = token.UserDetail().Key,
            Crtd_Dt = Configuration.DefaultDate(),
            SubFolder_Nm = model.SubFolder_Nm,
            Folder_Nm = model.Folder_Nm
        };
        await mongoDbContext.InsertAsync(postBusinessSubType, ConstantValues.BusinessSubType);
        apiResponse.Message = "SubFolder Added Successfully";
        return Ok(apiResponse);
    }
    [HttpGet]
    [ActionFromHeader]
    public async Task<IActionResult> GetBusinessSubType([FromQuery] GetBusinessSubTypeModel getTemplate)
    {
        var fileResponses = await mongoDbContext.GetAsync<BusinessSubTypeResponseObj>(getTemplate.ToDictionary(), ConstantValues.BusinessSubType);
        if (fileResponses.Count == 0)
            ErrorFormats.ThrowValidationException(ErrorMessages.NoDoc, nameof(getTemplate.Folder_Nm));
        return Ok(fileResponses);
    }
    // Function to recursively update JSON structure
    private static void UpdateJson(JObject source, JObject target)
    {
        foreach (var property in source.Properties())
        {
            if (target.TryGetValue(property.Name, out var targetToken))
            {
                HandleMatchingProperty(property, targetToken);
            }
            else
            {
                HandleNonMatchingProperty(property);
            }
        }
    }

    private static void HandleMatchingProperty(JProperty sourceProperty, JToken targetToken)
    {
        if (sourceProperty.Value.Type == JTokenType.Array && targetToken.Type == JTokenType.Array)
        {
            UpdateArray((JArray)sourceProperty.Value, (JArray)targetToken);
        }
        else if (sourceProperty.Value.Type == JTokenType.Object && targetToken.Type == JTokenType.Object)
        {
            UpdateJson((JObject)sourceProperty.Value, (JObject)targetToken);
        }
        else
        {
            sourceProperty.Value[1] = targetToken; // Primitive type case
        }
    }

    private static void HandleNonMatchingProperty(JProperty property)
    {
        if (property.Value.Type == JTokenType.Object)
        {
            property.Value.Replace(new JObject());
        }
        else if (property.Value.Type == JTokenType.Array)
        {
            property.Value[1] = ConstantValues.EmptyString;
        }
        else
        {
            property.Value = ConstantValues.EmptyString;
        }
    }

    private static void UpdateArray(JArray sourceArray, JArray targetArray)
    {
        for (var i = 0; i < sourceArray.Count; i++)
        {
            if (sourceArray[i].Type == JTokenType.Object && targetArray[i].Type == JTokenType.Object)
            {
                UpdateJson((JObject)sourceArray[i], (JObject)targetArray[i]);
            }
            else if (sourceArray[i].Type == JTokenType.Array && targetArray[i].Type == JTokenType.Array)
            {
                UpdateSubArray((JArray)sourceArray[i], (JArray)targetArray[i]);
            }
            else
            {
                sourceArray[i] = targetArray[i];
            }
        }
    }

    private static void UpdateSubArray(JArray sourceSubArray, JArray targetSubArray)
    {
        for (var j = 0; j < sourceSubArray.Count; j++)
        {
            sourceSubArray[j] = targetSubArray[j];
        }
    }

    private static bool IsValidJson(string jsonString)
    {
        try
        {
            JToken.Parse(jsonString);
            return true;
        }
        catch (JsonReaderException)
        {
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }
    private byte[] GetFileStream(string path)
    {
        var fileStream = memoryCache.Get<byte[]>(path);
        if (fileStream is null)
        {
            FileStorageInfo info = new()
            {
                File_Nm = path.Split(',')[0],
                Folder_Path = path.Split(',')[1]
            };
            fileStream = fileStorage.GetFileStream(info);
            memoryCache.Set(path, fileStream);
        }
        return fileStream;
    }
    private static decimal GetFileSize(byte[] bytes)
    {
        var fileSizeInBytes = bytes.Length;
        return Configuration.CustomRound((decimal)fileSizeInBytes / 1024, ConstantValues.Decimal_Place_2);
    }
    private async Task<TemplateFileDto?> GetTemplateFileData(GetTemplateDocument getTemplateDocument)
    {
        var cacheKey = string.IsNullOrEmpty(getTemplateDocument.Record_Version) ?
            $"{getTemplateDocument.Folder_Nm},{getTemplateDocument.SubFolder_Nm},{getTemplateDocument.Template_Cd}" :
            $"{getTemplateDocument.Folder_Nm},{getTemplateDocument.SubFolder_Nm},{getTemplateDocument.Template_Cd},{getTemplateDocument.Record_Version}";

        var collectionName = string.IsNullOrEmpty(getTemplateDocument.Record_Version) ? ConstantValues.TemplateFile : ConstantValues.TemplateFileHistory;
        TemplateFileDto? templateFileResponseObj;
        if (memoryCache.Get(cacheKey) != null)
        {
            templateFileResponseObj = memoryCache.Get<TemplateFileDto>(cacheKey);
        }
        else
        {
            var templateFiles = await mongoDbContext.GetAsync<TemplateFileDto>(getTemplateDocument.ToDictionary(), collectionName);

            templateFileResponseObj = templateFiles.FirstOrDefault();
            if (templateFileResponseObj != null)
            {
                memoryCache.Set(cacheKey, templateFileResponseObj);
            }
        }
        return templateFileResponseObj;
    }
    [HttpPost]
    [ActionFromHeader]
    public async Task<IActionResult> PostUserRegistration([FromBody] UserRegistrationDto registrationDto)
    {
        ExcelToApiResponseObj apiResponse = new();
        try
        {
            EncryptionDecryption.DecryptStringAes(registrationDto.Password);
        }
        catch (Exception)
        {
            ErrorFormats.ThrowValidationException(errorMessage: ErrorMessages.PassMessage, nameof(registrationDto.Password));
        }
        var userModel = new GetUserList() { User_Nm = registrationDto.User_Nm, Actv_Ind = ConstantValues.Actv_Ind_1 };
        var userResponses = await mongoDbContext.GetAsync<UserListingObj>(userModel.ToDictionary(), ConstantValues.Users);

        if (userResponses.Count > 0)
            ErrorFormats.ThrowValidationException(ErrorMessages.User_Name_Exists, nameof(userModel.User_Nm));

        registrationDto.Actv_Ind = ConstantValues.Actv_Ind_1;
        registrationDto.Crtd_Usr = token.UserDetail().Key;
        registrationDto.Crtd_Dt = Configuration.DefaultDate();
        await mongoDbContext.InsertAsync(registrationDto, ConstantValues.Users);
        apiResponse.Message = "User Added Successfully";
        return Ok(apiResponse);
    }
    [HttpPost]
    [ActionFromHeader]
    public async Task<IActionResult> PostGenerateOtp([FromBody] PostGenerateOtp model)
    {
        GenerateOtpObj response = new();
        var dto = Configuration.AutoMapper<PostGenerateOtp, GenerateOtpDto>(model);
        dto.OTP = RandomNumberGenerator.GetInt32(0, 999999).ToString("D6");
        dto.Message = "OTP";
        dto.Template_Cd = SmsCredential.OTPTemplateCode;
        dto.Error_Desc = !string.IsNullOrEmpty(dto.OTP) ? "Success" : "Failed";
        dto.SMS_Response = "Failed";
        dto.Actv_Ind = ConstantValues.Actv_Ind_1;
        dto.Sent_Dt = Configuration.DefaultDate();
        dto.OTP_Request = dto.Flag;
        if (dto.Flag == ConstantValues.Email)
        {
            MailItemsDto mailItemsDto = new()
            {
                MailFrom = MailCredential.MailAddress,
                MailSubject = $"One Time Password (OTP)-{ConstantValues.Sys_Name}",
                MailBody = $"Your One Time Password (OTP) is {dto.OTP}. This is valid for 5 minutes",
                MailTo = [new MailAddressDto { MailAddress = dto.Email! }],
                ActiveInd = ConstantValues.Actv_Ind_1,
                MailCc = [],
                MailBcc = [],
                MailAttachment = [],
                Flag = ConstantValues.OTP,
                IsPending = false
            };
            await PostSendMail(mailItemsDto);
            dto.SMS_Response = mailItemsDto.EmailResponse;
        }
        else
        {
            //SendSMS(dto); // Backup
        }
        if (dto.Sent_Dt.HasValue)
        {
            dto.Actv_Ind = ConstantValues.Actv_Ind_1;
            dto.Crtd_Dt = Configuration.DefaultDate();
            dto.Crtd_Usr = token.UserDetail().Key;
            await mongoDbContext.InsertAsync(dto, ConstantValues.OtpItems);
            response.Message = "Otp Generated Successfully";
            response.Error_Desc = dto.Error_Desc;
        }
        return Ok(response);
    }
    private async Task PostSendMail(MailItemsDto dto)
    {
        if (dto.MailTo == null) return;

        using MailMessage mail = new();
        SetMailProperties(mail, dto);
        AddMailRecipients(mail, dto);
        AddMailAttachments(mail, dto);

        using SmtpClient client = CreateSmtpClient();
        await SendEmailAsync(client, mail, dto);

        if (dto.Flag != ConstantValues.OTP)
        {
            await SaveMailBodyToFile(dto);
            ProcessMailAttachments(dto);
        }

        if (dto.IsPending)
        {
            PrepareForMongoDbInsert(dto);
            await mongoDbContext.InsertAsync(dto, "OtpItems");
        }
    }

    private static void SetMailProperties(MailMessage mail, MailItemsDto dto)
    {
        mail.From = new MailAddress(string.IsNullOrEmpty(dto.MailFrom) ? MailCredential.MailAddress : dto.MailFrom);
        mail.Subject = dto.MailSubject;
        mail.Body = dto.MailBody;
        mail.IsBodyHtml = true;
        mail.Priority = dto.MailPriority;
    }

    private static void AddMailRecipients(MailMessage mail, MailItemsDto dto)
    {
        foreach (var item in dto.MailTo!)
        {
            mail.To.Add(new MailAddress(item.MailAddress));
        }

        if (dto.MailCc != null)
        {
            foreach (var item in dto.MailCc)
            {
                mail.CC.Add(new MailAddress(item.MailAddress));
            }
        }

        if (dto.MailBcc != null)
        {
            foreach (var item in dto.MailBcc)
            {
                mail.Bcc.Add(new MailAddress(item.MailAddress));
            }
        }
    }

    private static void AddMailAttachments(MailMessage mail, MailItemsDto dto)
    {
        if (dto.MailAttachment != null)
        {
            foreach (var item in dto.MailAttachment)
            {
                mail.Attachments.Add(new Attachment(item.FilePath));
            }
        }
    }

    private static SmtpClient CreateSmtpClient()
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        var client = new SmtpClient(MailCredential.SmtpServer, MailCredential.SmtpPortNo)
        {
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(MailCredential.MailUserName, MailCredential.MailPassword),
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            TargetName = "STARTTLS/smtp.office365.com"
        };
        return client;
    }

    private static async Task SendEmailAsync(SmtpClient client, MailMessage mail, MailItemsDto dto)
    {
        try
        {
            await client.SendMailAsync(mail);
            dto.SentDt = Configuration.DefaultDate();
            dto.ActiveInd = ConstantValues.Actv_Ind_1;
            dto.EmailResponse = "Success";
        }
        catch (Exception ex)
        {
            dto.ActiveInd = ConstantValues.Actv_Ind_1;
            dto.ErrorDesc = ex.Message;
            dto.EmailResponse = "Fail";
        }
        finally
        {
            dto.MailFrom = string.IsNullOrEmpty(dto.MailFrom) ? MailCredential.MailAddress : dto.MailFrom;
        }
    }

    private async Task SaveMailBodyToFile(MailItemsDto dto)
    {
        var bodyPath = Path.Combine(Environment.CurrentDirectory, $@"wwwroot\{DateTime.Now:yyyyMMddTHHmmssfff}.html");
        await System.IO.File.WriteAllTextAsync(bodyPath, dto.MailBody, Encoding.GetEncoding("iso-8859-1"));

        await using var fileStream = System.IO.File.OpenRead(bodyPath);
        MemoryStream memStream = new();
        memStream.SetLength(fileStream.Length);
        _ = await fileStream.ReadAsync(memStream.GetBuffer().AsMemory(0, (int)fileStream.Length));

        FileStorageInfo info = new()
        {
            File_Nm = $"{Path.GetFileName(bodyPath)}",
            Folder_Path = $"{DateTime.Now.Year}/Mails/{MailCredential.EmailSource}/Body/{DateTime.Now:yyyyMMdd}"
        };

        fileStorage.UploadFile(memStream, info);
        System.IO.File.Delete(bodyPath);
    }

    private void ProcessMailAttachments(MailItemsDto dto)
    {
        if (dto.MailAttachment != null)
        {
            foreach (var item in dto.MailAttachment)
            {
                item.FilePath = Mail_Attachment(item);
            }
        }
    }

    private static void PrepareForMongoDbInsert(MailItemsDto dto)
    {
        if (dto.MailAttachment != null && dto.MailAttachment.Count != 0)
        {
            dto.AttachmentInd = ConstantValues.Actv_Ind_1;
        }

        dto.To = string.Join(";", dto.MailTo!.Select(x => x.MailAddress));
        dto.Cc = dto.MailCc != null ? string.Join(";", dto.MailCc.Select(x => x.MailAddress)) : null;
        dto.Bcc = dto.MailBcc != null ? string.Join(";", dto.MailBcc.Select(x => x.MailAddress)) : null;
        dto.Attachment = dto.MailAttachment != null ? string.Join(";", dto.MailAttachment.Select(x => x.FilePath)) : null;

        if (string.IsNullOrEmpty(dto.ErrorDesc))
        {
            dto.ErrorDesc = "Success";
        }
    }

    private string Mail_Attachment(MailAttachmentDto item)
    {
        using var fileStream = System.IO.File.OpenRead(item.FilePath);
        MemoryStream memStream = new();
        memStream.SetLength(fileStream.Length);
        _ = fileStream.Read(memStream.GetBuffer(), 0, (int)fileStream.Length);
        FileStorageInfo info = new()
        {
            File_Nm = $"{DateTime.Now:yyyyMMddTHHmmss}_{Path.GetFileName(item.FilePath)}",
            Folder_Path = $"{DateTime.Now.Year}/Mails/{MailCredential.EmailSource}/Attachments/{DateTime.Now:yyyyMMdd}"
        };
        return fileStorage.UploadFile(memStream, info).Url!;
    }
    [HttpGet]
    [ActionFromHeader]
    public async Task<IActionResult> GetUserList()
    {
        var userResponses = await mongoDbContext.GetAllAsync<UserListingObj>(ConstantValues.Users);
        if (userResponses.Count == 0)
            ErrorFormats.ThrowValidationException(ErrorMessages.NoDoc, ConstantValues.Users);
        return Ok(userResponses);
    }
    [HttpGet]
    [ActionFromHeader]
    public async Task<IActionResult> GetUser([FromQuery] GetUser getUser)
    {
        // Check for null or empty User_Nm
        if (string.IsNullOrEmpty(getUser.User_Nm))
        {
            ErrorFormats.ThrowValidationException(ErrorMessages.User_Nm_Required, nameof(getUser.User_Nm));
        }

        // Fetch user data from MongoDB
        var userResponses = await mongoDbContext.GetAsync<UserListingObj>(getUser.ToDictionary(), ConstantValues.Users);

        // Check if user is found
        if (userResponses == null || userResponses.Count == 0)
        {
            ErrorFormats.ThrowValidationException(ErrorMessages.NotFound, nameof(getUser.User_Nm));
        }

        // Return the first matched user
        return Ok(userResponses![0]);
    }
    [HttpPost]
    [ActionFromHeader]
    public async Task<IActionResult> PostValidateOtp(ValidateOtpModel model)
    {
        model.Actv_Ind = 1;
        ValidateOtpObj validateOtp = new() { Status = false };
        var OtpDetails = await mongoDbContext.GetAsync<PostOtpObj>(model.ToDictionary(), ConstantValues.OtpItems);
        if (OtpDetails.Count > 0)
        {
            var Otp = OtpDetails[0];
            if (Convert.ToDateTime(Otp.Crtd_Dt).ToLocalTime().AddMinutes(5) < DateTime.Now)
            {
                ErrorFormats.ThrowValidationException(ErrorMessages.Otp_Expired, nameof(model.Order_Id));
            }
            else
            {
                var updateOtpItemsDto = Configuration.AutoMapper<PostOtpObj, UpdateOtpItemsDto>(Otp);
                updateOtpItemsDto.Actv_Ind = 0;
                await mongoDbContext.UpdateAsync(updateOtpItemsDto, ConstantValues.OtpItems);
                validateOtp.Status = true;
                return Ok(validateOtp);
            }
        }
        else
        {
            ErrorFormats.ThrowValidationException(ErrorMessages.Invalid_Otp, nameof(model.Email));
        }
        return Ok(validateOtp);
    }
    [HttpPost]
    [ActionFromHeader]
    [AllowAnonymous]
    public async Task<IActionResult> PostToken([FromBody] PostToken model)
    {
        var userModel = new GetUserList() { User_Nm = model.User_Nm, Password = model.Password, Actv_Ind = ConstantValues.Actv_Ind_1 };

        //var result = await mongoDbContext.GetAsync<UserListingObj>(userModel.ToDictionary(), ConstantValues.Users);
        var appSrcModel = new GetApplicationSource() { Secret_Key = model.Secret_Key, Actv_Ind = ConstantValues.Actv_Ind_1 };
        //var applicationSourceResponse = await mongoDbContext.GetAsync<ApplicationSourceResponse>(appSrcModel.ToDictionary(), ConstantValues.ApplicationSource);
        if (true)
        {
            var Token = GenerateToken(model);
            return Ok(Token);
        }
        else
        {
            //if (result is null)
            //{
            //    ErrorFormats.ThrowValidationException(ErrorMessages.NotFound, nameof(model.User_Nm));
            //}
            //if (applicationSourceResponse is null)
            //{
            //    ErrorFormats.ThrowValidationException(ErrorMessages.NotFound, nameof(model.Application_Source));
            //}
        }
        //return Ok();
    }
    [HttpPost]
    [ActionFromHeader]
    public async Task<IActionResult> PostApplicationSource([FromBody] PostApplicationSource model)
    {
        ApplicationSourceObj sourceObj = new();
        ApplicationSourceDto applicationSourceDto = Configuration.AutoMapper<PostApplicationSource, ApplicationSourceDto>(model);
        var getApplication = new GetApplicationSource() { Secret_Key = model.Secret_Key, Actv_Ind = ConstantValues.Actv_Ind_1 };
        var result = await mongoDbContext.GetAsync<UserListingObj>(getApplication.ToDictionary(), ConstantValues.Users);
        if (result != null)
        {
            await mongoDbContext.InsertAsync(applicationSourceDto, ConstantValues.ApplicationSource);
            sourceObj.Message = "Application source created successfully";
        }
        else
        {
            ErrorFormats.ThrowValidationException(ErrorMessages.ApplicationSource_Exists, nameof(model.Secret_Key));
        }
        return Ok(sourceObj);
    }
    private static TokenResponse GenerateToken(PostToken dto)
    {
        // Define claims based on the incoming DTO (user name and application source).
        Claim[] claims =
        [
        new Claim("Id", dto.User_Nm),
        new Claim("Application_Source", dto.Application_Source)
    ];

        // Create the key using the secret stored in JwtSettings
        var signinKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSettings.SecretKey));

        // Specify the credentials (which algorithm to use for signing the token)
        var signinCredentials = new SigningCredentials(signinKey, SecurityAlgorithms.HmacSha256);

        // Define the token options (issuer, audience, claims, expiration, signing credentials)
        var tokenOptions = new JwtSecurityToken(
            issuer: JwtSettings.Issuer,
            audience: JwtSettings.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(Convert.ToDouble(JwtSettings.AccessTokenExpirationMinutes)),
            signingCredentials: signinCredentials
        );

        // Create a new token response to hold the access token
        var tokenResponse = new TokenResponse
        {
            // Generate the JWT token string
            AccessToken = new JwtSecurityTokenHandler().WriteToken(tokenOptions)
        };

        // Generate a refresh token using a cryptographic random number generator
        using (var rng = RandomNumberGenerator.Create())
        {
            var randomNumber = new byte[32];
            rng.GetBytes(randomNumber);

            // Convert the byte array to a Base64 string to use as a refresh token
            tokenResponse.RefreshToken = Convert.ToBase64String(randomNumber);
        }

        return tokenResponse;
    }

}