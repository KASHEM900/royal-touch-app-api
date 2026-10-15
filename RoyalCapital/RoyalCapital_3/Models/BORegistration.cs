using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RoyalCapital_3.Models
{
    public class BORegistrationModel
    {
        public string TrackingNo { get; set; }

        [Display(Name = "First Name *")]
        public string FirstName { get; set; }


        [Display(Name = "Last Name *")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email-ID *")]
        public string Email { get; set; }

        [Display(Name = "Mobile Number:")]
        [Required(ErrorMessage = "Mobile Number is required.")]
        [RegularExpression(@"^(\+88)?01[0-9]{9}$", ErrorMessage = "Enter a valid Bangladeshi mobile number.")]
        public string Mobile { get; set; }


        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string PromoCode { get; set; }

        public string Register { get; set; }
    }

}
