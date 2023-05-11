using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class BankModel
    {
        public int BankId { get; set; }
        public string BankName { get; set; }

        public BankModel()
        {

        }

        public BankModel(Entity.Bank entityBank)
        {
            BankId = entityBank.BankId;
            BankName = entityBank.BankName;
        }
    }
}