using MorningBank.BusinessLayer;
using MorningBank.Models.DomainModels;
using MorningBank.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MorningBank.Controllers
{
    [Authorize]
    public class LoanController : Controller
    {
        // GET: Loan
        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "manager")]
        public ActionResult Approve()
        {
            List<Loan> loanList = new List<Loan>();
            try
            {
                IBusinessLoan iloan = GenericFactory<Business, IBusinessLoan>.GetInstance();
                UserInfo ui = CookieFacade.USERINFO;
                loanList = iloan.getAllUnapprovedLoans();
                foreach (Loan l in loanList)
                {
                    if (l.Status == "UA")
                    {
                        l.Status = "Not yet Approved";
                    }
                    else if (l.Status == "A")
                    {
                        l.Status = "Approved";
                    }
                }
                Loan loan = new Loan();
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
            }
            return View(loanList);
        }

        [Authorize(Roles = "manager")]
        public ActionResult ApproveLoan(int? loanid)
        {
            int res = 0;
            List<Loan> loanList = new List<Loan>();
            try
            {
                IBusinessLoan iloan = GenericFactory<Business, IBusinessLoan>.GetInstance();
                UserInfo ui = CookieFacade.USERINFO;
                if(loanid != null)
                {
                    Loan appliedLoan = iloan.getLoan((int)loanid);
                    bool eligible = iloan.checkEligibilityforLoan(appliedLoan.UserName, (int)loanid);
                    if (eligible)
                    {
                        res = iloan.approveLoan((int)loanid);
                        if (res > 0)
                        {
                            ViewBag.Message = "Loan approved";
                        }
                    }
                    else
                    {
                        ViewBag.Message = "Not eligible for loan";
                    }
                } 
                loanList = iloan.getAllLoans();
                foreach (Loan l in loanList)
                {
                    if (l.Status == "UA")
                    {
                        l.Status = "Not yet Approved";
                    }
                    else if (l.Status == "A")
                    {
                        l.Status = "Approved";
                    }
                }
                Loan loan = new Loan();
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.StackTrace;
            }
            return View(loanList);
        }

        public ActionResult Apply()
        {
            Loan loan = new Loan();
            return View(loan);
        }

        [HttpPost]
        public ActionResult Apply(Loan loan)
        {
            int result = 0;
            try
            {
                IBusinessLoan iLoan = GenericFactory<Business, IBusinessLoan>.GetInstance();
                UserInfo ui = CookieFacade.USERINFO;
                result = iLoan.applyForLoan(ui.Username, loan.LoanName, loan.LoanAmt);
                if (result > 0)
                {
                    ModelState.Clear(); 
                }
                else
                {
                    ViewBag.Message = "Error in Applying for the loan, Please try later";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
            }

            return View();
        }

        public ActionResult Loans()
        {
            List<Loan> loanList = new List<Loan>();
            try
            {
                IBusinessLoan iloan = GenericFactory<Business, IBusinessLoan>.GetInstance();
                UserInfo ui = CookieFacade.USERINFO;
                loanList = iloan.getAllLoansForUser(ui.Username);
                foreach(Loan l in loanList)
                {
                    if(l.Status == "UA")
                    {
                        l.Status = "Not yet Approved";
                    }
                    else if(l.Status == "A")
                    {
                        l.Status = "Approved";
                    }
                }
                Loan loan = new Loan(); 
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
            }
            return View(loanList);
        }
    }
}