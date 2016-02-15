using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace StockSimulator
{
    public class GameLogic
    {
        public Exchange NYSE;

        public GameLogic()
        {

        }
    }

    /// <summary>
    /// Static class to store misc static methods
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Converts a MetaStock formatted date ("YYYYMMDD") into a DateTime object
        /// </summary>
        /// <param name="date">The date string</param>
        /// <returns>A DateTime object containing the date from the date string</returns>
        public static DateTime toDate(string date)
        {
            int year = Convert.ToInt32(
                date.Substring(0, 4) //get first 4 digits of date string and convert to int
            );
            int month = Convert.ToInt32(
                date.Substring(4, 2)
            );
            int day = Convert.ToInt32(
                date.Substring(6, 2)
            );

            return new DateTime(year, month, day);
        }

        /// <summary>
        /// Converts a string array of the component parts into a StockRow object
        /// </summary>
        /// <param name="data">The string array containing [symbol, date, open, high, low, close, volume]</param>
        /// <returns>A StockRow object for the specified data</returns>
        public static StockRow rowMaker(string[] data)
        {
            StockRow row = new StockRow(
                    Utilities.toDate(data[1]),
                    Convert.ToDecimal(data[2]),
                    Convert.ToDecimal(data[3]),
                    Convert.ToDecimal(data[4]),
                    Convert.ToDecimal(data[5]),
                    Convert.ToInt32(data[6])
                );

            return row;
        }

        /// <summary>
        /// Method to read in the API response, parse it into StockRow objects and add it to the specified exchange
        /// </summary>
        /// <param name="response">The response from the API</param>
        /// <returns>A list of stockrow objects from the API response</returns>
        public static void arrayify(string symbol, string response, Exchange ex)
        {
            Regex regex = new Regex(@"\[([^\[].*?[^\]])\]"); //split response into the subarrays
            MatchCollection matches = regex.Matches(response); //get all matches

            ex.Add(symbol); //ensure there exists a ticker for the symbol

            foreach (Match m in matches)
            {
                string[] splitted = m.ToString().Split(','); //split the match into each component

                for (int i = 0; i < splitted.Length; i++) //trim redundant characters from each component
                {
                    splitted[i] = splitted[i].Trim(new Char[] { '[', ' ', ']', '"' });
                }

                DateTime date = DateTime.Parse(splitted[0]);

                //parse datetime and decimals
                StockRow sr = new StockRow(
                    date,    //date
                    Decimal.Parse(splitted[1]),     //open
                    Decimal.Parse(splitted[2]),     //high
                    Decimal.Parse(splitted[3]),     //low
                    Decimal.Parse(splitted[4]),     //close
                    Convert.ToInt32(Decimal.Parse(splitted[5]))  //volume
                    );

                ex[symbol].Add(date, sr);
            }
        }
    }

    /// <summary>
    /// Static class to interface with the web API
    /// </summary>
    public static class WebInterface
    {
        public static String getPage(string url)
        {
            WebClient web = new WebClient();
            return web.DownloadString(url);
        }

        public static string queryAPI(string symbol)
        {
            string url = "http://philippratt.co.uk:5000/" + symbol;
            return getData(url);
        }
        public static string queryAPI(string symbol, string startDate)
        {
            //format .../symbol/YYYYMMDD
            string url = "http://philippratt.co.uk:5000/" + symbol + "/" + startDate;
            return getData(url);
        }
        public static string queryAPI(string symbol, string startDate, string endDate)
        {
            //format .../symbol/YYYYMMDD/YYYYMMDD
            string url = "http://philippratt.co.uk:5000/" + symbol + "/" + startDate + "/" + endDate;
            return getData(url);
        }

        /// <summary>
        /// Method to download the API response
        /// </summary>
        /// <param name="url">The URL to download</param>
        /// <returns>The string API response</returns>
        private static string getData(string url)
        {
            string data = "";

            try
            {
                data = new WebClient().DownloadString(url);
                return data;
            }
            catch (WebException e)
            {
                Console.Write(e.ToString());
                return null;
            }
        }
    }

    /// <summary>
    /// Static class to interface with files
    /// </summary>
    public static class FileInterface
    {
        /// <summary>
        /// Method to read a MetaStock formatted file and add the data to the specified Exchange
        /// </summary>
        /// <param name="path">The full path to the file to add</param>
        /// <param name="ex">The Exchange for the data to be added to</param>
        public static void readFile(string path, Exchange ex)
        {
            string symbol = "";
            using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (BufferedStream bs = new BufferedStream(fs))
            using (StreamReader sr = new StreamReader(bs))
            {
                string line;

                if ((line = sr.ReadLine()) == null) //read first line and test if not-null
                {
                    return; //if first line is null
                }
                else
                {
                    if (line[0] == '<') //test if header row is present
                    {
                        if ((line = sr.ReadLine()) == null)
                        {
                            return; //if header row is present and next row is null, return as no data to process
                        }
                    }

                    //get stock symbol and ensure ticker is ready
                    symbol = line.Split(',')[0];
                    ex.Add(symbol);

                    do
                    {
                        ex[symbol].AddDay(line); //add lines while there are lines to add
                    } while ((line = sr.ReadLine()) != null);
                }
            }
        }

        /// <summary>
        /// Method to write the specified exchange to a metastock formatted file
        /// </summary>
        /// <param name="path">The path of the file to write to</param>
        /// <param name="ex">The exchange to save</param>
        public static void writeExchangeToFile(string path, Exchange ex)
        {
            string header = "<ticker>,<date>,<open>,<high>,<low>,<close>,<vol>";

            using (FileStream fs = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
            using (BufferedStream bs = new BufferedStream(fs))
            using (StreamWriter sw = new StreamWriter(bs))
            {
                foreach (KeyValuePair<string, Ticker> t in ex)
                { //iterate through symbols in exchnage
                    string symbol = t.Key;

                    foreach (KeyValuePair<DateTime, StockRow> sr in t.Value)
                    { //iterate through each day's data for each symbol
                        string date = sr.Key.ToString("YYYYMMDD");

                        string high = sr.Value.high.ToString(); //convert to string from decimal
                        string low = sr.Value.low.ToString();
                        string open = sr.Value.open.ToString();
                        string close = sr.Value.close.ToString();
                        string volume = sr.Value.volume.ToString();

                        string toWrite = symbol + "," + date + "," + open + "," + high + "," + low + "," + close + "," + volume;

                        sw.WriteLine(toWrite);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Class to store all exchange data.
    /// SortedDictionary to speed up symbol access when large amounts of stocks are loaded
    /// </summary>
    public class Exchange : SortedDictionary<string, Ticker>
    {
        /// <summary>
        /// Adds a new Ticker (symbol, e.g. AAPL) to the Exchange
        /// </summary>
        /// <param name="symbol">The stock symbol (e.g. AAPL) to add</param>
        public void Add(string symbol)
        {
            if (!this.ContainsKey(symbol))
            { //ensure no duplicate symbols are added
                this.Add(symbol, new Ticker());
            }
        }

        /// <summary>
        /// Returns a list of the KVPairs contained in the specified Ticker, ordered by the specified property (high to low)
        /// </summary>
        /// <param name="symbol">The symbol's data to extract</param>
        /// <param name="property">The property to sort on (open,high,low,close,volume)</param>
        /// <returns>The sorted list of KVPs</returns>
        public List<KeyValuePair<DateTime, StockRow>> getSorted(string symbol, string property)
        {
            IOrderedEnumerable<KeyValuePair<DateTime, StockRow>> sortedDic;

            switch (symbol)
            {
                case "high":
                    sortedDic = from entry in this[symbol] orderby entry.Value.high descending select entry;
                    break;
                case "low":
                    sortedDic = from entry in this[symbol] orderby entry.Value.low descending select entry;
                    break;
                case "close":
                    sortedDic = from entry in this[symbol] orderby entry.Value.high descending select entry;
                    break;
                case "volume":
                    sortedDic = from entry in this[symbol] orderby entry.Value.high descending select entry;
                    break;
                case "open":
                default:
                    sortedDic = from entry in this[symbol] orderby entry.Value.open descending select entry;
                    break;
            }

            return new List<KeyValuePair<DateTime, StockRow>>(sortedDic);
        }
    }

    /// <summary>
    /// Class to store the individual Symbol's stock data.
    /// SortedDictionary to speed up day access, as well to keep in day order for speed
    /// </summary>
    public class Ticker : SortedDictionary<DateTime, StockRow>
    {
        /// <summary>
        /// Adds a day's data to the Ticker.
        /// If faced with duplicate data, it will remove the old data and replace with the new.
        /// </summary>
        /// <param name="data">The MetaStock formatted line to add to the Ticker</param>
        public void AddDay(string data)
        {
            string[] split = data.Split(',');
            StockRow row = Utilities.rowMaker(split);

            try
            {
                this.Add(row.date, row);
            }
            catch (ArgumentException e)
            {
                e.ToString();

                this.Remove(row.date); //catching duplicate data -will replace existing data with new data
                this.Add(row.date, row); //large performance hit ~81x expected performance
            }
        }
    }

    /// <summary>
    /// A struct to store an individual day's data
    /// </summary>
    public struct StockRow
    {
        public DateTime date { get; }
        public decimal open { get; }
        public decimal close { get; }
        public decimal high { get; }
        public decimal low { get; }
        public int volume { get; }

        /// <summary>
        /// Constructor for the individual values
        /// </summary>
        /// <param name="day">The date as a proper DateTime object</param>
        /// <param name="op">The open price</param>
        /// <param name="hi">The day's high price</param>
        /// <param name="lo">The day's low price</param>
        /// <param name="cl">The close price</param>
        /// <param name="vol">The volume available</param>
        public StockRow(DateTime day, decimal op, decimal hi, decimal lo, decimal cl, int vol)
        {
            date = day;
            open = op;
            close = cl;
            high = hi;
            low = lo;
            volume = vol;
        }
    }
}