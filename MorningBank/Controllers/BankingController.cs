using MorningBank.BusinessLayer;
using MorningBank.Models.DomainModels;
using MorningBank.Models.ViewModels;
using MorningBank.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MorningBank.Controllers
{
    [Authorize]
    public class BankingController : Controller
    {
        public ActionResult TransferCheckingToSaving()
        {
            TransferCToSModel tcs = new TransferCToSModel();
            UserInfo ui = CookieFacade.USERINFO;
            IBusinessBanking ibank = GenericFactory<Business, IBusinessBanking>.GetInstance();
            tcs.CheckingBalance = ibank.GetCheckingBalance(ui.CheckingAcccountNumber);
            tcs.SavingBalance = ibank.GetSavingBalance(ui.SavingAccountNumber);
            tcs.Amount = 5;
            ViewBag.Message = "";
            return View(tcs);
        }

        [HttpPost]
        public ActionResult TransferCheckingToSaving(TransferCToSModel tcs)
        {
            IBusinessBanking ibank = GenericFactory<Business, IBusinessBanking>.GetInstance();
            UserInfo ui = CookieFacade.USERINFO;
            try
            {
                if (ModelState.IsValid)
                {
                    bool ret = ibank.TransferCheckingToSaving(ui.CheckingAcccountNumber, ui.SavingAccountNumber, tcs.Amount);
                    if (ret == true)
                    {
                        ViewBag.Message = "Transfer successful..";
                        ModelState.Clear();   // otherwise, textbox will display the old amount   
                        tcs.Amount = 0;
                    }
                }
            }
            catch(Exception ex)
            {
                ViewBag.Message = ex.Message;
            }
            tcs.CheckingBalance = ibank.GetCheckingBalance(ui.CheckingAcccountNumber);
            tcs.SavingBalance = ibank.GetSavingBalance(ui.SavingAccountNumber);
            return View(tcs);
        }

        public ActionResult TransferHistory()
        {
            IBusinessBanking ibank = GenericFactory<Business, IBusinessBanking>.GetInstance();
            UserInfo ui = CookieFacade.USERINFO;
            List<TransactionHistory> THList = ibank.GetTransactionHistory(ui.CheckingAcccountNumber);
            List<TransactionHistoryModel> thmList = new List<TransactionHistoryModel>();
            return View(THList);
        }

        public ActionResult TransferSavingToChecking()
        {
            TransferSToCModel tcs = new TransferSToCModel();
            UserInfo ui = CookieFacade.USERINFO;
            IBusinessBanking ibank = GenericFactory<Business, IBusinessBanking>.GetInstance();
            tcs.CheckingBalance = ibank.GetCheckingBalance(ui.CheckingAcccountNumber);
            tcs.SavingBalance = ibank.GetSavingBalance(ui.SavingAccountNumber);
            tcs.Amount = 5;
            ViewBag.Message = "There is a $5 fee to transfer from Saving Account To Checking Account";
            return View(tcs);
        }

        [HttpPost]
        public ActionResult TransferSavingToChecking(TransferSToCModel tcs)
        {
            IBusinessBanking ibank = GenericFactory<Business, IBusinessBanking>.GetInstance();
            UserInfo ui = CookieFacade.USERINFO;
            try
            {
                if (ModelState.IsValid)
                {
                    bool ret = ibank.TransferSavingToChecking(ui.CheckingAcccountNumber, ui.SavingAccountNumber, tcs.Amount);
                    if (ret == true)
                    {
                        ViewBag.Message = "Transfer successful..";
                        ModelState.Clear();   // otherwise, textbox will display the old amount   
                        tcs.Amount = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
            }
            tcs.CheckingBalance = ibank.GetCheckingBalance(ui.CheckingAcccountNumber);
            tcs.SavingBalance = ibank.GetSavingBalance(ui.SavingAccountNumber);
            return View(tcs);
        }

        [Authorize(Roles ="admin")]
        public ActionResult BillPayment()
        {
            List<BillPayment> billPayments = new List<BillPayment>();
            try
            {
                IBusinessBanking ibank = GenericFactory<Business, IBusinessBanking>.GetInstance();
                UserInfo ui = CookieFacade.USERINFO;
                billPayments = ibank.GetBills(ui.Username);
                BillPayment bp = new BillPayment();
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
            } 
            return View(billPayments);
        }
        
        public ActionResult Pay()
        {
            PayBills pay = new PayBills();
            try
            {
                IBusinessBanking ibank = GenericFactory<Business, IBusinessBanking>.GetInstance();
                UserInfo ui = CookieFacade.USERINFO;
                List<BillPayment> billPayments = ibank.GetUnPaidBills(ui.Username); 
                pay.billPayments = billPayments;
            }
            catch(Exception ex)
            {
                ViewBag.Message = ex.Message;
            }
            
            return View(pay);
        }

        [HttpPost]
        public ActionResult Pay(PayBills pay)
        {
            bool res = false;
            try
            {
                IBusinessBanking ibank = GenericFactory<Business, IBusinessBanking>.GetInstance();
                UserInfo ui = CookieFacade.USERINFO;
                int selectedID = pay.selectedBill;
                pay.billPayments = ibank.GetUnPaidBills(ui.Username);
                BillPayment Bill = null;
                foreach (BillPayment bill in pay.billPayments)
                {
                    if (bill.BillID == selectedID)
                    {
                        Bill = bill;
                        break;
                    }
                }
                if (Bill != null)
                {
                    res = ibank.payBill(ui.CheckingAcccountNumber, Bill.BillID, Bill.Amount);
                    if (res)
                    {
                        ViewBag.Message = "Bill Paid";
                        ModelState.Clear();
                        pay.selectedBill = 0;
                    }
                }
                else
                {
                    ViewBag.Message = "This bill is already paid";
                    ModelState.Clear();
                    pay.selectedBill = 0;
                }
                
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
            }

            return View(pay);
        }

    }
}