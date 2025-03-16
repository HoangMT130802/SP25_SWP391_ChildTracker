using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DTOs.Payment
{
    public class PaymentRequestDTO
    {
        public int MembershipId { get; set; }
        public int UserId { get; set; }
        public string ReturnUrl { get; set; }
        public string CancelUrl { get; set; }
    }

}
