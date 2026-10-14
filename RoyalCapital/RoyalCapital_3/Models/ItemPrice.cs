namespace RoyalCapital_3.Models
{
    public class ItemPrice
    {
        public string MKISTAT_INSTRUMENT_CODE {  get; set; }
        public decimal? Last_Trade_Price { get; set; }
        public decimal? MKISTAT_CLOSE_PRICE { get; set; }
        public decimal? MKISTAT_YDAY_CLOSE_PRICE { get; set; }
        public decimal? Deviation { get; set; }
    }
}
