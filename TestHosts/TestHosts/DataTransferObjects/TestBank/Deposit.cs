namespace TestHosts.DataTransferObjects.TestBank
{
    using System;
    using Newtonsoft.Json;

    public class Deposit
    {
        #region Properties

        [JsonProperty("account_number")]
        public String AccountNumber { get; set; }

        [JsonProperty("amount")]
        public Decimal Amount { get; set; }

        [JsonProperty("date_time")]
        public DateTime DateTime { get; set; }

        [JsonProperty("deposit_id")]
        public Guid DepositId { get; set; }

        [JsonProperty("host_identifier")]
        public Guid HostIdentifier { get; set; }

        [JsonProperty("reference")]
        public String Reference { get; set; }

        [JsonProperty("sort_code")]
        public String SortCode { get; set; }

        #endregion
    }
}