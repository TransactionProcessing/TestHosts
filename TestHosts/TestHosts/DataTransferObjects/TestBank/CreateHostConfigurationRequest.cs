namespace TestHosts.DataTransferObjects.TestBank
{
    using System;
    using Newtonsoft.Json;

    public class CreateHostConfigurationRequest
    {
        [JsonProperty("sort_code")]
        public String SortCode { get; set; }
        [JsonProperty("account_number")]
        public String AccountNumber { get; set; }
        [JsonProperty("callback_url")]
        public String CallbackUrl { get; set; }
    }
}