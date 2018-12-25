using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using MorningBank.Cache;
using MorningBank.Models.DomainModels;
using MorningBank.Models.ViewModels;

namespace MorningBank.DataLayer
{
    public class Repository : IRepositoryBanking, IRepositoryAuthentication, IRepositoryLoan
    {
        IDataAccess _idac = null;

        public Repository(IDataAccess idac)
        {
            _idac = idac;
        }

        public Repository() : this(new SQLDataAccess()) { }

        public bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            throw new NotImplementedException();
        }

        public bool CheckIfValidUser(string username, string password)
        {
            bool ret = false;
            try
            {
                string sql = "select Username from Users where " + "Username=@Username and Password=@Password";
                List<DbParameter> PList = new List<DbParameter>();
                DbParameter p1 = new SqlParameter("@Username", SqlDbType.VarChar, 50);
                p1.Value = username;
                PList.Add(p1);
                DbParameter p2 = new SqlParameter("@Password", SqlDbType.VarChar, 50);
                p2.Value = password; PList.Add(p2);

                object obj = _idac.GetSingleAnswer(sql, PList);
                if (obj != null)
                    ret = true;
            }
            catch (Exception)
            {
                throw;
            }
            return ret;
        }

        public long GetCheckingAccountNumForUser(string username)
        {
            long checkingAccountNum = 0;
            try
            {
                string sql = "select CheckingAccountNumber from CheckingAccounts where " + "Username=@Username";
                List<DbParameter> PList = new List<DbParameter>();
                DbParameter p1 = new SqlParameter("@Username", SqlDbType.VarChar, 50);
                p1.Value = username;
                PList.Add(p1);
                object obj = _idac.GetSingleAnswer(sql, PList);
                if (obj != null)
                    checkingAccountNum = long.Parse(obj.ToString());
            }
            catch (Exception)
            {
                throw;
            }
            return checkingAccountNum;
        }

        public decimal GetCheckingBalance(long checkingAccountNum)
        {
            decimal balance = 0;
            try
            {
                string sql = "select balance from CheckingAccounts where CheckingAccountNumber=@CheckingAccountNumber";
                List<DbParameter> ParamList = new List<DbParameter>();
                SqlParameter p1 = new SqlParameter("@CheckingAccountNumber", SqlDbType.BigInt);
                p1.Value = checkingAccountNum;
                ParamList.Add(p1);
                object obj = _idac.GetSingleAnswer(sql, ParamList);
                if (obj != null)
                    balance = decimal.Parse(obj.ToString());
            }
            catch (Exception)
            {
                throw;
            }
            return balance;
        }

        public string GetRolesForUser(string username)
        {
            string ret = "";
            try
            {
                string sql = "select r.RoleName from Roles r inner join UserRoles ur on " + "r.RoleId=ur.RoleId inner join Users u on ur.Username=u.Username where " + "u.Username=@Username";
                List<DbParameter> PList = new List<DbParameter>();
                DbParameter p1 = new SqlParameter("@Username", SqlDbType.VarChar, 50);
                p1.Value = username; PList.Add(p1);
                DataTable dt = _idac.GetManyRowsCols(sql, PList);
                string roles = "";
                foreach (DataRow dr in dt.Rows)
                {
                    roles += dr["RoleName"] + "|";
                }
                if (roles.Length > 0)
                    roles = roles.Substring(0, roles.Length - 1);  //remove the trailing pipe |
                ret = roles;
            }
            catch (Exception)
            {
                throw;
            }
            return ret;
        }

        public long GetSavingAccountNumForUser(string username)
        {
            long savingAccountNum = 0;
            try
            {
                string sql = "select SavingAccountNumber from SavingAccounts where " + "Username=@Username";
                List<DbParameter> PList = new List<DbParameter>();
                DbParameter p1 = new SqlParameter("@Username", SqlDbType.VarChar, 50);
                p1.Value = username;
                PList.Add(p1);
                object obj = _idac.GetSingleAnswer(sql, PList);
                if (obj != null)
                    savingAccountNum = long.Parse(obj.ToString());
            }
            catch (Exception)
            {
                throw;
            }
            return savingAccountNum;
        }

