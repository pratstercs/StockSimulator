﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

//TODO: ensure date is a working day - i.e. has data (NullPointer try/catch?)
//TODO: handle bank holidays - days where data is missing
//TODO: expand to hourly, rather than daily. Use Utilties.generatePrices, the returned decimal array

namespace StockSimulator
{
    public class GameLogic
    {
        public Exchange ex; // { get; private set; }
        public List<Stock> wallet = new List<Stock>(); // { get; private set; }
        public decimal cash; // { get; private set; }
        public Dictionary<string,StockChange> changes = new Dictionary<string, StockChange>();
        public Dictionary<string, string> names = new Dictionary<string, string>();

        public DateTime currentDate;
        public DateTime CurrentDate
        {
            get
            {
                return currentDate;
            }
            set
            {
                currentDate = value;
            }
        }

        /// <summary>
        /// Empty exchange constructor
        /// </summary>
        public GameLogic()
                {
                    ex = new Exchange();

                    currentDate = DateTime.Now;
                    currentDate = currentDate.AddMonths(-1);
                }

        /// <summary>
        /// Constructor for an empty exchange but a specified start date
        /// </summary>
        /// <param name="date">The date to start on</param>
        /// <param name="money">Starting cash</param>
        public GameLogic(DateTime date, decimal money)
        {
            ex = new Exchange();
            currentDate = date;
            cash = money;
        }

        /// <summary>
        /// Constructor for pre-configured scenario
        /// </summary>
        /// <param name="e">The prebuilt exchange to use</param>
        /// <param name="time">The date on which to start the scenario</param>
        public GameLogic(Exchange e, DateTime time)
        {
            currentDate = time;
            ex = e;
        }

