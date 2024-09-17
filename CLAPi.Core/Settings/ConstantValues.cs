﻿namespace CLAPi.Core.Settings;
public static class ConstantValues
{
	public const string AESKey = "a244bc2be4245c022748235a46dedf15";
	public const string InitialisationVector = "26744a68b53dd87bb395584c00f7290a";
	public const string LicenceKey = "agHJHTwsuwnsodqksbrNTPSoSHifuEifC8SD1vA7";
    public static readonly string DateFormat = string.IsNullOrEmpty(AppSettings.DefaultDate) ? "dd-MM-yyyy HH:mm:ss" : $"{AppSettings.DefaultDate} HH:mm:ss";
	public const string DefaultTime = "T00:00:00";
	public const string ShortFormat = @"^([01]?[0-9]?[0-9]|2[0-4][0-9]|25[0-5])$";
	public const string IntFormat = @"^[+-]?\d{1,9}$";
	public const string LongFormat = @"^[+-]?\d+$";
	public const string DecimalFormat = @"^[+-]?(\d*\.)?\d+$";
	public const string GuidPattern = @"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$";
	public readonly static string[] ExcelExtentsions = [".xlsx"];
	public const short Actv_Ind_1 = 1;
	public const short Decimal_Place_2 = 2;
	public const int Year = 15;
	public const int Hour = 5;
	public const string Email = "Email";
	public const string OTP = "OTP";
	public const string Sys_Name = "CLAPi";
	public const string TemplateFile = "TemplateFile";
	public const string Users = "Users";
	public const string OtpItems = "OtpItems";
	public const string escalator = "escalator";
	public const string Excel = "Excel";
	public const string TempDoc = "TempDoc";
	public const string InitialVersion = "V1.0.0";
	public const string wwwroot = "wwwroot";
	public const string Excel_Input = "XInput_";
	public const string Excel_Output = "XOutput_";
    public const string Excel_List = "lst";
    public const string Print= "Print";
	public const string TemplateDocument = "TemplateDocument";
	public const string TemplateTransactionHistory = "TemplateTransactionHistory";
	public const string TemplateFileHistory = "TemplateFileHistory";
	public const string Pdf = "Pdf";
	public const string Patch = "Patch";
	public const string TemplateAccessHistory = "TemplateAccessHistory";
	public const string EmptyString = "";
	public const string BusinessSubType = "BusinessSubType";
	public const string BusinessType = "BusinessType";
	public const string ApplicationSource = "ApplicationSource";
}