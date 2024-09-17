using FluentValidation.Results;
using ServiceDefaults.Exceptions;

namespace CLAPi.Core.GenericServices;

public static class ErrorFormats
{
    public static List<ValidationFailure> ValidationError(string errorMessage, string propertyName)
    {
        List<ValidationFailure> list = [];
        ValidationFailure item = new()
        {
            PropertyName = propertyName,
            ErrorMessage = errorMessage
        };
        list.Add(item);
        return list;
    }

    public static void ThrowValidationException(string errorMessage, string propertyName)
    {
        List<ValidationFailure> list = [];
        ValidationFailure item = new()
        {
            PropertyName = propertyName,
            ErrorMessage = errorMessage
        };
        list.Add(item);
        throw new DataValidationException(list);
    }
}