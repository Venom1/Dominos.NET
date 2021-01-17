using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DominosNET
{
    /// <summary>
    /// NOTE: Coupons are applied when you place the order, they do not affect the price variable.
    /// Be sure to be aware of the products you are ordering with coupons.
    /// </summary>
    public class Order
    {
        public JObject data;
        public JObject menuJSON;
        public Customer customer;
        public Address address;
        public Country country;
        public ServiceType serviceType;
        public Store store;
        public List<MenuItem> Items { get; }
        public List<Coupon> Coupons { get; }

        /// <summary>
        /// Multiply this by your tax rate to get the actual price
        /// </summary>
        public decimal price = 0;
        public Order(Store s, Customer c, Address a, ServiceType st)
        {
            Items = new List<MenuItem>();
            Coupons = new List<Coupon>();
            country = a.country;
            menuJSON = Menu.FromStore(s.id, country).menuJSON;
            store = s;
            customer = c;
            address = a;
            serviceType = st;
            data = JObject.Parse(@"
            {
    ""Address"": {
        ""Street"": """",
        ""City"": """",
        ""Region"": """",
        ""PostalCode"": """",
        ""Type"": """"
    },
    ""Coupons"": [],
    ""CustomerID"": """",
    ""Extension"": """",
    ""OrderChannel"": ""OLO"",
    ""OrderID"": """",
    ""NoCombine"": ""true"",
    ""OrderMethod"": ""Web"",
    ""OrderTaker"": ""null"",
    ""Payments"": [],
    ""Products"": [],
    ""Market"": """",
    ""Currency"": """",
    ""ServiceMethod"": ""Delivery"",
    ""Tags"": {},
    ""Version"": ""1.0"",
    ""SourceOrganizationURI"": ""order.dominos.com"",
    ""LanguageCode"": ""en"",
    ""Partners"": {},
    ""NewUser"": true,
    ""metaData"": {},
    ""Amounts"": {},
    ""BusinessDate"": """",
    ""EstimatedWaitMinutes"": """",
    ""PriceOrderTime"": """",
    ""AmountsBreakdown"": {}
}
                                ");
            JObject addressData = (JObject)data["Address"];
            data["ServiceMethod"] = serviceType.ToString();
            addressData["Street"] = a.street;
            addressData["City"] = a.city;
            addressData["Region"] = a.region;
            addressData["PostalCode"] = a.zip;
            if (a.street.ToLower().Contains("apartment") || a.street.ToLower().Contains("apt") || a.street.ToLower().Contains("#"))
            {
                addressData["Type"] = "Apartment";
            }
            else
            {
                addressData["Type"] = "House";
            }
        }

        public void AddItem(MenuItem item, int quantity = 1)
        {
            if ((JObject)menuJSON["Variants"][item.code] == null)
            {
                throw new InvalidItemException("Invalid item.");
            }

            for (int i = 0; i < quantity; i++)
            {
                JObject jItem = (JObject)menuJSON["Variants"][item.code];
                JArray a = (JArray)data["Products"];
                a.Add((JToken)jItem);
                price += item.price;
                Items.Add(item);
            }
        }

        public void RemoveItem(MenuItem item, int quantityToRemove = 1)
        {
            if ((JObject)menuJSON["Variants"][item.code] == null)
            {
                throw new InvalidItemException("Invalid item.");
            }

            for (int i = 0; i < quantityToRemove; i++)
            {
                JObject jItem = (JObject)menuJSON["Variants"][item.code];
                JArray a = (JArray)data["Products"];
                a.Remove((JToken)jItem);
                if(Items.Remove(item))
                {
                    price -= item.price;
                }
            }
        }

        public void AddCoupon(Coupon coupon)
        {
            bool isAcceptableOrderType = false;

            if ((JObject)menuJSON["Coupons"][coupon.code] == null)
            {
                throw new InvalidItemException($"Invalid coupon code '{coupon.code}'.");
            }

            JObject item = (JObject)menuJSON["Coupons"][coupon.code];
            JArray a = (JArray)data["Coupons"];

            foreach (JToken vsm in JArray.Parse(item["Tags"]["ValidServiceMethods"].ToString()).Children())
            {
                if (serviceType.ToString() == vsm.ToString())
                {
                    isAcceptableOrderType = true;
                    break;
                }
            }

            if (!isAcceptableOrderType)
            {
                throw new Exception($"Coupon '{coupon.code}' does not support your service type.");
            }

            foreach (JToken jCoupon in a.Children())
            {
                JObject couponO = (JObject)jCoupon;
                if (couponO["Code"].ToString() == coupon.code)
                {
                    throw new Exception($"Coupon '{coupon.code}' already exists!");
                }
            }

            Coupons.Add(coupon);
            a.Add((JToken)item);
        }

        public void RemoveCoupon(Coupon coupon)
        {
            if ((JObject)menuJSON["Coupons"][coupon.code] == null)
            {
                throw new InvalidItemException($"Coupon '{coupon.code}' does not exist in order.");
            }

            JArray a = JArray.Parse(data["Coupons"].ToString());

            for (int i = 0; i < a.Count; i++)
            {
                JObject jCoupon = (JObject)a[i];
                if (JObject.Parse(coupon.ToString())["Code"].ToString() == coupon.code)
                {
                    a.Remove((JToken)jCoupon);
                    Coupons.Remove(coupon);
                    data["Coupons"] = JArray.Parse(a.ToString());
                }
            }
        }

        private JObject send(string URL, bool Merge, string content)
        {
            data["StoreID"] = store.id;
            data["Email"] = customer.email;
            data["FirstName"] = customer.first_name;
            data["LastName"] = customer.last_name;
            data["Phone"] = customer.phone_number;
            HttpClient c = new HttpClient();
            StringContent stringContent = null;

            if (content == null)
            {
                stringContent = new StringContent(" { " + @"""Order"" : " + data.ToString() + " } ", Encoding.UTF8, "application/json");
            }
            else
            {
                stringContent = new StringContent(content, Encoding.UTF8, "application/json");
            }

            c.DefaultRequestHeaders.Add("Referer", "https://order.dominos.com/en/pages/order/");

            Task<HttpResponseMessage> m = c.PostAsync(URL, stringContent);

            JObject jsonResponse = JObject.Parse(m.Result.Content.ReadAsStringAsync().Result.Replace(@"^""}]}", ""));

            if (Merge)
            {
                foreach (var keyValuePair in jsonResponse)
                {
                    data[keyValuePair.Key] = keyValuePair.Value;
                }
            }

            return jsonResponse;
        }

        public void PlaceOrder(PaymentType type)
        {
            bool isAcceptable = false;
            JArray acceptablePaymentTypes = (JArray)store.data["AcceptablePaymentTypes"];

            foreach (var v in acceptablePaymentTypes.Children())
            {
                if (v.ToString() == type.ToString())
                {
                    isAcceptable = true;
                }
            }

            if (!isAcceptable)
            {
                throw new Exception("Store does not support type " + type + ".");
            }

            JArray paymentArray = JArray.Parse(data["Payments"].ToString());
            JObject typeObj = new JObject();
            typeObj.Add("Type", type.ToString());
            paymentArray.Add(typeObj);

            if (country == Country.CA)
            {
                send(urls.ca["place_url"], false, send(urls.ca["price_url"], true, null).ToString());
            }
            else
            {
                send(urls.ca["place_url"], false, send(urls.us["price_url"], true, null).ToString());
            }
        }

        public void PlaceOrder(Card o)
        {
            bool isAcceptableCard = false;
            bool canPayWithCard = false;

            JArray acceptableCards = (JArray)store.data["AcceptableCreditCards"];
            JArray acceptablePaymentTypes = (JArray)store.data["AcceptablePaymentTypes"];
            foreach (var v in acceptableCards.Children())
            {
                if (v.ToString() == o.type.ToString())
                {
                    isAcceptableCard = true;
                }
            }
            foreach (var v in acceptablePaymentTypes.Children())
            {
                if (v.ToString() == "CreditCard")
                {
                    isAcceptableCard = true;
                }
            }
            if (canPayWithCard == false)
            {
                throw new Exception("Store does not support credit cards.");
            }
            if (isAcceptableCard)
            {
                JArray paymentArray = JArray.Parse(data["Payments"].ToString());
                JObject typeObj = new JObject();
                typeObj.Add("Type", "CreditCard");
                typeObj.Add("Expiration", o.expiration);
                typeObj.Add("Amount", 0);
                typeObj.Add("CardType", o.type.ToString());
                typeObj.Add("Number", int.Parse(o.number));
                typeObj.Add("SecurityCode", int.Parse(o.cvv));
                typeObj.Add("PostalCode", int.Parse(o.zip));
                paymentArray.Add(typeObj);
            }
            else
            {
                throw new Exception("Card unsupported.");
            }
            if (country == Country.CA)
            {
                send(urls.ca["place_url"], false, send(urls.ca["price_url"], true, null).ToString());
            }
            else
            {
                send(urls.ca["place_url"], false, send(urls.us["price_url"], true, null).ToString());
            }
        }
    }

    public readonly struct Coupon
    {
        public readonly string code;

        public Coupon(string code)
        {
            this.code = code;
        }
    }
}
