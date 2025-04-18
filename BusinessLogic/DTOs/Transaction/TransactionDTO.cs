﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DTOs.Transaction
{
    public class TransactionDTO
    {
        public int TransactionId { get; set; }
        public int UserId { get; set; }
        public int UserMembershipId { get; set; }
        public string MembershipName { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string TransactionCode { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