        public decimal GetSavingBalance(long savingAccountNum)
        {
            decimal balance = 0;
            try
            {
                string sql = "select balance from SavingAccounts where SavingAccountNumber=@SavingAccountNumber";
                List<DbParameter> ParamList = new List<DbParameter>();
                SqlParameter p1 = new SqlParameter("@SavingAccountNumber", SqlDbType.BigInt);
                p1.Value = savingAccountNum; ParamList.Add(p1);
                object obj = _idac.GetSingleAnswer(sql, ParamList);
                if (obj != null)
                    balance = decimal.Parse(obj.ToString());
            }
            catch (Exception)
            {
                throw;
            }
            return balance;
        }

        public bool TransferCheckingToSaving(long checkingAccountNum, long savingAccountNum, decimal amount, decimal transactionFee)
        {
            bool ret = false;
            string CONNSTR = ConfigurationManager.ConnectionStrings["MYBANK"].ConnectionString;
            SqlConnection conn = new SqlConnection(CONNSTR);
            SqlTransaction sqtr = null;
            try
            {
                conn.Open(); sqtr = conn.BeginTransaction();
                int rows = UpdateCheckingBalanceTR(checkingAccountNum, -1 * amount, conn, sqtr, true);
                if (rows == 0)
                    throw new Exception("Problem in transferring from Checking Account..");

                object obj = GetCheckingBalanceTR(checkingAccountNum, conn, sqtr, true);
                if (obj != null)
                {
                    if (decimal.Parse(obj.ToString()) < 0)  // exception causes transaction to be rolled back                             
                        throw new Exception("Insufficient funds in Checking Account - rolling back transaction");
                }
                rows = UpdateSavingBalanceTR(savingAccountNum, amount, conn, sqtr, true);
                if (rows == 0)
                    throw new Exception("Problem in transferring to Saving Account..");
                rows = AddToTransactionHistoryTR(checkingAccountNum, savingAccountNum, amount, 100, transactionFee, conn, sqtr, true);
                if (rows == 0)
                    throw new Exception("Problem in transferring to Saving Account..");
                else
                {
                    sqtr.Commit();
                    ret = true;
                    // clear the cache                     
                    CacheAbstraction cabs = new CacheAbstraction();
                    cabs.Remove("TRHISTORY");
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                conn.Close();
            }
            return ret;
        }

        public bool TransferSavingToChecking(long checkingAccountNum, long savingAccountNum, decimal amount, decimal transactionFee)
        {
            bool ret = false;
            string CONNSTR = ConfigurationManager.ConnectionStrings["MYBANK"].ConnectionString;
            SqlConnection conn = new SqlConnection(CONNSTR);
            SqlTransaction sqtr = null;
            try
            {
                conn.Open();
                sqtr = conn.BeginTransaction();
                int rows = UpdateSavingBalanceTR(savingAccountNum, -1 * (amount + transactionFee), conn, sqtr, true);
                if (rows == 0)
                    throw new Exception("Problem in transferring from Saving Account..");
                
                object obj = GetSavingBalanceTR(savingAccountNum,conn,sqtr, true);
                if (obj != null)
                {
                    if (decimal.Parse(obj.ToString()) < 0)  // exception causes transaction to be rolled back                             
                        throw new Exception("Insufficient funds in Saving Account - rolling back transaction");
                }
                rows = UpdateCheckingBalanceTR(checkingAccountNum, amount, conn, sqtr, true);
                if (rows == 0)
                    throw new Exception("Problem in transferring to Checking Account..");
                // 101 transaction type id for saving to checking transfer
                rows = AddToTransactionHistoryTR(checkingAccountNum, savingAccountNum, amount, 101, transactionFee, conn, sqtr, true);
                if (rows == 0)
                    throw new Exception("Problem in transferring to Checking Account..");
                else
                {
                    sqtr.Commit();
                    ret = true;
                    // clear the cache                     
                    CacheAbstraction cabs = new CacheAbstraction();
                    cabs.Remove("TRHISTORY");
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                conn.Close();
            }
            return ret;
        }

        private int UpdateCheckingBalanceTR(long checkingAccountNum, decimal amount, DbConnection conn, DbTransaction sqtr, bool doTransaction)
        {
            int rows = 0;
            try
            {
                string sql1 = "Update CheckingAccounts set Balance=Balance+@Amount where CheckingAccountNumber=@CheckingAccountNumber";
                List<DbParameter> ParamList = new List<DbParameter>();
                SqlParameter p1 = new SqlParameter("@CheckingAccountNumber", SqlDbType.BigInt);
                p1.Value = checkingAccountNum;
                ParamList.Add(p1);
                SqlParameter p2 = new SqlParameter("@Amount", SqlDbType.Decimal);
                p2.Value = amount; ParamList.Add(p2);
                rows = _idac.InsertUpdateDelete(sql1, ParamList, conn, sqtr, doTransaction);
            }
            catch (Exception)
            {
                throw;
            }
            return rows;
        }

        private int UpdateSavingBalanceTR(long savingAccountNum, decimal amount, DbConnection conn, DbTransaction sqtr, bool doTransaction)
        {
            int rows = 0;
            try
            {
                string sql1 = "Update SavingAccounts set Balance=Balance+@Amount where SavingAccountNumber=@SavingAccountNumber";
                List<DbParameter> ParamList = new List<DbParameter>();
                SqlParameter p1 = new SqlParameter("@SavingAccountNumber", SqlDbType.BigInt);
                p1.Value = savingAccountNum;
                ParamList.Add(p1);
                SqlParameter p2 = new SqlParameter("@Amount", SqlDbType.Decimal);
                p2.Value = amount; ParamList.Add(p2);
                rows = _idac.InsertUpdateDelete(sql1, ParamList, conn, sqtr, doTransaction);

            }
            catch (Exception)
            {
                throw;
            }
            return rows;
        }

        private object GetCheckingBalanceTR(long checkingAccountNum, DbConnection conn, DbTransaction sqtr, bool doTransaction)
        {
            object objBal = null;
            try
            {
                string sql2 = "select Balance from CheckingAccounts where CheckingAccountNumber=@CheckingAccountNumber";
                List<DbParameter> ParamList2 = new List<DbParameter>();
                SqlParameter pa = new SqlParameter("@CheckingAccountNumber", SqlDbType.BigInt);
                pa.Value = checkingAccountNum;
                ParamList2.Add(pa);
                objBal = _idac.GetSingleAnswer(sql2, ParamList2, conn, sqtr, doTransaction);
            }
            catch (Exception)
            {
                throw;
            }
            return objBal;
        }

        private object GetSavingBalanceTR(long saviningAccountNum, DbConnection conn, DbTransaction sqtr, bool doTransaction)
        {
            object objBal = null;
            try
            {
                string sql2 = "select Balance from SavingAccounts where SavingAccountNumber=@SavingAccountNumber";
                List<DbParameter> ParamList2 = new List<DbParameter>();
                SqlParameter pa = new SqlParameter("@SavingAccountNumber", SqlDbType.BigInt);
                pa.Value = saviningAccountNum;
                ParamList2.Add(pa);
                objBal = _idac.GetSingleAnswer(sql2, ParamList2, conn, sqtr, doTransaction);
            }
            catch (Exception)
            {
                throw;
            }
            return objBal;
        }

        private int AddToTransactionHistoryTR(long checkingAccountNum, long savingAccountNum, decimal amount, int transTypeId, decimal transFee, DbConnection conn, DbTransaction sqtr, bool doTransaction)
        {
            int rows = 0;
            try
            {
                string sql1 = "insert into TransactionHistories(CheckingAccountNumber,SavingAccountNumber," + "Amount,TransactionFee,TransactionTypeId) values (@CheckingAccountNumber,@SavingAccountNumber," + "@Amount,@TransactionFee,@TransactionTypeId)";
                List<DbParameter> ParamList = new List<DbParameter>();
                SqlParameter p1 = new SqlParameter("@CheckingAccountNumber", SqlDbType.BigInt);
                p1.Value = checkingAccountNum;
                ParamList.Add(p1);
                SqlParameter p2 = new SqlParameter("@SavingAccountNumber", SqlDbType.BigInt);
                p2.Value = savingAccountNum;
                ParamList.Add(p2);
                SqlParameter p3 = new SqlParameter("@Amount", SqlDbType.Decimal);
                p3.Value = amount;
                ParamList.Add(p3);
                SqlParameter p4 = new SqlParameter("@TransactionFee", SqlDbType.Decimal);
                p4.Value = transFee;
                ParamList.Add(p4);
                SqlParameter p5 = new SqlParameter("@TransactionTypeId", SqlDbType.Int);
                p5.Value = transTypeId;
                ParamList.Add(p5);
                rows = _idac.InsertUpdateDelete(sql1, ParamList, conn, sqtr, doTransaction);  
            }
            catch (Exception)
            {
                throw;
            }
            return rows;
        }

        public List<TransactionHistory> GetTransactionHistory(long checkingAccountNum)
        {
            List<TransactionHistory> THList = null;
            try
            {
                CacheAbstraction cabs = new CacheAbstraction();
                THList = cabs.Retrieve<List<TransactionHistory>>("TRHISTORY" + ":" + checkingAccountNum);
                if (THList != null)
                    return THList;
                string sql = "select th.*, trt.TransactionTypeName from TransactionHistories th " + "inner join TransactionType trt on th.TransactionTypeId=trt.TransactionTypeId " + "where CheckingAccountNumber=@CheckingAccountNumber";
                List<DbParameter> ParamList = new List<DbParameter>();
                SqlParameter p1 = new SqlParameter("@CheckingAccountNumber", SqlDbType.BigInt);
                p1.Value = checkingAccountNum;
                ParamList.Add(p1);
                DataTable dt = _idac.GetManyRowsCols(sql, ParamList);
                string amt = dt.Rows[0]["Amount"].ToString();
                THList = DBList.ToList<TransactionHistory>(dt);
                cabs.Insert("TRHISTORY" + ":" + checkingAccountNum, THList);
            }
            catch(Exception)
            {
                throw;
            }
            return THList;
        }

        public List<BillPayment> GetBills(string username)
        {
            string sql = "select * from BillPayment where Username=@Username";
            List<BillPayment> billPayments = new List<BillPayment>();
            try
            {
                List<DbParameter> paramList = new List<DbParameter>();
                SqlParameter p1 = new SqlParameter("@Username", SqlDbType.VarChar, 50);
                p1.Value = username;
                paramList.Add(p1);
                DataTable dt = _idac.GetManyRowsCols(sql, paramList);
                billPayments = DBList.ToList<BillPayment>(dt);
            }
            catch (Exception)
            {
                throw;
            }

            return billPayments;
        }

        public List<BillPayment> GetUnPaidBills(string username)
        {
            string sql = "select * from BillPayment where Username=@Username and Status = 'UNPAID'";
            List<BillPayment> billPayments = new List<BillPayment>();
            try
            {
                List<DbParameter> paramList = new List<DbParameter>();
                SqlParameter p1 = new SqlParameter("@Username", SqlDbType.VarChar, 50);
                p1.Value = username;
                paramList.Add(p1);
                DataTable dt = _idac.GetManyRowsCols(sql, paramList);
                billPayments = DBList.ToList<BillPayment>(dt);
            }
            catch (Exception)
            {
                throw;
            }

            return billPayments;
        }

        private int UpdateBillsPaymentStatus(int billID, DbConnection conn, DbTransaction sqtr, bool doTransaction)
        {
            int rows = 0;
            try
            {
                string sql1 = "Update BillPayment set Status='PAID' where BillID=@BillID";
                List<DbParameter> ParamList = new List<DbParameter>();
                SqlParameter p1 = new SqlParameter("@BillID", SqlDbType.Int);
                p1.Value = billID;
                ParamList.Add(p1);
                rows = _idac.InsertUpdateDelete(sql1, ParamList, conn, sqtr, doTransaction); 
            }
            catch (Exception)
            {
                throw;
            }
            return rows;
        }

        public bool payBill(long checkingAccountNum, int billId, decimal? amount)
        {
            bool res = false; 
            string CONNSTR = ConfigurationManager.ConnectionStrings["MYBANK"].ConnectionString;
            SqlConnection conn = new SqlConnection(CONNSTR);
            SqlTransaction sqtr = null;
            try
            {
                conn.Open();
                sqtr = conn.BeginTransaction();
                int rows = UpdateCheckingBalanceTR(checkingAccountNum, -1 * (decimal)amount, conn, sqtr, true);
                if (rows == 0)
                    throw new Exception("Problem in transferring from Checking Account..");

                object obj = GetCheckingBalanceTR(checkingAccountNum, conn, sqtr, true);
                if (obj != null)
                {
                    if (decimal.Parse(obj.ToString()) < 0)  // exception causes transaction to be rolled back                             
                        throw new Exception("Insufficient funds in Checking Account - rolling back transaction");

                    int result = 0;

                    result = AddToTransactionHistoryTR(checkingAccountNum, billId, (decimal)amount, 102, 0, conn, sqtr, true); //102 for tanstype for billpay

                    if (result > 0)
                    {
                        result = UpdateBillsPaymentStatus(billId, conn, sqtr, true);
                        sqtr.Commit();
                        res = true;
                        // clear the cache                     
                        CacheAbstraction cabs = new CacheAbstraction();
                        cabs.Remove("TRHISTORY");
                    }
                    else
                    {
                        throw new Exception("Error in Updating the transaction history");
                    }
                }
                else
                {
                    throw new Exception("Error in Updating the checking Balance");
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                conn.Close();
            }

            return res;
        }

        public List<Loan> getAllUnapprovedLoans()
        {
            string sql = "select * from Loan where Status = 'UA'";
            List<Loan> loanList = new List<Loan>();
            try
            { 
                DataTable dt = _idac.GetManyRowsCols(sql);
                loanList = DBList.ToList<Loan>(dt);
            }
            catch (Exception)
            {
                throw;
            }

            return loanList;
        }

        public List<Loan> getAllLoans()
        {
            string sql = "select * from Loan";
            List<Loan> loanList = new List<Loan>();
            try
            {
                DataTable dt = _idac.GetManyRowsCols(sql);
                loanList = DBList.ToList<Loan>(dt);
            }
            catch (Exception)
            {
                throw;
            }

            return loanList;
        }

        public List<Loan> getAllLoansForUser(string username)
        {
            string sql = "select * from Loan where UserName = @Username";
            List<Loan> loanList = new List<Loan>();
            try
            {
                List<DbParameter> paramList = new List<DbParameter>();
                SqlParameter p1 = new SqlParameter("@Username", SqlDbType.VarChar, 50);
                p1.Value = username;
                paramList.Add(p1);
                DataTable dt = _idac.GetManyRowsCols(sql, paramList);
                loanList = DBList.ToList<Loan>(dt);
            }
            catch (Exception)
            {
                throw;
            }

            return loanList;
        }

        public int applyForLoan(string username, string desc, decimal? amt)
        {
            int rows = 0;
            try
            {
                string sql = "insert into Loan(LoanName,LoanAmt,UserName,Status) values (@desc,@amount,@Username,'UA')";
                List<DbParameter> ParamList = new List<DbParameter>();
                SqlParameter p1 = new SqlParameter("@desc", SqlDbType.Text);
                p1.Value = desc;
                ParamList.Add(p1);
                SqlParameter p2 = new SqlParameter("@amount", SqlDbType.Money);
                p2.Value = amt;
                ParamList.Add(p2);
                SqlParameter p3 = new SqlParameter("@Username", SqlDbType.VarChar,50);
                p3.Value = username;
                ParamList.Add(p3);
                rows = _idac.InsertUpdateDelete(sql, ParamList);
            }
            catch (Exception)
            {
                throw;
            }
            return rows;
        }

        public Loan getLoan(int loanid)
        {
            string sql = "select * from Loan where LoanId = @loanid";
            List<Loan> loanlist = new List<Loan>();
            Loan loan = null;
            try
            {
                List<DbParameter> paramList = new List<DbParameter>();
                SqlParameter p1 = new SqlParameter("@loanid", SqlDbType.Int);
                p1.Value = loanid;
                paramList.Add(p1);
                DataTable dt = _idac.GetManyRowsCols(sql, paramList);
                loanlist = DBList.ToList<Loan>(dt);
                if(loanlist.Count > 0)
                {
                    loan = loanlist[0];
                }
            }
            catch (Exception)
            {
                throw;
            }

            return loan;
        }

        public bool checkEligibilityforLoan(String username, int loanId)
        {
            bool res = false;
            try
            {
                Loan loanApplied = getLoan(loanId);
                long acctnum = GetSavingAccountNumForUser(username);
                decimal bal = GetSavingBalance(acctnum);

                if ((loanApplied != null)&&((loanApplied.LoanAmt * (decimal)0.2) < bal))
                {
                    res = true;
                }
            }
            catch (Exception)
            {
                throw;
            }
            

            return res;
        }

        public int approveLoan(int loanId)
        {
            int rows = 0;
            try
            {
                string sql = "Update Loan set Status='A' where LoanId=@loanid";
                List<DbParameter> ParamList = new List<DbParameter>();
                SqlParameter p1 = new SqlParameter("@loanid", SqlDbType.Int);
                p1.Value = loanId;
                ParamList.Add(p1);
                rows = _idac.InsertUpdateDelete(sql, ParamList);
            }
            catch (Exception)
            {
                throw;
            }
            return rows;
        }
    }
}