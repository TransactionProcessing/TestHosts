namespace TestHosts.Controllers{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Metrics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Database.PataPawa;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using Shared.General;

    [Route("api/patapawaprepay")]
    [ApiController]
    public class PataPawaPrePaidController : ControllerBase{
        #region Fields

        private readonly Func<String, PataPawaContext> ContextResolver;

        private List<(String username, String password, String key, String balance)> users = new();

        #endregion

        #region Constructors

        public PataPawaPrePaidController(Func<String, PataPawaContext> contextResolver){
            this.ContextResolver = contextResolver;
        }

        #endregion

        #region Methods

        [HttpPost]
        public async Task<IActionResult> SingleFunction([FromForm] String request, CancellationToken cancellationToken){
            RequestType xlatedRequestType = this.TranslateRequestType(request);

            IActionResult response = xlatedRequestType switch{
                RequestType.login => await this.HandleLoginRequest(this.Request.Form, cancellationToken),
                RequestType.meter => await this.HandleMeterRequest(this.Request.Form, cancellationToken),
                RequestType.vend => await this.HandleVendRequest(this.Request.Form, cancellationToken),
                RequestType.balance => await this.HandleBalanceRequest(this.Request.Form, cancellationToken),
                RequestType.lastvendfull => await HandlelastVendRequest(xlatedRequestType, Request.Form, cancellationToken),
                RequestType.lastvendfullfail => await HandlelastVendRequest(xlatedRequestType, Request.Form, cancellationToken),
                // TODO: last vend and last vend full 
                _ => this.BadRequest($"Request type {request} not supported.")
            };

            return response;
        }

        private async Task<IActionResult> HandlelastVendRequest(RequestType xlatedRequestType, IFormCollection requestForm, CancellationToken cancellationToken){

            String username = requestForm["username"].ToString();
            String key = requestForm["key"].ToString();
            String meter = requestForm["meter"].ToString();

            (PrePayMeter meterDetails, IActionResult result) meterValidation = await this.ValidateMeterDetails(meter, cancellationToken);
            if (meterValidation.result != null)
                return meterValidation.result;

            PataPawaContext context = this.GetPataPawaContext();
            Database.PataPawa.Transaction transaction = xlatedRequestType switch{
                RequestType.lastvendfull => await context.Transactions.Where(t => t.MeterNumber == meter && t.Status == 0).OrderByDescending(t => t.Date).SingleOrDefaultAsync(cancellationToken),
                _ => await context.Transactions.Where(t => t.MeterNumber == meter && t.Status == 0).OrderByDescending(t => t.Date).SingleOrDefaultAsync(cancellationToken)
            };

            if (transaction == null)
            {
                var response = new
                               {
                                   status = 0,
                                   msg = "Record not found"
                               };
                return this.Ok(response);
            }
            else
            {
                var response = new
                               {
                                   status = transaction.Status,
                                   msg = transaction.Messaage,
                                   transaction = new Transaction
                                                 {
                                                     transactionId = transaction.TransactionId,
                                                     status = transaction.Status,
                                                     vendor = transaction.Vendor,
                                                     meterNo = transaction.MeterNumber,
                                                     rescode = transaction.ResultCode,
                                                     stdTokenAmt = transaction.StandardTokenAmt,
                                                     stdTokenTax = transaction.StandardTokenTax.ToString(),
                                                     units = transaction.Units.ToString(),
                                                     token = transaction.Token,
                                                     stdTokenRctNum = transaction.StandardTokenRctNum,
                                                     date = transaction.Date.ToString("yyyy-MM-dd hh:mm:ss"),
                                                     totalAmount = transaction.TotalAmount.ToString(),
                                                     customerName = transaction.CustomerName,
                                                     reference = transaction.Reference,
                                                     Charges = new List<Fixed>()
                                                 }
                               };
                return this.Ok(response);
            }
        }

        private Database.PataPawa.Transaction CreateTransactionRecord(String amount, PrePayMeter meter){
            Database.PataPawa.Transaction CreatePendingTransaction(){
                return new Database.PataPawa.Transaction{
                                                            Date = DateTime.Now,
                                                            Status = 0,
                                                            Messaage = "transaction still pending",
                                                            IsPending = true,
                                                            MeterNumber = meter.MeterNumber
                };
            }

            Database.PataPawa.Transaction CreateTimeBasedFailedTransaction(){
                return new Database.PataPawa.Transaction{
                                                            Date = DateTime.Now,
                                                            Status = 2,
                                                            Messaage = "MTRFE013-M1: There is an insufficient amount (676.77) for the time based \\r\\ncharges (3132). Either tender more money or request less units.",
                                                            IsPending = false
                };
            }

            Database.PataPawa.Transaction CreateInsufficientFloatFailedTransaction(){
                return new Database.PataPawa.Transaction{
                                                            Date = DateTime.Now,
                                                            Status = 2,
                                                            Messaage = "MTRFE013-M1: There is an insufficient amount (676.77) for the time based \\r\\ncharges (3132). Either tender more money or request less units.",
                                                            IsPending = false
                };
            }

            Database.PataPawa.Transaction CreateTooSoonFailedTransaction(){
                return new Database.PataPawa.Transaction{
                                                            Date = DateTime.Now,
                                                            Status = 1,
                                                            Messaage = "This transaction has been done too soon from the last one. Please wait for a \\r\\nfew minutes.",
                                                            IsPending = false
                };
            }

            Database.PataPawa.Transaction CreateSuccessfulTransaction(){
                return new Database.PataPawa.Transaction{
                                                            Status = 0,
                                                            Messaage = "success",
                                                            Vendor = "support",
                                                            MeterNumber = meter.MeterNumber,
                                                            ResultCode = "elec000",
                                                            StandardTokenAmt = 64,
                                                            StandardTokenTax = 0,
                                                            Units = 6.1m,
                                                            Token = Guid.NewGuid().ToString("N"),
                                                            StandardTokenRctNum = "Ce001OVS3709952",
                                                            Date = DateTime.Now,
                    TotalAmount = 400,
                                                            Charges = new List<TransactionCharge>{
                                                                                                     new TransactionCharge{
                                                                                                                              ERCCharge = 3.19m,
                                                                                                                              ForexCharge = 0.47m,
                                                                                                                              FuelIndexCharge = 2.47m,
                                                                                                                              InflationAdjustment = 0,
                                                                                                                              MonthlyFC = 13.27m,
                                                                                                                              REPCharge = 1.39m,
                                                                                                                              TotalTax = 15.21m
                                                                                                                          }
                                                                                                 },
                                                            CustomerName = meter.CustomerName,
                                                            Reference = DateTime.Now.ToString("yyyyMMddhhmmsssfff"),
                                                            IsPending = false
                };
            }

            Database.PataPawa.Transaction transaction = amount switch{
                "150" => CreatePendingTransaction(),
                "999" => CreateTimeBasedFailedTransaction(),
                "888" => CreateInsufficientFloatFailedTransaction(),
                "777" => CreateTooSoonFailedTransaction(),
                _ => CreateSuccessfulTransaction()
            };

            return transaction;
        }

        private PataPawaContext GetPataPawaContext(){
            String connectionString = ConfigurationReader.GetConnectionString("PataPawaReadModel");
            PataPawaContext context = this.ContextResolver(connectionString);
            return context;
        }

        private async Task<IActionResult> HandleBalanceRequest(IFormCollection requestForm, CancellationToken cancellationToken){
            String username = requestForm["username"].ToString();
            String key = requestForm["key"].ToString();
            String meter = requestForm["meter"].ToString();

            PataPawaContext context = this.GetPataPawaContext();

            PrePayUser user = await context.PrePayUsers.SingleOrDefaultAsync(u => u.UserName == username && u.Key == key, cancellationToken);

            var response = new{
                                  status = 0,
                                  msg = "success",
                                  balance = user.Balance.ToString(),
                              };
            return this.Ok(response);
        }

        private async Task<IActionResult> HandleLoginRequest(IFormCollection requestForm, CancellationToken cancellationToken){
            String username = requestForm["username"].ToString();
            String password = requestForm["password"].ToString();

            PataPawaContext context = this.GetPataPawaContext();

            PrePayUser user = await context.PrePayUsers.SingleOrDefaultAsync(u => u.UserName == username && u.Password == password, cancellationToken);

            if (user == default){
                var errorReponse = new{
                                          status = 1,
                                          msg = "Wrong Username or Password"
                                      };
                return this.Ok(errorReponse);
            }

            var response = new{
                                  status = 0,
                                  msg = "success",
                                  balance = user.Balance,
                                  key = user.Key,
                              };
            return this.Ok(response);
        }

        private async Task<IActionResult> HandleMeterRequest(IFormCollection requestForm, CancellationToken cancellationToken){
            String username = requestForm["username"].ToString();
            String meter = requestForm["meter"].ToString();
            String key = requestForm["key"].ToString();

            (PrePayMeter meterDetails, IActionResult result) meterValidation = await this.ValidateMeterDetails(meter, cancellationToken);
            if (meterValidation.result != null)
                return meterValidation.result;

            var response = new{
                                  status = 0,
                                  msg = "success",
                                  customerName = meterValidation.meterDetails.CustomerName
                              };
            return this.Ok(response);
        }

        private async Task<IActionResult> HandleVendRequest(IFormCollection requestForm, CancellationToken cancellationToken){
            String username = requestForm["username"].ToString();
            String meter = requestForm["meter"].ToString();
            String key = requestForm["key"].ToString();
            String amount = requestForm["amount"].ToString();
            String customerName = requestForm["customerName"].ToString();

            (PrePayMeter meterDetails, IActionResult result) meterValidation = await this.ValidateMeterDetails(meter, cancellationToken);
            if (meterValidation.result != null)
                return meterValidation.result;

            PataPawaContext context = this.GetPataPawaContext();

            Database.PataPawa.Transaction transaction = this.CreateTransactionRecord(amount, meterValidation.meterDetails);

            await context.Transactions.AddAsync(transaction, cancellationToken);
            if (transaction.Charges != null){
                await context.TransactionCharges.AddRangeAsync(transaction.Charges, cancellationToken);
            }

            await context.SaveChangesAsync(cancellationToken);

            // Now build the response object
            var response = new{
                                  status = transaction.Status,
                                  msg = transaction.Messaage,
                                  transaction = new Transaction{
                                                                   transactionId = transaction.TransactionId,
                                                                   status = transaction.Status,
                                                                   vendor = transaction.Vendor,
                                                                   meterNo = transaction.MeterNumber,
                                                                   rescode = transaction.ResultCode,
                                                                   stdTokenAmt = transaction.StandardTokenAmt,
                                                                   stdTokenTax = transaction.StandardTokenTax.ToString(),
                                                                   units = transaction.Units.ToString(),
                                                                   token = transaction.Token,
                                                                   stdTokenRctNum = transaction.StandardTokenRctNum,
                                                                   date = transaction.Date.ToString("yyyy-MM-dd hh:mm:ss"),
                                                                   totalAmount = transaction.TotalAmount.ToString(),
                                                                   customerName = transaction.CustomerName,
                                                                   reference = transaction.Reference,
                                                                   Charges = new List<Fixed>()
                                                               }
                              };

            if (transaction.Charges != null){
                foreach (TransactionCharge transactionCharge in transaction.Charges){
                    response.transaction.Charges.Add(new Fixed{
                                                                  ForexCharge = transactionCharge.ForexCharge,
                                                                  ERCCharge = transactionCharge.ERCCharge,
                                                                  FuelIndexCharge = transactionCharge.FuelIndexCharge,
                                                                  InflationAdjustment = transactionCharge.InflationAdjustment,
                                                                  MonthlyFC = transactionCharge.MonthlyFC,
                                                                  REPCharge = transactionCharge.REPCharge,
                                                                  TotalTax = transactionCharge.TotalTax,
                                                              });
                }
            }

            return this.Ok(response);
        }

        private RequestType TranslateRequestType(String formRequest){
            // get the values from the form
            return formRequest switch{
                "login" => RequestType.login,
                "meter" => RequestType.meter,
                "vend" => RequestType.vend,
                "balance" => RequestType.balance,
                "last-vend-full" => RequestType.lastvendfull,
                "last-vend-full-fail" => RequestType.lastvendfullfail,

                _ => RequestType.unknownrequestype
            };
        }

        private async Task<(PrePayMeter meterDetails, IActionResult result)> ValidateMeterDetails(String meterNumber, CancellationToken cancellationToken){
            if (meterNumber == "01234567890"){
                return (null, this.Ok(new{
                                             status = 1,
                                             msg = "Request timed out. please fetch the response again",
                                             code = "elec100"
                                         }));
            }

            if (meterNumber == "01234567891"){
                return (null, this.Ok(new{
                                             status = 1,
                                             msg = "Kenya Power link down, repeat the transaction after sometime",
                                             code = "elec100"
                                         }));
            }

            PataPawaContext context = this.GetPataPawaContext();

            PrePayMeter meterDetails = await context.PrePayMeters.SingleOrDefaultAsync(m => m.MeterNumber == meterNumber, cancellationToken);

            if (meterDetails == default){
                var errorReponse = new{
                                          status = 1,
                                          msg = "Meter number not found"
                                      };
                return (null, this.Ok(errorReponse));
            }

            return (meterDetails, null);
        }

        #endregion

        #region Others

        private enum RequestType{
            unknownrequestype,

            login,

            meter,

            vend,
            
            balance,

            lastvendfull,

            lastvendfullfail
        }

        #endregion
    }

    public class Fixed{
        #region Properties

        public Decimal ERCCharge{ get; set; }
        public Decimal ForexCharge{ get; set; }
        public Decimal FuelIndexCharge{ get; set; }
        public Int32 InflationAdjustment{ get; set; }
        public Decimal MonthlyFC{ get; set; }
        public Decimal REPCharge{ get; set; }
        public Decimal TotalTax{ get; set; }

        #endregion
    }

    public class Transaction{
        #region Properties

        [JsonProperty("fixed")]
        public List<Fixed> Charges{ get; set; }
        public String customerName{ get; set; }
        public String date{ get; set; }
        public String meterNo{ get; set; }
        public String msg{ get; set; }
        [JsonProperty("ref")]
        public String reference{ get; set; }
        public String rescode{ get; set; }
        public Int32 status{ get; set; }
        public Decimal stdTokenAmt{ get; set; }
        public String stdTokenRctNum{ get; set; }
        public String stdTokenTax{ get; set; }
        public String token{ get; set; }
        public String totalAmount{ get; set; }
        public Int32 transactionId{ get; set; }
        public String units{ get; set; }
        public String vendor{ get; set; }

        #endregion
    }
}