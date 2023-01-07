﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BloodDonationApp.Models
{
    public class BloodBankStockMV
    {
        public int BloodBankStockID { get; set; }
        public int BloodBankID { get; set; }
        public string BloodBank{ get; set; }
        [Display(Name = "Blood Bank")]
        public int BloodGroupID { get; set; }
        [Display(Name = "Blood Group")]
        public String BloodGroup { get; set; }
        public double Quantity { get; set; }
        [Display(Name = "Is Ready")]
        public string Status { get; set; }
        [Display(Name ="Blood Details")]
        public string Description { get; set; }
    }
}