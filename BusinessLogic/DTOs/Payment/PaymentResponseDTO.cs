﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DTOs.Payment
{
    public class PaymentResponseDTO
    {
        public string PaymentUrl { get; set; }
        public string OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string ReturnUrl { get; set; }
        public string CancelUrl { get; set; }
    }
}
