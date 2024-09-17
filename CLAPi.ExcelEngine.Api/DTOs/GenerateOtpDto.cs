namespace CLAPi.ExcelEngine.Api.DTOs;

public class GenerateOtpDto : BaseDto
{
	public string Order_Id { get; set; } = null!;
	public string Message { get; set; } = null!;
	public string? Mobile_No { get; set; }
	public string OTP { get; set; } = null!;
	public string OTP_Request { get; set; }	 = null!;
	public string Error_Desc { get; set; } = null!;
	public DateTime? Sent_Dt { get; set; }
	public string? Template_Cd { get; set; }
	public string? SMS_Response { get; set; }
	public string? Email { get; set; }
	public string Flag { get; set; } = null!;
}
