using AutoMapper;
using CLAPi.Core.Settings;
using Newtonsoft.Json;
using System.Globalization;
using System.Text;

namespace CLAPi.Core.GenericServices;

public static class Configuration
{
    public static DateTime DefaultDate()
    {
        return DateTime.ParseExact(string.IsNullOrEmpty(AppSettings.DefaultDate) ? DateTime.Now.ToString(ConstantValues.DateFormat) :
               Convert.ToDateTime(AppSettings.DefaultDate).ToString(ConstantValues.DateFormat), ConstantValues.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);
    }
    public static D AutoMapper<S, D>(S model)
    {
        MapperConfiguration config = new(myObj =>
        {
            myObj.CreateMap<S, D>();
        });
        var iMapper = config.CreateMapper();
        var dto = iMapper.Map<S, D>(model);
        return dto;
    }
    public static byte[] ConvertFileToByte(string File_Path)
    {
        var bytes = File.ReadAllBytes(File_Path);
        return bytes;
    }
    public static string JsonFromList(List<(string, dynamic)> dataList)
    {
        StringBuilder json = new();
        try
        {
            json.Append('{');
            for (var i = 0; i < dataList.Count; i++)
            {
                var item = dataList[i];
                json.Append($"\"{item.Item1}\": ");

                if (item.Item2 is List<(int, string, string[])> tuples)
                {
                    var resultList = tuples
                        .GroupBy(t => t.Item1)
                        .Select(group => group.ToDictionary(t => t.Item2, t => t.Item3))
                        .ToList();

                    json.Append(JsonConvert.SerializeObject(resultList, Formatting.Indented));
                }
                else if (item.Item2 is string[])
                {
                    json.Append(JsonConvert.SerializeObject(item.Item2));
                }
                else
                {
                    json.Append($"\"{item.Item2}\"");
                }

                if (i < dataList.Count - 1)
                {
                    json.Append(',');
                }
            }
            json.Append('}');
        }
        catch (Exception ex)
        {
            return $"{{\"error\": \"{ex.Message}\"}}";
        }
        return json.ToString();
    }
    public static decimal CustomRound(decimal d, int decimals)
    {
        return Math.Round(d, decimals, MidpointRounding.AwayFromZero);
    }
}