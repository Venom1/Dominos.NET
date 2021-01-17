using System;
using System.Collections.Generic;
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

        public Address(string street, string city, string region, string zip, Country country)
        {
            this.street = street;
            this.city = city;
            this.region = region;
            this.zip = zip;
            this.country = country;
        }

        public Store GetClosestStore(ServiceType serviceType)
        {
            Store closestStore = null;

            async Task<string> GetJSON()
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

            async Task<string> GetStoreInfo(string storeId)
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
                        JObject data = JObject.Parse(GetStoreInfo(store["StoreID"].ToString()).Result);
                        Address address = new Address(data["StreetName"].ToString(), data["City"].ToString(), data["Region"].ToString(), data["PostalCode"].ToString(), country);
                        closestStore = new Store(data, country, store["StoreID"].ToString(), address);
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

        /// <param name="getClosed">Should closed stores also be included in the search?</param>
        public IEnumerable<StoreInfo> GetClosestStores(ServiceType serviceType, bool getClosed = false)
        {
            async Task<string> GetJSON()
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

            async Task<string> GetStoreInfo(string storeId)
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

            IEnumerable<StoreInfo> SetStoreClass()
            {
                JObject json = JObject.Parse(GetJSON().Result);
                JArray stores = JArray.Parse(json["Stores"].ToString());
                foreach (JObject store in stores.Children())
                {
                    if (getClosed)
                    {
                        List<ServiceType> openTypes = new List<ServiceType>();
                        foreach (string type in Enum.GetNames<ServiceType>())
                        {
                            if (store["ServiceIsOpen"][serviceType.ToString()].ToObject<bool>())
                            {
                                openTypes.Add(Enum.Parse<ServiceType>(type));
                            }
                        }

                        JObject data = JObject.Parse(GetStoreInfo(store["StoreID"].ToString()).Result);
                        Address address = new Address(data["StreetName"].ToString(), data["City"].ToString(), data["Region"].ToString(), data["PostalCode"].ToString(), country);
                        Store str = new Store(data, country, store["StoreID"].ToString(), address);
                        yield return new StoreInfo(str, store["IsOnlineNow"].ToObject<bool>(), openTypes);
                    }
                    else
                    {
                        if (store["IsOnlineNow"].ToObject<bool>() && store["ServiceIsOpen"][serviceType.ToString()].ToObject<bool>())
                        {
                            List<ServiceType> openTypes = new List<ServiceType>();
                            foreach (string type in Enum.GetNames<ServiceType>())
                            {
                                if (store["ServiceIsOpen"][serviceType.ToString()].ToObject<bool>())
                                {
                                    openTypes.Add(Enum.Parse<ServiceType>(type));
                                }
                            }

                            JObject data = JObject.Parse(GetStoreInfo(store["StoreID"].ToString()).Result);
                            Address address = new Address(data["StreetName"].ToString(), data["City"].ToString(), data["Region"].ToString(), data["PostalCode"].ToString(), country);
                            Store str = new Store(data, country, store["StoreID"].ToString(), address);
                            yield return new StoreInfo(str, true, openTypes);
                        }
                    }
                }
            }

            return SetStoreClass();
        }

        public override string ToString()
        {
            return $"{street}, {city}, {region}";
        }
    }
}
