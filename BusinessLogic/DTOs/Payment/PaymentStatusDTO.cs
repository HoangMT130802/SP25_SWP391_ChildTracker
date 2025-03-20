using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DTOs.Payment
{
    public class PaymentStatusDTO
    {
        public bool Success { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public int? TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string MembershipName { get; set; }
        public bool? MembershipStatus { get; set; }
    }
}
