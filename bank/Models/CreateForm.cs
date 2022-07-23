using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace bank.Models
{
    public class CreateForm
    {
        public string createUsername { get; set; }
        public string createPassword { get; set; }
        public long createBalance { get; set; }
    }
}