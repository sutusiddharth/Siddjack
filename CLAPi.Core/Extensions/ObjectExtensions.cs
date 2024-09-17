using System.Reflection;

namespace CLAPi.Core.Extensions;

public static class ObjectExtensions
{
    public static Dictionary<string, object> ToDictionary(this object obj)
    {
        Dictionary<string, object> dictionary = [];
        foreach (PropertyInfo property in obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            var propertyValue = property.GetValue(obj, null);
            if (propertyValue != null)
            {
                dictionary[property.Name] = propertyValue;
            }
        }
        return dictionary;
    }
}
