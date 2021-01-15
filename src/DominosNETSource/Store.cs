using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DominosNET
{
    //TODO: Add a method which lets the user get a store by its' id.
    public class Store
    {
        public Country country;
        public JObject data;
        public string id;
        public List<PaymentType> acceptedPaymentTypes { get; }

        private async Task<string> GetMenuJSONString()
        {
            if (country == Country.CA)
            {
                var httpClient = new HttpClient();
                string URL = urls.ca["menu_url"].Replace("{store_id}", id).Replace("{lang}", "en");

                var content = await httpClient.GetStringAsync(URL);
                return content;
            }
            else
            {
                var httpClient = new HttpClient();
                string URL = urls.us["menu_url"].Replace("{store_id}", id).Replace("{lang}", "en");

                var content = await httpClient.GetStringAsync(URL);
                return content;
            }
        }

        public Menu GetMenu()
        {
            JObject MenuJSON = JObject.Parse(GetMenuJSONString().Result);
            return new Menu(country, MenuJSON);
        }

        public Store(JObject data, Country country, string storeID)
        {
            this.country = country;
            this.data = data;
            this.id = storeID;
            this.acceptedPaymentTypes = GetPaymentTypes(data).ToList();
        }

        private IEnumerable<PaymentType> GetPaymentTypes(JObject data)
        {
            JArray acceptablePaymentTypes = (JArray)data["AcceptablePaymentTypes"];

            foreach (JToken token in acceptablePaymentTypes.Children())
            {
                yield return Enum.Parse<PaymentType>(token.ToString(), true);
            }
        }
    }
}
