using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DominosNET
{
    public enum By
    {
        Code,
        Name,
        CodeAndName
    }

    public class Menu
    {
        public JObject menuJSON;
        public Country country;

        public static Menu FromStore(string storeID, Country country)
        {
            async Task<string> GetMenuJSONString()
            {
                if (country == Country.CA)
                {
                    HttpClient httpClient = new HttpClient();
                    string URL = urls.ca["menu_url"].Replace("{store_id}", storeID).Replace("{lang}", "en");

                    string content = await httpClient.GetStringAsync(URL);
                    return content;

                }
                else
                {
                    HttpClient httpClient = new HttpClient();
                    string URL = urls.us["menu_url"].Replace("{store_id}", storeID).Replace("{lang}", "en");

                    string content = await httpClient.GetStringAsync(URL);
                    return content;

                }
            }
            JObject jsonData = JObject.Parse(GetMenuJSONString().Result);
            return new Menu(country, jsonData);
        }

        public IEnumerable<MenuItem> SearchMenu(string searchTerm, By by = By.CodeAndName)
        {
            JObject predefinedproducts = JObject.Parse(menuJSON["Variants"].ToString());

            foreach (var predefinedproduct in predefinedproducts)
            {
                if (JObject.Parse(predefinedproduct.Value.ToString())["Price"] == null) continue;

                if (JObject.Parse(predefinedproduct.Value.ToString())["Code"].ToString().ToLower().Contains(searchTerm.ToLower()) && (by == By.Code || by == By.CodeAndName))
                {
                    yield return new MenuItem(predefinedproduct);
                }
                else if (JObject.Parse(predefinedproduct.Value.ToString())["Name"].ToString().ToLower().Contains(searchTerm.ToLower()) && (by == By.Name || by == By.CodeAndName))
                {
                    yield return new MenuItem(predefinedproduct);
                }
            }
        }

        public IEnumerable<MenuItem> GetMenuItems()
        {
            JObject predefinedproducts = JObject.Parse(menuJSON["Variants"].ToString());

            foreach (var predefinedproduct in predefinedproducts)
            {
                if (JObject.Parse(predefinedproduct.Value.ToString())["Price"] != null)
                {
                    yield return new MenuItem(predefinedproduct);
                }
            }
        }

        public Menu(Country c, JObject j)
        {
            country = c;
            menuJSON = j;
        }
    }

    [Serializable]
    public class InvalidItemException : Exception
    {

        public InvalidItemException() { }
        public InvalidItemException(string message) : base(message) { }
        public InvalidItemException(string message, Exception inner) : base(message, inner) { }
        protected InvalidItemException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class ItemNotFoundException : Exception
    {

        public ItemNotFoundException() { }
        public ItemNotFoundException(string message) : base(message) { }
        public ItemNotFoundException(string message, Exception inner) : base(message, inner) { }
        protected ItemNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public readonly struct MenuItem
    {
        public readonly string code, name;
        public readonly decimal price;

        public MenuItem(string code, string name, decimal price)
        {
            this.code = code;
            this.name = name;
            this.price = price;
        }

        public MenuItem(KeyValuePair<string, JToken> item)
        {
            code = JObject.Parse(item.Value.ToString())["Code"].ToString();
            name = JObject.Parse(item.Value.ToString())["Name"].ToString();
            price = decimal.Parse(JObject.Parse(item.Value.ToString())["Price"].ToString());
        }

        public static MenuItem FromCode(Menu menu, string itemCode)
        {
            IEnumerable<MenuItem> items = menu.SearchMenu(itemCode, By.Code);
            MenuItem? item = null;

            foreach (MenuItem i in items)
            {
                if (i.code == itemCode)
                {
                    item = i;
                    break;
                }
            }

            if (items.Count() > 0 && item.HasValue)
            {
                return items.First();
            }
            else
            {
                throw new ItemNotFoundException($"No item exists with code '{itemCode}'.");
            }
        }

        public override string ToString()
        {
            return $"[{code}]   \"{name}\"   ${Math.Round(price, 2)}";
        }

        public override bool Equals(object obj)
        {
            return ((MenuItem)obj).code == code;
        }

        public override int GetHashCode()
        {
            return code.GetHashCode();
        }
    }
}