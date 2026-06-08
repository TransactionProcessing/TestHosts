namespace TestHosts.SoapServices
{
    using System;
    using System.ServiceModel;
    using CoreWCF;
    using DataTransferObjects;
    using Microsoft.EntityFrameworkCore;
    using NLog.LayoutRenderers.Wrappers;

    [ServiceContract]
    public interface IPataPawaPostPayService
    {
        [OperationContract(Name = "getLoginRequest", ReplyAction = "getLoginResponse")]
        LoginResponse Login(String username,
                            String password);

        [OperationContract(Name = "getVerifyRequest", ReplyAction = "getVerifyResponse")]
        VerifyResponse VerifyAccount(String username, String api_key, String account_no);

        [OperationContract(Name = "getPayBillRequest", ReplyAction = "getPayBillResponse")]
        ProcessBillResponse ProcessBill(String username, String api_key, String account_no,
                                        String mobile_no, String customer_name,
                                        Decimal amount);
    }
}
