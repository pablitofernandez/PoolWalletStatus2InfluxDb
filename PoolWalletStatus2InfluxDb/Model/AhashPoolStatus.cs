using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoolWalletStatus2InfluxDb.Model
{
    public class AhashPoolStatus
    {
        public string currency { get; set; }
        public double unsold { get; set; }
        public double balance { get; set; }
        public double total_unpaid { get; set; }
        public double total_paid { get; set; }
        public double total_earned { get; set; }
        public int miners_count { get; set; }
        public Miner[] miners { get; set; }
    }

    public class Miner
    {
        public string version { get; set; }
        public string password { get; set; }
        public string ID { get; set; }
        public string algo { get; set; }
        public float difficulty { get; set; }
        public int subscribe { get; set; }
        public float accepted { get; set; }
        public float rejected { get; set; }
    }

}
