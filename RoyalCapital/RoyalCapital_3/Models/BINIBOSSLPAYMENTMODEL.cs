using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using static RoyalCapital_3.SSLCommerz;

namespace RoyalCapital_3.Models
{
    public class SSLCommerzInitResponse
    {
        public string status { get; set; }
        public string failedreason { get; set; }
        public string sessionkey { get; set; }
        public Gw gw { get; set; }
        public string GatewayPageURL { get; set; }
        public string storeBanner { get; set; }
        public string storeLogo { get; set; }
        public List<Desc> desc { get; set; }
    }

    public class SSLCommerzValidatorResponse
    {
        public string status { get; set; }
        public string tran_date { get; set; }
        public string tran_id { get; set; }
        public string val_id { get; set; }
        public string amount { get; set; }
        public string store_amount { get; set; }
        public string currency { get; set; }
        public string bank_tran_id { get; set; }
        public string card_type { get; set; }
        public string card_no { get; set; }
        public string card_issuer { get; set; }
        public string card_brand { get; set; }
        public string card_issuer_country { get; set; }
        public string card_issuer_country_code { get; set; }
        public string currency_type { get; set; }
        public string currency_amount { get; set; }
        public string currency_rate { get; set; }
        public string base_fair { get; set; }
        public string value_a { get; set; }
        public string value_b { get; set; }
        public string value_c { get; set; }
        public string value_d { get; set; }
        public string emi_instalment { get; set; }
        public string emi_amount { get; set; }
        public string emi_description { get; set; }
        public string emi_issuer { get; set; }
        public string risk_title { get; set; }
        public string risk_level { get; set; }
        public string APIConnect { get; set; }
        public string validated_on { get; set; }
        public string gw_version { get; set; }
        public string discount_amount { get; set; }
        public string discount_percentage { get; set; }
        public string discount_remarks { get; set; }
    }

    public class SSLCommerzInitRequest
    {
        public string total_amount { get; set; }
        public string currency { get; set; }
        public string tran_id { get; set; }
        public string success_url { get; set; }
        public string fail_url { get; set; }
        public string cancel_url { get; set; }
        public string cus_name { get; set; }
        public string cus_email { get; set; }
        public string cus_add1 { get; set; }
        public string cus_city { get; set; }
        public string cus_postcode { get; set; }
        public string cus_country { get; set; }
        public string cus_phone { get; set; }
        public string shipping_method { get; set; }
        public string product_name { get; set; }
        public string product_category { get; set; }
        public string product_profile { get; set; }
    }

    public class AjaxRequestModel
    {
        public string order { get; set; }
        public string cart_json { get; set; }
    }

    public class BINIBOPaymentRequest
    {
        public string TrackingNo { get; set; }
        public string Mobile { get; set; }
        public string CustomerName { get; set; }
        public string Email { get; set; }
    }

    public class BINIBOPaymentResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string TranId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string PaymentUrl { get; set; }
    }
}
