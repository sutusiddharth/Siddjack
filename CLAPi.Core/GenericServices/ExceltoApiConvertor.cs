using CLAPi.Core.Settings;
using Newtonsoft.Json;
using Syncfusion.XlsIO;
using Syncfusion.XlsIO.Implementation;
using Syncfusion.XlsIORenderer;
using System.Collections;
using System.Dynamic;
using System.Text.RegularExpressions;

namespace CLAPi.Core.GenericServices;

public static class ExcelToApiConvertor
{
    private static string GetWorkSheetName(string valueR1C1)
    {
        return valueR1C1.Split('!')[0].Replace("'", ConstantValues.EmptyString);
    }

    private static string GetCleanedName(string name, string fieldType)
    {
        return name.Replace(fieldType, ConstantValues.EmptyString, StringComparison.InvariantCultureIgnoreCase);
    }
    private static bool HasDataValidationWithList(IRange range)
    {
        return range.HasDataValidation && range.DataValidation.FirstFormula != null && range.DataValidation.FirstFormula.Contains(ConstantValues.Excel_List, StringComparison.OrdinalIgnoreCase);
    }

    private static void AddDataValidationValues(IWorkbook workbook, NameImpl name, string fieldType, List<(string, dynamic)> dataValidationValuePairs)
    {
        var dataValidationFirstFormula = name.DataValidation?.FirstFormula;
        if (string.IsNullOrEmpty(dataValidationFirstFormula)) return;

        var dataValidationName = (NameImpl)workbook.Names[dataValidationFirstFormula];
        var workSheetName = GetWorkSheetName(dataValidationName.ValueR1C1);
        var dataRange = workbook.Worksheets[workSheetName][dataValidationName.RefersToRange.AddressLocal];

        dataValidationValuePairs.Add((GetCleanedName(name.Name, fieldType), new[]
        {
        dataValidationFirstFormula,
        JsonConvert.SerializeObject(dataRange.Select(a => a.DisplayText))
    }));
    }

    //Get All the fields with type
    public static List<(string, dynamic)> GetFieldWithType(IWorkbook workbook, string fieldType, ref List<(string, dynamic)> dataValidationValuePairs)
    {
        var keyValuePairs = new List<(string, dynamic)>();
        const string pattern = @"[\s\.\W]";
        var regex = new Regex(pattern, RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));

        foreach (var name in workbook.Names.Cast<NameImpl>().Where(n => IsValidName(n, fieldType)))
        {
            var workSheetName = GetWorkSheetName(name.ValueR1C1);
            if (!workbook.Worksheets.Any(a => a.Name.Contains(workSheetName)))
                continue;

            var range = workbook.Worksheets[workSheetName][name.RefersToRange.AddressLocal];


            if (regex.IsMatch(name.Name)) continue;

            if (IsRangeMultipleCells(name.RefersToRange.AddressLocal))
            {
                keyValuePairs.Add((GetCleanedName(name.Name, fieldType), ProcessTableRange(range, workSheetName, regex)));
            }
            else
            {
                keyValuePairs.Add((GetCleanedName(name.Name, fieldType), ProcessSingleRange(range, workSheetName, name.RefersToRange.AddressLocal)));

                if (HasDataValidationWithList(range))
                {
                    AddDataValidationValues(workbook, name, fieldType, dataValidationValuePairs);
                }
            }
        }

