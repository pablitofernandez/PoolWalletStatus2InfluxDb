using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoolWalletStatus2InfluxDb.Model
{
    public class ZpoolStatus
    {
        public string currency { get; set; }
        public int unsold { get; set; }
        public double balance { get; set; }
        public double unpaid { get; set; }
        public double paid24h { get; set; }
        public double total { get; set; }
        public Miner[] miners { get; set; }
    }

}
