using MorningBank.Models.DomainModels;
using MorningBank.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorningBank.BusinessLayer
{
    public interface IBusinessBanking
    {
        decimal GetCheckingBalance(long checkingAccountNum);
        decimal GetSavingBalance(long savingAccountNum);
        long GetCheckingAccountNumForUser(string username);
        long GetSavingAccountNumForUser(string username);
        bool TransferCheckingToSaving(long checkingAccountNum, long savingAccountNum, decimal amount);
        bool TransferSavingToChecking(long checkingAccountNum, long savingAccountNum, decimal amount);
        List<TransactionHistory> GetTransactionHistory(long checkingAccountNum);
        List<BillPayment> GetBills(string username);
        List<BillPayment> GetUnPaidBills(string username);
        bool payBill(long checkingAccountNum, int billId, decimal? amount);
    }
}
