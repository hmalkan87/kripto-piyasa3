using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VcmDeneme.Entities
{
    public class Ticker
    {
        public string Market { get; set; }
        public string Trade_price { get; set; }
        public string Acc_trade_price_24h { get; set; }
    }
}