        /// <summary>
        /// Method to get data from the API and add it to the Exchange
        /// </summary>
        /// <param name="symbol">The symbol for which to get data</param>
        /// <param name="startDate">The date to start the data on</param>
        /// <param name="endDate">The date to end the data on</param>
        /// <returns>Whether the operation was successful</returns>
        public bool getData(string symbol, string startDate, string endDate)
        {
            try
            {
                string response = WebInterface.queryAPI(symbol, startDate, endDate);
                Utilities.arrayify(symbol, response, ex);
                return true;
            }
            catch (WebException e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        /// <summary>
        /// Method to buy stocks from the exchange and add to the user's wallet
        /// </summary>
        /// <param name="date">The date of the data to use when buying</param>
        /// <param name="symbol">The stock symbol to purchase</param>
        /// <param name="amount">The number of stocks to purchase</param>
        /// <returns>Bool whether the purchase completed successfully</returns>
        public bool buyStock(DateTime date, string symbol, int amount)
        {
            StockRow sr = ex[symbol][date];

            decimal totalCost = sr.close * amount; //close? not open?
            if (cash < totalCost || amount > sr.volume) //totalCost is more than available cash or amount wanting to purchase is greater than volume available
            {
                return false;
            }
            else
            {
                wallet.Add(
                    new Stock(
                        symbol,
                        date,
                        sr.close,
                        amount
                        )
                    );
                //ex[symbol][date].volume -= amount; //can't modify directly for some reason. Will leave as edge case - unlikely someone will want to buy more than is available of a company
                cash -= totalCost;

                return true;
            }
        }

        /// <summary>
        /// Method to sell stocks from the wallet to the exchange
        /// </summary>
        /// <param name="date">The date of the data to use when buying</param>
        /// <param name="symbol">The stock symbol to purchase</param>
        /// <param name="amount">The number of stocks to purchase</param>
        /// <returns>Bool whether the sale completed successfully</returns>
        public bool sellStock(DateTime date, string symbol, int amount)
        {
            IOrderedEnumerable<Stock> currentStocks = from entry in wallet where entry.symbol == symbol orderby entry.amount ascending select entry; //extract matching stocks from wallet

            int amountHeld = 0;

            foreach (Stock s in currentStocks)
            {
                amountHeld += s.amount;
            }

            if (amount <= amountHeld) //if amountToSell is less than or equal to the amount held
            {
                int remainingToSell = amount;
                decimal cost = ex[symbol][date].close * amount; //calculate total cost of sale

                IEnumerator<Stock> e = currentStocks.GetEnumerator();
                e.MoveNext(); //move enumerator to first element in collection
                while (remainingToSell > 0)
                {
                    Stock current = e.Current;

                    if (current.amount > remainingToSell) //current stock chunk is enough to cover sale
                    {
                        current.amount -= remainingToSell; //remove the sale amount from the current chunk
                        remainingToSell = 0; //no more still to sell 
                    }
                    else //need to use next chunk of stocks
                    {
                        wallet.Remove(current); //remove the chunk just sold from the wallet
                        remainingToSell -= current.amount; //decrement remaining by the chunk just sold

                        e.MoveNext(); //move enumerator to the next element
                    }
                }

                //ex["AAPL"][date].volume += amount; //can't edit directly, but edge case - will user want to buy back stocks on the same day?
                cash += cost; //add cost of sale to cash balance

                return true;
            }
            else //if amountToSell is greater than the amount held
            {
                return false;
            }
        }

        /// <summary>
        /// Method to compare the purchase prices of the user's held stocks with the current sale price
        /// </summary>
        /// <param name="date">The date to class as "current"</param>
        /// <returns>A decimal array containing the numerical and percentage changes</returns>
        public decimal[] calculateProfitLoss(DateTime date)
        {
            decimal totalCost = 0;
            decimal currentPrice = 0;

            foreach (Stock purchase in wallet)
            {
                currentPrice += (ex[purchase.symbol][date].close * purchase.amount);
                totalCost += (purchase.purchasePrice * purchase.amount);
            }

            decimal profit = currentPrice - totalCost;
            decimal percProfit = (profit / totalCost) * 100;
            profit = Decimal.Round(profit, 2);
            percProfit = Decimal.Round(percProfit, 2);

            return new decimal[2] { profit, percProfit };
        }

        /// <summary>
        /// Method to increment the time by the hours from testWorkingDay
        /// </summary>
        public void incrementTime()
        {
            int hoursToIncrement = Utilities.testWorkingDay(currentDate);
            currentDate = currentDate.AddHours(hoursToIncrement);
        }

        /// <summary>
        /// Method to get a 2D decimal array for all the stock prices in a specified range
        /// </summary>
        /// <param name="symbol">The symbol for the data to extract</param>
        /// <param name="start">The DateTime of the start point</param>
        /// <param name="end">The DateTime of the end point</param>
        /// <returns>A 2D decimal array (rows: open, high, low, close) of the prices</returns>
        public decimal[][] getStockDataByDay(string symbol, DateTime start, DateTime end)
        {
            //TODO: Add code to generate inter-day data?
            var values = from entry in ex[symbol] where (entry.Key > start && entry.Key < end) select entry; //get values from the ticker within the specified range

            decimal[][] toReturn = new decimal[4][]; //open,high,low,close
            for(int x = 0; x < 4; x++)
            {
                toReturn[x] = new decimal[values.Count()]; //make the correct number of array spaces
            }

            int i = 0;
            foreach(var x in values)
            {
                toReturn[0][i] = x.Value.open; //get each data stream to its own array
                toReturn[1][i] = x.Value.high;
                toReturn[2][i] = x.Value.low;
                toReturn[3][i] = x.Value.close;

                i++;
            }

            return toReturn;
        }

        /// <summary>
        /// Method to get a datetime array of the dates for the stock values in a specified range
        /// </summary>
        /// <param name="symbol">The symbol for the data to extract</param>
        /// <param name="start">The DateTime of the start point</param>
        /// <param name="end">The DateTime of the end point</param>
        /// <returns>A DateTime array with each element's date</returns>
        public DateTime[] getStockDates(string symbol, DateTime start, DateTime end)
        {
            var values = from entry in ex[symbol] where (entry.Key > start && entry.Key < end) select entry; //get values from the ticker within the specified range
   
            DateTime[] toReturn = new DateTime[values.Count()];

            int i = 0;
            foreach (var x in values)
            {
                toReturn[i] = x.Key;
                i++;
            }

            return toReturn;
        }

        /// <summary>
        /// Method to get the percentage change for a stock
        /// </summary>
        /// <param name="symbol">The symbol to get the change for</param>
        /// <returns>A StockChange object with the change and date</returns>
        public StockChange getChange(string symbol)
        {
            StockChange toReturn;

            changes.TryGetValue(symbol, out toReturn); //get value if exists

            if(toReturn.symbol == null || toReturn.date.Date != currentDate.Date) //if does not exist or is out of date
            {
                toReturn = Utilities.refreshStockChange(symbol, currentDate);
                changes.Remove(symbol);
                changes.Add(symbol, toReturn);
            }

            return toReturn;
        }

        /// <summary>
        /// Get percentage changes for all stocks in exchange
        /// </summary>
        public void getAllChanges()
        {
            foreach(string str in ex.Keys)
            {
                getChange(str);
            }
        }

        /// <summary>
        /// Method to get a company name and add it to the list if it isn't already there
        /// </summary>
        /// <param name="symbol">The symbol to get the name for</param>
        /// <returns>The full company name</returns>
        public string getName(string symbol)
        {
            try
            {
                return names[symbol];
            }
            catch
            {
                string name = WebInterface.getName(symbol);
                names.Add(symbol, name);
                return name;
            }
        }

        /// <summary>
        /// Method to get the StockRow value for a symbol and date
        /// </summary>
        /// <param name="symbol">The stock to get</param>
        /// <param name="date">The date's data to fetch</param>
        /// <returns>The StockRow of the requested data</returns>
        public StockRow getValues(string symbol, DateTime date)
        {
            return ex[symbol][date];
        }
    }
    

    /// <summary>
    /// Static class to store misc static methods
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Method to find text between two strings
        /// Method from Oscar Jara, Stack Overflow: http://stackoverflow.com/a/10709874/1188467
        /// </summary>
        /// <param name="strSource"></param>
        /// <param name="strStart"></param>
        /// <param name="strEnd"></param>
        /// <returns></returns>
        public static string getBetween(string strSource, string strStart, string strEnd)
        {
            int Start, End;
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Method to split the name of the company from the full string containing name, symbol and stats available
        /// </summary>
        /// <param name="source">The full source string</param>
        /// <param name="sym">The company's symbol</param>
        /// <returns>The company name without any other stuff</returns>
        public static string getName(string source, string sym)
        {
            string symbol = " (" + sym + ")";
            int end;

            end = source.IndexOf(symbol, 0);
            string name = source.Substring(0, end);

            name = name.Replace("&amp;", "&");

            return name;
        }

        /// <summary>
        /// Method to query the API for new percentage changes
        /// </summary>
        /// <param name="symbol">The symbol to get the change for</param>
        /// <param name="dte">The date to end the change on</param>
        /// <returns>A StockChange object containing the date, symbol and percentage change</returns>
        public static StockChange refreshStockChange(string symbol, DateTime dte)
        {
            DateTime date = dte;
            int reduce = -1;

            if(date.DayOfWeek == DayOfWeek.Monday)
            {
                reduce = -3;
            }
            else if (date.DayOfWeek == DayOfWeek.Saturday)
            {
                date = date.AddDays(-1);
            }
            else if (date.DayOfWeek == DayOfWeek.Sunday)
            {
                date = date.AddDays(-2);
            }

            DateTime oldDate = date.AddDays(reduce);

            float change = (float)WebInterface.getChange(symbol, oldDate.ToString("yyyyMMdd"), date.ToString("yyyyMMdd"));

            StockChange toReturn = new StockChange(symbol, change, dte);

            return toReturn;
        }

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

        /// <summary>
        /// Method to test the day of week of the specified date
        /// </summary>
        /// <param name="date">The date to test</param>
        /// <returns>The number of days until the next working day</returns>
        public static int testWorkingDay(DateTime date)
        {
            DayOfWeek day = date.DayOfWeek;
            int hour = date.Hour;

            if (hour >= 9 && hour < 16)
            {
                switch (day)
                {
                    case DayOfWeek.Friday:
                        return (24 + 24 + 24);
                    case DayOfWeek.Saturday:
                        return (24 + 24);
                    case DayOfWeek.Sunday:
                    default:
                        return 24;
                }
            }
            else if (hour >= 16)
            {
                switch (day)
                {
                    case DayOfWeek.Friday:
                        return (24 + 24 + 17);
                    case DayOfWeek.Saturday:
                        return (24 + 17);
                    case DayOfWeek.Sunday:
                    default:
                        return 17;
                }
            }
            else
            {
                //before 9am
                return (9 - hour);
            }
        }

        /// <summary>
        /// Generates random but realistic prices for a day's trading based on the open & close, high & low prices
        /// </summary>
        /// <param name="open">The day's starting price</param>
        /// <param name="low">The day's lowest price, here used as a minimum for Random</param>
        /// <param name="high">The day's highest price, here used as a maximum</param>
        /// <param name="close">The day's ending price</param>
        /// <returns>A decimal array for the generated hourly prices</returns>
        public static decimal[] generatePrices(decimal open, decimal low, decimal high, decimal close)
        {
            Random r = new Random();
            int bottom = Convert.ToInt32(low * 100); //decimal to int for random.next()
            int top = Convert.ToInt32(high * 100);

            decimal[] prices = new decimal[9]; //9am-5pm
            prices[0] = open; //setting the two set prices of the day
            prices[8] = close;
            for (int i = 1; i < 8; i++)
            {
                decimal random = r.Next(bottom, top); //generate random price (int, x100) between the day's high and low
                decimal price = (random / 100); //get the random price as a decimal and in the right order of magnitude
                prices[i] = price; //set that hour's price
            }

            return prices;
        }

        /// <summary>
        /// Get the maximum and minimum values of a 2D array
        /// </summary>
        /// <param name="data">The 2D array to work with</param>
        /// <returns>The maximum and minimum values (in that order) of the array</returns>
        public static decimal[] getExtremes(decimal[][] data)
        {
            var values = data.SelectMany(x => x); //flatten array

            decimal[] toReturn =
            {
                values.Max(),
                values.Min()
            };

            return toReturn;
        }

        /// <summary>
        /// Method to generate the axis labels for a stock graph
        /// </summary>
        /// <param name="data">The stock prices of the graph</param>
        /// <param name="dates">The dates of the stock prices used</param>
        /// <returns>A 2D string array containing the vertical price labels [0][i] and the horizontal date labels [1][i]</returns>
        public static string[][] axisLabeller(decimal[][] data, DateTime[] dates)
        {
            string[][] toReturn = new string[2][];
            toReturn[0] = new string[11];
            toReturn[1] = new string[11];

            //get max and min across arrays
            decimal[] extremes = getExtremes(data); //min = 1, max = 0

            //left axis labels
            decimal leftSpacing = ((extremes[0] - extremes[1]) / 10); //10 labels on the vertical axis, so each one 1/10 of the way up
            for(int i = 0; i < 10; i++)
            {
                decimal value = extremes[1] + (i * leftSpacing); //minimum + i tenths of the difference
                toReturn[0][i] = (value).ToString("N1");
            }
            toReturn[0][10] = (extremes[0]).ToString("N1");

            //bottom axis labels
            float rightSpacing = ((float)dates.Count() / 10f);
            for(int i = 0; i < 10; i++)
            {
                int index = (int)(0 + (i * rightSpacing));
                toReturn[1][i] = dates[index].ToString("dd/MM/yyyy");
            }
            toReturn[1][10] = dates.Last().ToString("dd/MM/yyyy");

            return toReturn;
        }
    }

    /// <summary>
    /// Static class to interface with the web API
    /// </summary>
    public static class WebInterface
    {
        public static string getPage(string url)
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
        /// Method to get the percentage change of a stock between the two specified dates
        /// </summary>
        /// <param name="symbol">The stock to get the change for</param>
        /// <param name="startDate">The start date of the change</param>
        /// <param name="endDate">The end date of the change</param>
        /// <returns></returns>
        public static double getChange(string symbol, string startDate, string endDate)
        {
            //format .../symbol/YYYYMMDD/YYYYMMDD/c
            string url = "http://philippratt.co.uk:5000/" + symbol + "/" + startDate + "/" + endDate + "/c";
            return Double.Parse(getData(url));

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

        /// <summary>
        /// Method to get the full name of the company with the specified symbol
        /// </summary>
        /// <param name="symbol">The symbol of the company</param>
        /// <returns>The full name of that company</returns>
        public static string getName(string symbol)
        {
            string data = "";

            string url = "https://www.quandl.com/api/v3/datasets/WIKI/" + symbol + "/metadata.xml?api_key=VAjcx6n-wo8WLqb6VD-p";

            try
            {
                data = new WebClient().DownloadString(url);

                string fullName = Utilities.getBetween(data, "<name>", "</name>");
                string name = Utilities.getName(fullName, symbol);

                return name;
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
        /// <returns>A GameLogic object set up by the save file</returns>
        public static GameLogic readSaveFile(string path)
        {
            string symbol = "";
            using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (BufferedStream bs = new BufferedStream(fs))
            using (StreamReader sr = new StreamReader(bs))
            {
                string line;

                if ((line = sr.ReadLine()) == null) //read first line and test if not-null
                {
                    return null; //if first line is null
                }
                else
                {
                    DateTime date = DateTime.Parse(line);
                    sr.ReadLine();

                    if (line[0] == '<') //test if header row is present
                    {
                        if ((line = sr.ReadLine()) == null)
                        {
                            return null; //if header row is present and next row is null, return as no data to process
                        }
                    }

                    Exchange ex = new Exchange();

                    //get stock symbol and ensure ticker is ready
                    symbol = line.Split(',')[0];
                    ex.Add(symbol);

                    do
                    {
                        ex[symbol].AddDay(line); //add lines while there are lines to add
                    } while ((line = sr.ReadLine()) != null);

                    return new GameLogic(ex, date);
                }
            }
        }

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
        public static void writeExchangeToFile(string path, GameLogic gl)
        {
            string date = gl.CurrentDate.ToString();
            string header = "<ticker>,<date>,<open>,<high>,<low>,<close>,<vol>";

            File.WriteAllText(path, string.Empty); //ensures file is empty before writing

            using (FileStream fs = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
            using (StreamWriter sw = new StreamWriter(new BufferedStream(fs)))
            {
                sw.WriteLine(date);
                sw.WriteLine(header);

                foreach (KeyValuePair<string, Ticker> t in gl.ex)
                { //iterate through symbols in exchnage
                    string symbol = t.Key;

                    foreach (KeyValuePair<DateTime, StockRow> sr in t.Value)
                    { //iterate through each day's data for each symbol
                        string dte = sr.Key.ToString("yyyyMMdd");

                        string high = sr.Value.high.ToString(); //convert to string from decimal
                        string low = sr.Value.low.ToString();
                        string open = sr.Value.open.ToString();
                        string close = sr.Value.close.ToString();
                        string volume = sr.Value.volume.ToString();

                        string toWrite = symbol + "," + dte + "," + open + "," + high + "," + low + "," + close + "," + volume;

                        sw.WriteLine(toWrite);
                    }
                }

                sw.Close(); //closes the file and ensures the buffer is flushed
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
        public int volume { get; set; }

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

    /// <summary>
    /// A struct to store purchased stocks
    /// </summary>
    public struct Stock
    {
        public DateTime purchaseDate { get; }
        public decimal purchasePrice { get; }
        public int amount { get; set; }
        public string symbol { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sym">The stock symbol</param>
        /// <param name="date">The date of the Stock values</param>
        /// <param name="price">The stock price</param>
        /// <param name="volume">The number of shares available</param>
        public Stock(string sym, DateTime date, decimal price, int volume)
        {
            symbol = sym;
            purchaseDate = date;
            purchasePrice = price;
            amount = volume;
        }
    }

    public struct StockChange
    {
        public string symbol { get; }
        public float change { get; set; }
        public DateTime date { get; set; }

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="sym">The stock symbol</param>
        /// <param name="chg">The percentage change</param>
        /// <param name="dte">The date when the change ends</param>
        public StockChange(string sym, float chg, DateTime dte)
        {
            symbol = sym;
            change = chg;
            date = dte;
        }
    }
}