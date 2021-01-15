using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DominosNET
{
    public enum ServiceType
    {
        Delivery,
        Carryout,
        DriveUpCarryout
    }

    public enum Country
    {
        US,
        CA
    }

    /// <summary>
    /// The class for the user's address. NOTE: If you live in Canada, region is your province/territory, and zip is your postal code.
    /// </summary>

    public class Address
    {

        [Serializable]
        private class StoreNotFoundException : Exception
        {

            public StoreNotFoundException() { }
            public StoreNotFoundException(string message) : base(message) { }
            public StoreNotFoundException(string message, Exception inner) : base(message, inner) { }
            protected StoreNotFoundException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }

        public string street;
        public string city;
        public string region; //state or province
        public string zip; //This can be your postal code if you live in canada
        public Country country;
        public ServiceType serviceType;

        public Address(string street, string city, string region, string zip, Country country, ServiceType serviceType)
        {
            this.street = street;
            this.city = city;
            this.region = region;
            this.zip = zip;
            this.country = country;
            this.serviceType = serviceType;
        }

        public Store GetClosestStore()
        {
            Store closestStore = null;

            async Task<String> GetJSON()
            {
                if (country == Country.CA)
                {
                    var httpClient = new HttpClient();
                    string URL = urls.ca["find_url"].Replace("{line1}", street).Replace("{line2}", city + ", " + region + ", " + zip).Replace("{type}", serviceType.ToString());

                    var content = await httpClient.GetStringAsync(URL);
                    return content;
                }
                else
                {
                    var httpClient = new HttpClient();
                    string URL = urls.us["find_url"].Replace("{line1}", street).Replace("{line2}", city + ", " + region + ", " + zip).Replace("{type}", serviceType.ToString());

                    var content = await httpClient.GetStringAsync(URL);
                    return content;
                }
            }

            async Task<String> GetStoreInfo(string storeId)
            {
                if (country == Country.CA)
                {
                    var httpClient = new HttpClient();
                    string URL = urls.ca["info_url"].Replace("{store_id}", storeId);

                    var content = await httpClient.GetStringAsync(URL);
                    return content;

                }
                else
                {

                    var httpClient = new HttpClient();
                    string URL = urls.us["info_url"].Replace("{store_id}", storeId);


                    var content = await httpClient.GetStringAsync(URL);

                    return content;

                }
            }

            void SetStoreClass()
            {
                JObject json = JObject.Parse(GetJSON().Result);
                JArray stores = JArray.Parse(json["Stores"].ToString());
                foreach (JObject store in stores.Children())
                {
                    if (store["IsOnlineNow"].ToObject<bool>() && store["ServiceIsOpen"][serviceType.ToString()].ToObject<bool>())
                    {
                        closestStore = new Store(JObject.Parse(GetStoreInfo(store["StoreID"].ToString()).Result), country, store["StoreID"].ToString());
                        break;
                    }
                }
            }

            SetStoreClass();
            if (closestStore == null)
            {
                throw new StoreNotFoundException("Error: No stores nearby are currently open. Try using another service method (e.g ServiceType.Carryout instead of ServiceType.Delivery).");
            }
            return closestStore;
        }
    }
}
