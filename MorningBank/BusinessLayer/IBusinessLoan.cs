using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorningBank.BusinessLayer
{
    interface IBusinessLoan
    {
        List<Loan> getAllUnapprovedLoans();
        List<Loan> getAllLoans();
        List<Loan> getAllLoansForUser(string username);
        int applyForLoan(string username, string desc, decimal? amt);
        int approveLoan(int loanId);
        Loan getLoan(int loanid);
        bool checkEligibilityforLoan(String username, int loanId);
    }
}
