using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DominosNET
{
    // TODO: Add a method which lets the user get a store by its' id.
    public class Store
    {
        public Country country;
        public Address address;
        public JObject data;
        public decimal distance;
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
            JObject menuJSON = JObject.Parse(GetMenuJSONString().Result);
            return new Menu(country, menuJSON);
        }

        public Store(JObject data, Country country, string storeID, Address address, decimal distance)
        {
            this.country = country;
            this.data = data;
            this.id = storeID;
            this.address = address;
            this.acceptedPaymentTypes = GetPaymentTypes(data).ToList();
            this.distance = distance;
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

    public readonly struct StoreInfo
    {
        public readonly Store store;
        public readonly bool isOpen;
        public readonly List<ServiceType> openServiceTypes;

        public StoreInfo(Store store, bool isOpen, List<ServiceType> openServiceTypes)
        {
            this.store = store;
            this.isOpen = isOpen;
            this.openServiceTypes = openServiceTypes;
        }
    }
}
