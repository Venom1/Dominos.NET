# Dominos.NET
**A C# Wrapper for [pizzapi.](https://github.com/ggrammar/pizzapi)**<br>
Order Dominos Pizza using .NET! (Works in the US and Canada, more country support coming soon.)<br>
**Made with [Json.NET.](https://www.newtonsoft.com/json)**


# Installation
1. Install [Json.NET](https://www.newtonsoft.com/json) via NuGet
2. Add the DominosNETSource folder to your project.
3. Get pizza-ing!

# Tutorial
**Coupons coming soon!**
NOTE: Set your consoles encoding to UTF8 for certain symbols such as ® to render (if you're using this in the console, that is):
```cs
Console.OutputEncoding = Encoding.UTF8;
```
First, add this namespace:
```cs
using DominosNET;
```
Construct a `Customer`and `Address` object.
```cs
Customer customer = new Customer("2024561111", "Joe", "Biden", "joe@gmail.com");
Address address = new Address("1600 Pennsylvania Avenue NW", "Washington", "DC", "20500", Country.US, ServiceType.Delivery);
```
Or, if you live in Canada:
```cs
Customer customer = new Customer("6139924793", "Justin", "Trudeau", "justin@gmail.com");
Address address = new Address("Wellington Street", "Ottawa", "Ontario", "K1A0A9", Country.CA, ServiceType.Delivery);
```
Then, get the nearest store to the user that supports the given service type:
```cs
Store store = address.GetClosestStore();
```


```cs
Menu menu = store.GetMenu();
IEnumerable<MenuItem> items = menu.Search("Coke");

foreach(MenuItem item in items)
{
  Console.WriteLine(item.ToString());
}
```
prints to the console something along the lines of:
```
20BCOKE    20oz Bottle Coke®        $1.89
20BDCOKE   20oz Bottle Diet Coke®   $1.89
D20BZRO    20oz Bottle Coke Zero™   $1.89
2LDCOKE    2-Liter Diet Coke®       $2.99
2LCOKE     2-Liter Coke®            $2.99
```
Or, you can search coupons.
```cs
menu.SearchCoupons(" ");
```
(This would literally print all the coupons on the menu) <br>
Create an order and add items to the order:
```cs
Order order = new Order(store, customer, address);
``` 
Add items to the order:
```cs
order.AddItem(MenuItem.FromCode(menu, "2LDCOKE"), 3); //adds 3 2 litre diet cokes
```
Or, remove items from the order:
```cs
order.RemoveItem(MenuItem.FromCode(menu, "2LDCOKE"), 2); //removes 2 2 litre diet cokes, leaving you with 1
```
You can also do the same with coupons:
```cs
order.AddCoupon(new Coupon("12345"));
order.RemoveCoupon(new Coupon("12345"));
```
Finally, when you are done, place your order!
There are two methods to do this, credit card or no credit card.
Lets go over the first one. To place an order with a credit card, construct a `Card` and use that.
Canada:
```cs
Card creditCard = new Card("Justin Trudeau", "0123", "4498584993849383", "456" "K1A0A9");
order.PlaceOrder(creditCard);
```
America:
```cs
Card creditCard = new Card("Joe Biden", "0123", "4498584993849383", "456" "20500");
order.PlaceOrder(creditCard);
```
Or, you could pay with, for example, cash.
```cs
order.place(PaymentType.Cash);
```
You could also pay with GiftCard, CreditCard, DoorDebit, DoorCredit (will throw an exception if the store doesnt support said type)
```cs
order.place(PaymentType.GiftCard);
```

That's all, enjoy! Of course, more will be added in the future.
