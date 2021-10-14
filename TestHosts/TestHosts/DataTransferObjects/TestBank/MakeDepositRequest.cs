namespace TestHosts.DataTransferObjects.TestBank
{
    using System;
    using Newtonsoft.Json;

    public class MakeDepositRequest
    {
        [JsonProperty("date_time")]
        public DateTime DateTime { get; set; }
        [JsonProperty("from_sort_code")]
        public String FromSortCode { get; set; }
        [JsonProperty("from_account_number")]
        public String FromAccountNumber { get; set; }
        [JsonProperty("to_sort_code")]
        public String ToSortCode { get; set; }
        [JsonProperty("to_account_number")]
        public String ToAccountNumber { get; set; }
        [JsonProperty("deposit_reference")]
        public String DepositReference { get; set; }
        [JsonProperty("amount")]
        public Decimal Amount { get; set; }
    }
}