using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace bank.Models
{
    public class LogTable
    {
        [Key]
        public int Tid { get; set; }
        [Display(Name = "交易類型")]
        public string Type { get; set; }
        [Display(Name = "轉帳目標")]
        public string Dest { get; set; } 
        [Display(Name = "交易金額")]
        public long Amount { get; set; }     
        [Display(Name = "結餘")]
        public long Balance { get; set; }        
        [Display(Name = "交易時間")]
        public string Time { get; set; } 
    }
}