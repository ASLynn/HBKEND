using Braintree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class CreditCardModel
    {
        public string CardHolderName { get; set; }
        public string CardType { get; set; }
        public string ExpirationDate { get; set; }
        public string ExpirationMonth { get; set; }
        public string ExpirationYear { get; set; }
        public string ImageUrl { get; set; }
        public string IssuingBank { get; set; }
        public string MaskedNumber { get; set; }
        public UserCardType UserCardType { get; set; }
        public string PaymentMethodNonce { get; set; }

        public CreditCardModel()
        {

        }

        public CreditCardModel(CreditCard objectCreditCard,string nonce)
        {
            CardHolderName = objectCreditCard.CardholderName;
            CardType = objectCreditCard.CardType.ToString();
            ExpirationDate = objectCreditCard.ExpirationDate;
            ExpirationMonth = objectCreditCard.ExpirationMonth;
            ExpirationYear = objectCreditCard.ExpirationYear;
            ImageUrl = objectCreditCard.ImageUrl;
            IssuingBank = objectCreditCard.IssuingBank;
            MaskedNumber = objectCreditCard.MaskedNumber;
            PaymentMethodNonce = nonce;
            if (objectCreditCard.Debit == CreditCardDebit.YES)
            {
                UserCardType = Web.UserCardType.Debit;
            }
            //else if (objectCreditCard.Debit == CreditCardDebit.NO)
            //{
            //    UserCardType = Web.UserCardType.Credit;
            //}
            else
            {
                UserCardType = Web.UserCardType.Credit;
            }
        }
    }
}