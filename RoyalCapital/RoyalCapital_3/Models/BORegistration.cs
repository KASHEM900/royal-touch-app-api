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

        //[Required(ErrorMessage = "First Name is required.")]
        [Display(Name = "First Name *")]
        public string FirstName { get; set; }

        //[Required(ErrorMessage = "Last Name is required.")]
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

        //[Required(ErrorMessage = "Password is required.")]
        //[StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string PromoCode { get; set; }
    }

}
