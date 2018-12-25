using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MorningBank.Models.ViewModels
{
    public class PayBills
    {
        public List<BillPayment> billPayments { get; set; }
        public int selectedBill { get; set; }
    }
}