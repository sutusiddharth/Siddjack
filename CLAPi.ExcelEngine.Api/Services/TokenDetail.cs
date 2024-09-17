using System.Security.Claims;

namespace CLAPi.ExcelEngine.Api.Services;
public class TokenDetail(IHttpContextAccessor httpContextAccessor) : ITokenDetail
{
    public KeyValuePair<string, string> UserDetail()
    {
        var httpContext = httpContextAccessor.HttpContext;

        // Ensure HttpContext and User are not null
        if (httpContext?.User?.Claims == null)
        {
            throw new InvalidOperationException("No claims available in the current HttpContext.");
        }

        var claims = httpContext.User.Claims;

        return new KeyValuePair<string, string>("siddhartha.d@enoviq.com", "192.178.1.1");
    }
}
