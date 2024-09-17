namespace CLAPi.ExcelEngine.Api.Models
{
    public class ValidateOtpModel
    {
        public string Flag { get; set; } = null!;
        public string OTP { get; set; } = null!;
        public string Order_Id { get; set; } = null!;
        public string? Email { get; set; }
        public string? Mobile { get; set; }
        public short? Actv_Ind { get; set; }
    }
}
