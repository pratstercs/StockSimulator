using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator {
    public class GameLogic
    {
	    public GameLogic()
	    {
	    }

        /// <summary>
        /// Method to read a MetaStock formatted file and add the data to the specified Exchange
        /// </summary>
        /// <param name="path">The full path to the file to add</param>
        /// <param name="ex">The Exchange for the data to be added to</param>
        public void readFile(string path, Exchange ex)
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
            if (!this.ContainsKey(symbol)) { //ensure no duplicate symbols are added
                this.Add(symbol, new Ticker());
            }
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
                this.Remove(row.date); //catching duplicate data -will replace existing data with new data
                this.Add(row.date, row); //large performance hit ~81x expected performance
            }
        }
    }

    /// <summary>
    /// Abstract class to store useful static methods
    /// </summary>
    public abstract class Utilities
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
    }

    /// <summary>
    /// A struct to store an individual day's data
    /// </summary>
    public struct StockRow
    {
        public DateTime date;
        public decimal open;
        public decimal close;
        public decimal high;
        public decimal low;
        public int volume;

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