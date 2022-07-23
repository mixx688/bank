using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using bank.Models;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;

namespace bank.Controllers
{
    public class AccountsController : Controller
    {
        private Database1Entities db = new Database1Entities();
        public static string Encrypt(string plaintext)
        {
            byte[] data = Encoding.Default.GetBytes(plaintext);
            SHA256 sha256 = new SHA256CryptoServiceProvider();
            byte[] result = sha256.ComputeHash(data);
            return Convert.ToBase64String(result);
        }
        public ActionResult Login()
        {
            if (Session["account"] != null) return RedirectToAction("Service");
            //解決登入後還是可以進入登入頁面的問題
            return View();
        }
        [HttpPost]
        public ActionResult Login(LoginForm form)
        {
            var r = (from a in db.Account
                     where a.username == form.loginUsername
                     select a).FirstOrDefault();
            if (r == null)
            {
                ViewBag.message = "帳號密碼錯誤!";
                return View();
            }
            string saltedpw = String.Concat(r.Id, form.loginPassword);
            string pw = Encrypt(saltedpw);
            if (string.Compare(pw, r.password, false) == 0)
            {
                Session.RemoveAll();
                Session["account"] = form.loginUsername;
                FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                    1,
                    form.loginUsername,
                    DateTime.Now,
                    DateTime.Now.AddMinutes(30),
                    false,
                    "user data",
                    FormsAuthentication.FormsCookiePath);
                string encTicket = FormsAuthentication.Encrypt(ticket);
                var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
                cookie.HttpOnly = true;
                Response.Cookies.Add(cookie);
                return RedirectToAction("Service");
            }
            else ViewBag.message = "帳號密碼錯誤!";
            return View();
        }
        [Authorize]
        public ActionResult Service()
        {
            ViewBag.message = "目前的使用者為: " + Session["account"];
            return View();
        }
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session["account"] = null;
            return View();
        }
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create(CreateForm form)
        {
            var r = (from a in db.Account
                     where a.username == form.createUsername
                     select a).FirstOrDefault();
            if (r == null)
            {
                Account account = new Account();
                account.username = form.createUsername;
                account.Id = Guid.NewGuid();
                string saltedpw = String.Concat(account.Id, form.createPassword);
                string hashedpw = Encrypt(saltedpw);
                account.password = hashedpw;
                account.balance = form.createBalance;
                db.Account.Add(account);
                logGen(form.createUsername,2,"",form.createBalance,form.createBalance);
                ViewBag.message = "新增成功!";
                return View();
            }
            ViewBag.message = "該帳號已存在!";
            return View();
        }
        [Authorize]
        public ActionResult Balance()
        {
            String user = Session["account"] as string;
            var r = (from a in db.Account
                     where a.username == user
                     select a).FirstOrDefault();
            ViewBag.message = r.balance;
            return View();
        }
        [Authorize]
        public ActionResult Withdraw()
        {
            return View();
        }
        [HttpPost]
        [Authorize]
        public ActionResult Withdraw(AmountForm form)
        {
            String user = Session["account"] as string;
            var r = (from a in db.Account
                     where a.username == user
                     select a).FirstOrDefault();
            if (r.balance < form.amount) ViewBag.message = "餘額不足!";
            else
            {
                r.balance = r.balance - form.amount;
                logGen(user, 1, "", form.amount, r.balance);
                ViewBag.message = "操作成功!";
            }

            return View();
        }
        [Authorize]
        public ActionResult Deposit()
        {
            return View();
        }
        [HttpPost]
        [Authorize]
        public ActionResult Deposit(AmountForm form)
        {
            String user = Session["account"] as string;
            var r = (from a in db.Account
                     where a.username == user
                     select a).FirstOrDefault();
            r.balance = r.balance + form.amount;
            logGen(user, 2, "", form.amount, r.balance);
            ViewBag.message = "操作成功!";
            return View();
        }
        [Authorize]
        public ActionResult Transfer()
        {
            return View();
        }
        [HttpPost]
        [Authorize]
        public ActionResult Transfer(TransferForm form)
        {
            String user = Session["account"] as string;
            var r = (from a in db.Account
                     where a.username == user
                     select a).FirstOrDefault();
            var dest = (from a in db.Account
                        where a.username == form.transferName
                        select a).FirstOrDefault();
            if (r.balance < form.transferAmount) ViewBag.message = "餘額不足!";
            else if (dest == null) ViewBag.message = "此帳號不存在!";
            else if (user == form.transferName) ViewBag.message = "不能轉帳給自己!";
            else
            {
                r.balance = r.balance - form.transferAmount;
                logGen(user, 3, dest.username, form.transferAmount, r.balance);
                dest.balance = dest.balance + form.transferAmount;
                logGen(dest.username, 4, user, form.transferAmount, dest.balance);
                ViewBag.message = "操作成功!";
            }

            return View();
        }
        [Authorize]
        public ActionResult Record()
        {
            String user = Session["account"] as string;
            List<LogTable> list = new List<LogTable>();
            var r = (from a in db.Transaction_log
                     where a.name == user
                     select a);
            foreach(var log in r)
            {
                byte t = log.transaction_type;
                string tType = "";
                switch (t)
                {                  
                    case 1:tType = "提款";
                        break;
                    case 2:tType = "存款";
                        break;
                    case 3:tType = "轉出";
                        break;
                    case 4:tType = "轉入";
                        break;
                }
                list.Add(new LogTable { Tid = log.tid, Type = tType, Dest = log.destination, Amount = log.amount, Balance = log.balance, Time = log.time.ToString() });
            }
            return View(list);
        }
        public void logGen(String name, byte type, String dest, long amount, long balance)
        {
            Transaction_log log = new Transaction_log();
            var c = (from a in db.Transaction_log
                     select a);
            log.tid = c.Count();
            log.name = name;
            log.transaction_type = type;
            if (dest != "") log.destination = dest;
            log.amount = amount;
            log.balance = balance;
            log.time = DateTime.Now;
            db.Transaction_log.Add(log);
            db.SaveChanges();//必須馬上更新log，tid才不會重複，課堂demo轉帳失敗的原因
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
