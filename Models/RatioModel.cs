using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arbitrage01.Models
{
    public class RatioModel
    {
        public string Symbol { get; set; }
        public string LastPrice { get; set; }
        public string QuoteVolume { get; set; }
        public string Value1 { get; set; }
        public string Value2 { get; set; }
        public string Value3 { get; set; }
        public string Value4 { get; set; }
        public string XxxEth { get; set; }
        public string XxxUsdt { get; set; }
        public string XxxDkkt { get; set; }
        public string Difference { get; set; }
        public string ResultValue { get; set; }
    }
}
