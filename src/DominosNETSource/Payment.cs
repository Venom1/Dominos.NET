using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DominosNET
{
    /// <summary>
    /// Class for paying via credit card, with some nice logic to see if your card is valid using regular expressions (regex).
    /// </summary>
    public class Card
    {
        public enum CardType
        {
            MasterCard,
            Visa,
            AmericanExpress,
            Discover,
            JCB
        };

        private static CardType FindType(string cardNumber)
        {
            //https://www.regular-expressions.info/creditcard.html
            if (Regex.Match(cardNumber, @"^4[0-9]{12}(?:[0-9]{3})?$").Success)
            {
                return CardType.Visa;
            }
            else if (Regex.Match(cardNumber, @"^(?:5[1-5][0-9]{2}|222[1-9]|22[3-9][0-9]|2[3-6][0-9]{2}|27[01][0-9]|2720)[0-9]{12}$").Success)
            {
                return CardType.MasterCard;
            }
            else if (Regex.Match(cardNumber, @"^3[47][0-9]{13}$").Success)
            {
                return CardType.AmericanExpress;
            }
            else if (Regex.Match(cardNumber, @"^6(?:011|5[0-9]{2})[0-9]{12}$").Success)
            {
                return CardType.Discover;
            }
            else if (Regex.Match(cardNumber, @"^(?:2131|1800|35\d{3})\d{11}$").Success)
            {
                return CardType.JCB;
            }
            else
            {
                throw new InvalidCardException("Unknown card.");
            }
        }

        public string name;
        public string expiration;
        public string number;
        public string cvv;
        public string zip;
        public CardType type;

        public Card(string name, string expiration, string number, string cvv, string zip)
        {
            this.name = name;
            this.expiration = expiration;
            this.number = number;
            this.cvv = cvv;
            this.zip = zip;
            this.type = FindType(number);
        }
    }

    [Serializable]
    public class InvalidCardException : Exception
    {
        public InvalidCardException() { }
        public InvalidCardException(string message) : base(message) { }
        public InvalidCardException(string message, Exception inner) : base(message, inner) { }
        protected InvalidCardException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public enum PaymentType
    {
        Cash,
        GiftCard,
        CreditCard,
        DoorDebit,
        DoorCredit
    }
}
