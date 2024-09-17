using System.Net.Mail;

namespace CLAPi.ExcelEngine.Api.DTOs;

public class MailItemsDto
{
    public string? MailFrom { get; set; }
    public string? Alias { get; set; }
    public string? MailSource { get; set; }
    public List<MailAddressDto>? MailTo { get; set; }
    public List<MailAddressDto>? MailCc { get; set; }
    public List<MailAddressDto>? MailBcc { get; set; }
    public string? MailSubject { get; set; }
    public string? MailBody { get; set; }
    public List<MailAttachmentDto>? MailAttachment { get; set; }
    public DateTime? SentDt { get; set; }
    public string? ErrorDesc { get; set; }
    public short? AttachmentInd { get; set; }
    public short? ActiveInd { get; set; }
    public MailPriority MailPriority { get; set; }
    public string? To { get; set; }
    public string? Cc { get; set; }
    public string? Bcc { get; set; }
    public string? Attachment { get; set; }
    public bool IsPending { get; set; } = false;
    public string? EmailResponse { get; set; }
    public string? RefType { get; set; }
    public string? RefNo { get; set; }
    public string? Flag { get; set; }
}
public class MailAddressDto
{
    public string MailAddress { get; set; } = null!;
}
public class MailAttachmentDto
{
    public string FilePath { get; set; } = null!;
    public string FileNm { get; set; } = null!;
}
