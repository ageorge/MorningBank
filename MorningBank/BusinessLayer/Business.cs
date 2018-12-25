using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MorningBank.DataLayer;
using MorningBank.Models.DomainModels;
using MorningBank.Models.ViewModels;
using MorningBank.Utils;

namespace MorningBank.BusinessLayer
{
    public class Business : IBusinessAuthentication, IBusinessBanking, IBusinessLoan
    {
        IRepositoryAuthentication _iauth = null;
        IRepositoryBanking _ibank = null;
        IRepositoryLoan _iloan = null;

        public Business(IRepositoryAuthentication iauth, IRepositoryBanking ibank, IRepositoryLoan iloan)
        {
            _iauth = iauth;
            _ibank = ibank;
            _iloan = iloan;
        }

        public Business() : this(GenericFactory<Repository, IRepositoryAuthentication>.GetInstance(), GenericFactory<Repository, IRepositoryBanking>.GetInstance(), GenericFactory<Repository, IRepositoryLoan>.GetInstance())
        { }

        public bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            throw new NotImplementedException();
        }

        public bool CheckIfValidUser(string username, string password)
        {
            return _iauth.CheckIfValidUser(username, password);
        }

        public long GetCheckingAccountNumForUser(string username)
        {
            return _ibank.GetCheckingAccountNumForUser(username);
        }

        public decimal GetCheckingBalance(long checkingAccountNum)
        {
            return _ibank.GetCheckingBalance(checkingAccountNum);
        }

        public string GetRolesForUser(string username)
        {
            return _iauth.GetRolesForUser(username);
        }

        public long GetSavingAccountNumForUser(string username)
        {
            return _ibank.GetSavingAccountNumForUser(username);
        }

        public decimal GetSavingBalance(long savingAccountNum)
        {
            return _ibank.GetSavingBalance(savingAccountNum);
        }

        public List<TransactionHistory> GetTransactionHistory(long checkingAccountNum)
        {
            return _ibank.GetTransactionHistory(checkingAccountNum);
        }

        public bool TransferCheckingToSaving(long checkingAccountNum, long savingAccountNum, decimal amount)
        {
            return _ibank.TransferCheckingToSaving(checkingAccountNum, savingAccountNum, amount, 0);
        }

        public bool TransferSavingToChecking(long checkingAccountNum, long savingAccountNum, decimal amount)
        {
            return _ibank.TransferSavingToChecking(checkingAccountNum, savingAccountNum, amount, 5);
        }

        public List<BillPayment> GetBills(string username)
        {
            return _ibank.GetBills(username);
        }

        public List<BillPayment> GetUnPaidBills(string username)
        {
            return _ibank.GetUnPaidBills(username);
        }

        public bool payBill(long checkingAccountNum, int billId, decimal? amount)
        {
            return _ibank.payBill(checkingAccountNum,billId,amount);
        }

        public List<Loan> getAllUnapprovedLoans()
        {
            return _iloan.getAllUnapprovedLoans();
        }

        public List<Loan> getAllLoans()
        {
            return _iloan.getAllLoans();
        }

        public List<Loan> getAllLoansForUser(string username)
        {
            return _iloan.getAllLoansForUser(username);
        }

        public int applyForLoan(string username, string desc, decimal? amt)
        {
            return _iloan.applyForLoan(username,desc,amt);
        }

        public int approveLoan(int loanId)
        {
            return _iloan.approveLoan(loanId);
        }

        public Loan getLoan(int loanid)
        {
            return _iloan.getLoan(loanid);
        }

        public bool checkEligibilityforLoan(string username, int loanId)
        {
            return _iloan.checkEligibilityforLoan(username,loanId);
        }
    }
}