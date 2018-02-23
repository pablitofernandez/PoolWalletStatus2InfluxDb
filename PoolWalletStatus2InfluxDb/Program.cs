using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AdysTech.InfluxDB.Client.Net;
using PoolWalletStatus2InfluxDb.Model;

namespace PoolWalletStatus2InfluxDb
{
    class Program
    {

        private const string ZpoolApiURL = "https://www.zpool.ca/api/walletEx?address=";
        private const string AhashpoolApiURL = "https://www.ahashpool.com/api/walletEx/?address=";

        static void Main(string[] args)
        {
            string sSource;
            string sLog;
            string sEvent;

            sSource = "PoolWalletStatus2InfluxDb";
            sLog = "Application";

            if (!EventLog.SourceExists(sSource))
                EventLog.CreateEventSource(sSource, sLog);

            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                var btcAddress = appSettings["BTCAddress"];


                var zpoolUrl = ZpoolApiURL + btcAddress;
                var ahashpoolUrl = AhashpoolApiURL + btcAddress;

                var request = WebRequest.Create(zpoolUrl);
                request.ContentType = "application/json; charset=utf-8";
                string text;
                var response = (HttpWebResponse)request.GetResponse();
                Console.WriteLine("Reading json data from AwesomeMiner api");
                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    text = sr.ReadToEnd();
                }

                ZpoolStatus zpoolStatus = Newtonsoft.Json.JsonConvert.DeserializeObject<ZpoolStatus>(text);

                Console.WriteLine("Json received and parsed");
                var influxUrl = appSettings["InfluxDbHost"];
                var influxDbName = appSettings["InfluxDbName"];

                Console.WriteLine($"Connecting to Influxdb @ {influxUrl} DBName: {influxDbName}");
                InfluxDBClient client = new InfluxDBClient($"http://{influxUrl}");
                client.CreateDatabaseAsync(influxDbName).Wait();
                var utcNow = DateTime.UtcNow;
                var valMixed = new InfluxDatapoint<InfluxValueField>();
                valMixed.UtcTimestamp = utcNow;
                valMixed.Fields.Add("currency", new InfluxValueField(zpoolStatus.currency));
                valMixed.Fields.Add("unsold", new InfluxValueField(zpoolStatus.unsold));
                valMixed.Fields.Add("balance", new InfluxValueField(zpoolStatus.balance));
                valMixed.Fields.Add("unpaid", new InfluxValueField(zpoolStatus.unpaid));
                valMixed.Fields.Add("paid24h", new InfluxValueField(zpoolStatus.paid24h));
                valMixed.Fields.Add("total", new InfluxValueField(zpoolStatus.total));

                valMixed.MeasurementName = "Zpool";

                Console.WriteLine("Trying to write zpoool data to DB");
                client.PostPointAsync(influxDbName, valMixed).Wait();

                request = WebRequest.Create(ahashpoolUrl);
                request.ContentType = "application/json; charset=utf-8";
                response = (HttpWebResponse)request.GetResponse();
                Console.WriteLine("Reading json data from AwesomeMiner api");
                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    text = sr.ReadToEnd();
                }

                AhashPoolStatus ahashPoolStatus = Newtonsoft.Json.JsonConvert.DeserializeObject<AhashPoolStatus>(text);

                client.CreateDatabaseAsync(influxDbName).Wait();
                valMixed = new InfluxDatapoint<InfluxValueField>();
                valMixed.UtcTimestamp = utcNow;
                valMixed.Fields.Add("currency", new InfluxValueField(ahashPoolStatus.currency));
                valMixed.Fields.Add("unsold", new InfluxValueField(ahashPoolStatus.unsold));
                valMixed.Fields.Add("balance", new InfluxValueField(ahashPoolStatus.balance));
                valMixed.Fields.Add("total_unpaid", new InfluxValueField(ahashPoolStatus.total_unpaid));
                valMixed.Fields.Add("total_paid", new InfluxValueField(ahashPoolStatus.total_paid));
                valMixed.Fields.Add("total_earned", new InfluxValueField(ahashPoolStatus.total_earned));

                valMixed.MeasurementName = "Ahashpool";

                Console.WriteLine("Trying to write ahashpool data to DB");
                client.PostPointAsync(influxDbName, valMixed).Wait();

            }
            catch (Exception e)
            {
                EventLog.WriteEntry(sSource, e.Message, EventLogEntryType.Error);
            }

        }
    }
}