        return keyValuePairs;
    }

    public static List<(string, dynamic)> GetFieldWithType(IWorkbook workbook, IWorksheet worksheet, string fieldType)
    {
        var keyValuePairs = new List<(string, dynamic)>();
        const string pattern = @"[\s\.\W]";
        var regex = new Regex(pattern, RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));
        var workSheetName = worksheet.Name;

        foreach (var name in workbook.Names.Cast<NameImpl>().Where(n => IsValidName(n, workSheetName, fieldType)))
        {
            if (name.RefersToRange == null || regex.IsMatch(name.Name)) continue;

            var range = worksheet[name.RefersToRange.AddressLocal];

            if (IsRangeMultipleCells(name.RefersToRange.AddressLocal))
            {
                ////if (name.Formula != null && name.Formula.Contains("Offset", StringComparison.OrdinalIgnoreCase))
                ////{

                ////}
                keyValuePairs.Add((GetCleanedName(name.Name, fieldType), ProcessTableRange(range, regex, workSheetName)));
            }
            else
            {
                keyValuePairs.Add((GetCleanedName(name.Name, fieldType), ProcessSingleRange(range, workSheetName, name.RefersToRange.AddressLocal)));
            }
        }

        return keyValuePairs;
    }

    private static bool IsValidName(NameImpl name, string fieldType)
    {
        return name.Name.Contains(fieldType, StringComparison.InvariantCultureIgnoreCase) && name.RefersToRange != null;
    }
    private static bool IsValidName(NameImpl name, string worksheetName, string fieldType)
    {
        return name.ValueR1C1 != null &&
               name.ValueR1C1.Contains(worksheetName) &&
               name.Name.Contains(fieldType, StringComparison.InvariantCultureIgnoreCase);
    }

    private static bool IsRangeMultipleCells(string address)
    {
        return address.Contains(':');
    }

    private static List<(int, string, string[])> ProcessTableRange(IRange range, string workSheetName, Regex regex)
    {
        var tableKeyPairs = new List<(int, string, string[])>();
        var minRow = range.Min(a => a.Row);
        var headerCells = range.Where(a => a.Row == minRow).ToList();

        foreach (var cell in range.Where(a => a.Row != minRow))
        {
            var header = headerCells.Find(h => h.Column == cell.Column);
            if (header != null && !string.IsNullOrEmpty(header.DisplayText) && !regex.IsMatch(header.DisplayText))
            {
                tableKeyPairs.Add((cell.Row, header.DisplayText, new[]
                {
                GetDataType(cell),
                GetValue(cell),
                workSheetName,
                cell.AddressLocal,
                cell.HasDataValidation.ToString()
            }));
            }
        }

        return tableKeyPairs;
    }
    private static List<(int, string, string[])> ProcessTableRange(IRange range, Regex regex, string worksheetName)
    {
        var tableKeyPairs = new List<(int, string, string[])>();
        var minRow = range.Min(a => a.Row);
        var headerCells = range.Where(a => a.Row == minRow).ToList();

        foreach (var cell in range.Where(a => a.Row != minRow))
        {
            var header = headerCells.Find(h => h.Column == cell.Column);
            if (header != null && !string.IsNullOrEmpty(header.DisplayText) && !regex.IsMatch(header.DisplayText))
            {
                tableKeyPairs.Add((
                    cell.Row,
                    header.DisplayText,
                    new[]
                    {
                    GetDataType(cell),
                    GetValue(cell),
                    worksheetName,
                    cell.AddressLocal,
                    cell.HasDataValidation.ToString()
                    }
                ));
            }
        }

        return tableKeyPairs;
    }

    private static dynamic ProcessSingleRange(IRange range, string worksheetName, string addressLocal)
    {
        var dataValidationFirstFormula = range.DataValidation?.FirstFormula ?? string.Empty;
        var hasDataFile = range.HasDataValidation && dataValidationFirstFormula.Contains(ConstantValues.Excel_List, StringComparison.OrdinalIgnoreCase);

        return new[]
        {
        GetDataType(range),
        GetValue(range),
        worksheetName,
        addressLocal,
        hasDataFile.ToString()
    };
    }
    //mapping input ExpandoObjects with excel
    public static void MapClassToExcel(IDictionary<string, dynamic> modelData, IWorkbook workbook)
    {
        MapDynamicToExcel(modelData, workbook);
    }
    private static void MapDynamicToExcel(dynamic dynamicData, IWorkbook workbook)
    {
        switch (dynamicData)
        {
            case KeyValuePair<string, dynamic> keypair:
                MapDynamicToExcel(keypair.Value, workbook);
                break;

            case IEnumerable enumerable when enumerable.GetEnumerator().MoveNext():
                var array = enumerable.Cast<dynamic>().ToArray();
                if (array[0] is string)
                {
                    workbook.Worksheets[array[2]][array[3]].Value = array[1];
                }
                else
                {
                    foreach (var item in array)
                    {
                        MapDynamicToExcel(item, workbook);
                    }
                }
                break;
        }
    }
    //CRUD operations with excel
    ////public static string CallExcelEngine<T>(T request, MemoryStream fileStream, string IsApi, ApiService apiService,ISecretsManagerService secretsManager) where T : class
    public static string CallExcelEngine<T>(T request, MemoryStream fileStream) where T : class
    {
        var outputString = string.Empty;
        using (ExcelEngine excelEngine = new())
        {
            var application = excelEngine.Excel;
            application.DefaultVersion = ExcelVersion.Xlsx;
            var workbook = application.Workbooks.Open(fileStream, ExcelOpenType.Automatic);

            try
            {
                List<(string, dynamic)> outputData = [];

                MapClassToExcel((IDictionary<string, dynamic>)request, workbook);

                foreach (var sheet in workbook.Worksheets)
                {
                    sheet.EnableSheetCalculations();
                    outputData.AddRange(GetFieldWithType(workbook, sheet, ConstantValues.Excel_Output));
                    sheet.DisableSheetCalculations();
                }
                outputString = Configuration.JsonFromList(outputData);

                ////if (IsApi == "Yes")
                ////{
                ////    string workSheetName = "ApiCall";
                ////    string FieldType = "EQApi_";
                ////    var pattern = @"[\s\.\W]";
                ////    var regex = new Regex(pattern, RegexOptions.Compiled);

                ////    Dictionary<string, string> inputFields = [];
                ////    Dictionary<string, string> apiNames = [];
                ////    Dictionary<string, string> apiSequences = [];
                ////    Dictionary<string, string> tokenInputFields = [];
                ////    Dictionary<string, string> endPoints = [];
                ////    Dictionary<string, string> httpMethods = [];
                ////    Dictionary<string, string> tokenRequireds = [];

                ////    foreach (NameImpl name in workbook.Names.Cast<NameImpl>().Where(a => a.ValueR1C1 != null && a.ValueR1C1.Contains(workSheetName)))
                ////    {
                ////        if (!name.Name.Contains(FieldType, StringComparison.InvariantCultureIgnoreCase) || name.RefersToRange == null)
                ////            continue;
                ////        IRange range = workbook.Worksheets[workSheetName][name.RefersToRange.AddressLocal];

                ////        if (regex.IsMatch(name.Name))
                ////            continue;
                ////        string key = name.Name.Replace(FieldType, ConstantValues.EmptyString, StringComparison.InvariantCultureIgnoreCase);
                ////        string value = GetValue(range);
                ////        if (key.StartsWith("Name"))
                ////        {
                ////            apiNames.Add(key, value);
                ////        }
                ////        else if (key.StartsWith("Sequence"))
                ////        {
                ////            apiSequences.Add(key, value);
                ////        }
                ////        else if (key.StartsWith("TI_"))
                ////        {
                ////            tokenInputFields[key.Replace("TI_", ConstantValues.EmptyString, StringComparison.InvariantCultureIgnoreCase)] = value;
                ////        }
                ////        else if (key.StartsWith("EndPoint"))
                ////        {
                ////            endPoints[key] = value;
                ////        }
                ////        else if (key.StartsWith("Http_Method"))
                ////        {
                ////            httpMethods[key] = value;
                ////        }
                ////        else if (key.StartsWith("TokenRequired"))
                ////        {
                ////            tokenRequireds[key] = value;
                ////        }
                ////        else
                ////        {
                ////            inputFields[key] = value;
                ////        }
                ////    }
                ////    foreach (var item in apiNames)
                ////    {
                ////        string Token = string.Empty;
                ////        if (tokenRequireds.Keys.Contains(item.Value))
                ////        {

                ////        }
                ////        var d=apiService.PostAsync<Dictionary<string,string>,string>(item.Value, tokenInputFields);
                ////    }
                ////}
                workbook.Close();
                fileStream.Close();
                return outputString;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                workbook.Close();
            }
        }
        return outputString;
    }
    public static string GetChangedVersioned(string version, string versionChangeType)
    {

        // Regex pattern to match version _V1.0.0
        var pattern = @"V(\d+)\.(\d+)\.(\d+)$";
        var match = Regex.Match(version, pattern, RegexOptions.None, TimeSpan.FromMilliseconds(100));

        int majorVersion;
        int minorVersion;
        int patchVersion;

        if (match.Success)
        {
            // Extract current versions
            majorVersion = int.Parse(match.Groups[1].Value);
            minorVersion = int.Parse(match.Groups[2].Value);
            patchVersion = int.Parse(match.Groups[3].Value);

            // Determine which version to increment
            switch (versionChangeType.ToLower())
            {
                case "major":
                    majorVersion++;
                    minorVersion = 0; // Reset minor and patch versions on major increment
                    patchVersion = 0;
                    break;
                case "minor":
                    minorVersion++;
                    patchVersion = 0; // Reset patch version on minor increment
                    break;
                case "patch":
                    patchVersion++;
                    break;
                default:
                    throw new ArgumentException("Invalid version change type. Use 'major', 'minor', or 'patch'.");
            }

            // Form new version string
            return $"V{majorVersion}.{minorVersion}.{patchVersion}";
        }
        else
        {
            // If no version found, start with 1.0.0 and handle version change accordingly
            return versionChangeType.ToLower() switch
            {
                "major" => $"V1.0.0",
                "minor" => $"V0.1.0",
                "patch" => $"V0.0.1",
                _ => throw new ArgumentException("Invalid version change type. Use 'major', 'minor', or 'patch'."),
            };
        }
    }
    public static ExpandoObject? DeserializeExpando(string json)
    {
        try
        {
            return JsonConvert.DeserializeObject<ExpandoObject>(json);
        }
        catch
        {
            return new ExpandoObject();
        }
    }
    private static string GetDataType(IRange range)
    {
        if (range.HasDateTime)
        {
            return "DateTime";
        }
        else if (range.HasNumber)
        {
            return "Number";
        }
        else
        {
            return "String";
        }
    }
    private static string GetValue(IRange range)
    {
        if (range.CalculatedValue != null)
        {
            return range.CalculatedValue;
        }
        else if (range.DisplayText != null)
        {
            return range.DisplayText;
        }
        else
        {
            return range.Value;
        }
    }

    public static List<Tuple<byte[], string>> CallDocExcelEngine(dynamic request, dynamic repsonse, MemoryStream fileStream, string basePath)
    {
        List<Tuple<byte[], string>> pdfFileStrams = [];
        using (ExcelEngine excelEngine = new())
        {
            var application = excelEngine.Excel;
            application.DefaultVersion = ExcelVersion.Xlsx;
            var workbook = application.Workbooks.Open(fileStream, ExcelOpenType.Automatic);

            try
            {
                MapClassToExcel((IDictionary<string, dynamic>)request, workbook);
                MapClassToExcel((IDictionary<string, dynamic>)repsonse, workbook);

                foreach (var worksheet in workbook.Worksheets.Where(a => a.Name.Contains("Print")))
                {
                    //Initialize XlsIO renderer.
                    XlsIORenderer renderer = new();
                    //Convert Excel document into PDF document
                    var pdfDocument = renderer.ConvertToPDF(worksheet);
                    var pdfName = $"{worksheet.Name.Replace("Print", ConstantValues.EmptyString, StringComparison.OrdinalIgnoreCase)}.pdf";
                    var pdfPath = $"{basePath}/{pdfName}";

                    using (Stream stream = new FileStream(pdfPath, FileMode.Create, FileAccess.ReadWrite))
                    {
                        pdfDocument.Save(stream);
                    }
                    pdfFileStrams.Add(new Tuple<byte[], string>(Configuration.ConvertFileToByte(pdfPath), pdfName));
                }
                workbook.Close();
                fileStream.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                workbook.Close();
            }
        }
        return pdfFileStrams;
    }
}