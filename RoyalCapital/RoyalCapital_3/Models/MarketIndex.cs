using System;

namespace RoyalCapital_3.Models
{
    public class MarketIndex
    {
        public string IDX_INDEX_ID {  get; set; }
        public DateTime IDX_DATE_TIME { get; set; }
        public decimal? IDX_CAPITAL_VALUE { get; set; }
        public decimal? IDX_DEVIATION { get; set; }
        public decimal? lDX_PERCENTAGE_DEVIATION { get; set; }
    }
}
