using System;

namespace DominosNET
{
    ///<summary>
    /// The class for the customer, used to create an order.
    /// </summary>
    public class Customer
    {
        public string phoneNumber;
        public string firstName;
        public string lastName; //state or province
        public string email; //This can be your postal code if you live in canada

        public Customer(string phonenumber, string firstname, string lastname, string email)
        {
            this.phoneNumber = phonenumber;
            this.firstName = firstname;
            this.lastName = lastname;
            this.email = email;
        }
    }
}