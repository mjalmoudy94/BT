using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BT.Model
{
    public class AnalyzeInfo
    {
        public decimal CurrentPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public decimal MinPrice { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal ChangeRange { get; set; }
        ////
        public AnalyzeInfo()
        {
            CurrentPrice = 0;
            MaxPrice = 0;
            MinPrice = 0;
            AveragePrice = 0;
            ChangeRange = 0;
        }
    }
}