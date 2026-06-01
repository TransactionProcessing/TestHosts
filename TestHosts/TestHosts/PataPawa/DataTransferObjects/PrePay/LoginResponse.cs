namespace TestHosts.PataPawa.DataTransferObjects.PrePay;

public class LoginResponse : BaseResponse
{
    public string balance { get; set; }
    public string key { get; set; }
}