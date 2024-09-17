namespace CLAPi.ExcelEngine.Api.Models;

public class PostToken
{
    public string User_Type { get; set; } = null!;
    /// <summary>
    /// User Name to access
    /// </summary>
    public string User_Nm { get; set; } = null!;
    /// <summary>
    /// Password with encryption
    /// </summary>
    public string Password { get; set; } = null!;
    /// <summary>
    /// Application Source Name to validate
    /// </summary>
    public string Application_Source { get; set; } = null!;
    /// <summary>
    /// Is External Login with third party Authenication
    /// </summary>
    public bool Is_External { get; set; }
    /// <summary>
    /// Secret key to validate
    /// </summary>
    public string Secret_Key { get; set; } = null!;
}
