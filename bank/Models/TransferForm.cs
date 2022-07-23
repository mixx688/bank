using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace bank.Models
{
    public class TransferForm
    {
        public long transferAmount { get; set; }
        public string transferName { get; set; }
    }
}