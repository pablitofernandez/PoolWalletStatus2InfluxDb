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
        private const string ZergpoolApiURL = "http://api.zergpool.com:8080/api/walletEx?address=";

        static void Main(string[] args)
        {
            string sSource;
            string sLog;
            string sEvent;

            sSource = "PoolWalletStatus2InfluxDb";
            sLog = "Application";

            if (!EventLog.SourceExists(sSource))
                EventLog.CreateEventSource(sSource, sLog);

            var appSettings = ConfigurationManager.AppSettings;
            var btcAddress = appSettings["BTCAddress"];


            var zpoolUrl = ZpoolApiURL + btcAddress;
            var ahashpoolUrl = AhashpoolApiURL + btcAddress;
            var zergpoolUrl = ZergpoolApiURL + btcAddress;

            var influxUrl = appSettings["InfluxDbHost"];
            var influxDbName = appSettings["InfluxDbName"];

            var utcNow = DateTime.UtcNow;

            try
            {
                var request = WebRequest.Create(zpoolUrl);
                request.ContentType = "application/json; charset=utf-8";
                string text;
                var response = (HttpWebResponse) request.GetResponse();
                
                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    text = sr.ReadToEnd();
                }

                ZpoolStatus zpoolStatus = Newtonsoft.Json.JsonConvert.DeserializeObject<ZpoolStatus>(text);

                Console.WriteLine("Json received and parsed");


                Console.WriteLine($"Connecting to Influxdb @ {influxUrl} DBName: {influxDbName}");
                InfluxDBClient client = new InfluxDBClient($"http://{influxUrl}");
                client.CreateDatabaseAsync(influxDbName).Wait();
                
                var valMixed = new InfluxDatapoint<InfluxValueField>();
                valMixed.UtcTimestamp = utcNow;
                valMixed.Tags.Add("BTC_Address", btcAddress);
                valMixed.Fields.Add("currency", new InfluxValueField(zpoolStatus.currency));
                valMixed.Fields.Add("unsold", new InfluxValueField(zpoolStatus.unsold));
                valMixed.Fields.Add("balance", new InfluxValueField(zpoolStatus.balance));
                valMixed.Fields.Add("total_unpaid", new InfluxValueField(zpoolStatus.unpaid));
                valMixed.Fields.Add("total_paid", new InfluxValueField(zpoolStatus.paid24h));
                valMixed.Fields.Add("total_earned", new InfluxValueField(zpoolStatus.total));

                valMixed.MeasurementName = "Zpool";

                Console.WriteLine("Trying to write zpool data to DB");
                client.PostPointAsync(influxDbName, valMixed).Wait();
            }
            catch (Exception e)
            {
                EventLog.WriteEntry(sSource, "Error while writing zpool data: " + e.Message, EventLogEntryType.Error);
            }
            try
            {
                var request = WebRequest.Create(ahashpoolUrl);
                request.ContentType = "application/json; charset=utf-8";
                string text;
                var response = (HttpWebResponse) request.GetResponse();
                
                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    text = sr.ReadToEnd();
                }

                AhashPoolStatus ahashPoolStatus = Newtonsoft.Json.JsonConvert.DeserializeObject<AhashPoolStatus>(text);
                InfluxDBClient client = new InfluxDBClient($"http://{influxUrl}");
                client.CreateDatabaseAsync(influxDbName).Wait();
                var valMixed = new InfluxDatapoint<InfluxValueField>();
                valMixed = new InfluxDatapoint<InfluxValueField>();
                valMixed.UtcTimestamp = utcNow;
                valMixed.Tags.Add("BTC_Address", btcAddress);
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
                EventLog.WriteEntry(sSource, "Error while writing ahashpool data: " + e.Message, EventLogEntryType.Error);
            }
            try { 
                var request = WebRequest.Create(zergpoolUrl);
                request.ContentType = "application/json; charset=utf-8";
                string text;
                var response = (HttpWebResponse)request.GetResponse();
                
                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    text = sr.ReadToEnd();
                }

                ZpoolStatus zergpoolStatus = Newtonsoft.Json.JsonConvert.DeserializeObject<ZpoolStatus>(text);
                InfluxDBClient client = new InfluxDBClient($"http://{influxUrl}");
                client.CreateDatabaseAsync(influxDbName).Wait();
                var valMixed = new InfluxDatapoint<InfluxValueField>();
                valMixed = new InfluxDatapoint<InfluxValueField>();
                valMixed.UtcTimestamp = utcNow;
                valMixed.Tags.Add("BTC_Address", btcAddress);
                valMixed.Fields.Add("currency", new InfluxValueField(zergpoolStatus.currency));
                valMixed.Fields.Add("unsold", new InfluxValueField(zergpoolStatus.unsold));
                valMixed.Fields.Add("balance", new InfluxValueField(zergpoolStatus.balance));
                valMixed.Fields.Add("total_unpaid", new InfluxValueField(zergpoolStatus.unpaid));
                valMixed.Fields.Add("total_paid", new InfluxValueField(zergpoolStatus.paid24h));
                valMixed.Fields.Add("total_earned", new InfluxValueField(zergpoolStatus.total));

                valMixed.MeasurementName = "Zergpool";

                Console.WriteLine("Trying to write zergpool data to DB");
                client.PostPointAsync(influxDbName, valMixed).Wait();

            }
            catch (Exception e)
            {
                EventLog.WriteEntry(sSource, e.Message, EventLogEntryType.Error);
            }

        }
    }
}
