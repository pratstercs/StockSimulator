using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator
{
    class Program
    {
        static void Main(string[] args)
        {
            Program program = new Program();
            program.menu();
        }

        void menu()
        {
            testExchange();
            Console.In.Read();
        }

        void testTicker()
        {
            Ticker JPM = new Ticker();
            JPM.AddDay("JPM,20150102,62.62,62.96,62.07,62.49,12599900");

            StockRow row = JPM[Ticker.toDate("20150102")];

            string date = "Date: " + row.date;
            string high = "High: $" + row.high;

            Console.Out.WriteLine(date);
            Console.Out.WriteLine(high);
        } //raw hardcoded single string

        void testExchange()
        {
            Exchange NYSE = new Exchange();
            //NYSE.Add("JPM");

            string symbol = "";

            String path = @"C:\Users\Phil\Downloads\JPM_20150101_20160127.txt";

            DateTime startTime = DateTime.Now;
            DateTime intTime = new DateTime();

            using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (BufferedStream bs = new BufferedStream(fs))
            using (StreamReader sr = new StreamReader(bs))
            {
                string line;
                
                if((line = sr.ReadLine()) != null) //read first line and test if not-null
                {
                    if(line[0] == '<') //test if header row is present
                    {
                        if ((line = sr.ReadLine()) == null)
                        {
                            return; //if header row is present and next row is null, return as no data to process
                        }
                    }

                    //get stock symbol and ensure ticker is ready
                    symbol = line.Split(',')[0];
                    NYSE.Add(symbol);

                    intTime = DateTime.Now;

                    do
                    {
                        NYSE[symbol].AddDay(line);
                    } while ((line = sr.ReadLine()) != null);
                }
            }

            DateTime stopTime = DateTime.Now;

            StockRow start = NYSE[symbol][Ticker.toDate("20150102")];
            StockRow end = NYSE[symbol][Ticker.toDate("20150520")];

            decimal high = start.high > end.high ? start.high : end.high;
            decimal low = start.low < end.low ? start.low : end.low;

            Console.Out.WriteLine("Symbol: " + symbol);
            Console.Out.WriteLine("High: $" + high);
            Console.Out.WriteLine("Low: $" + low);


            TimeSpan time = stopTime - startTime;
            TimeSpan time1 = intTime - startTime;
            TimeSpan time2 = stopTime - intTime;

            Console.Out.WriteLine("");
            Console.Out.WriteLine("Time taken: " + time.TotalMilliseconds + "ms");
            Console.Out.WriteLine("Time to add stock: " + time1.TotalMilliseconds + "ms");
            Console.Out.WriteLine("Time to add data: " + time2.TotalMilliseconds + "ms");
            Console.Out.WriteLine("Time per line: " + (time2.TotalMilliseconds/280) + "ms");
        }
    }
}

public class Exchange : SortedDictionary<string, Ticker>
{
    public void Add(string symbol)
    {
        if (!this.ContainsKey(symbol)) {
            this.Add(symbol, new Ticker());
        }
    }

    public void AddData(string symbol, string data)
    {

    }
}

public class Ticker : SortedDictionary<DateTime, StockRow>
{
    public void AddDay(string data)
    {
        string[] split = data.Split(',');
        StockRow row = rowMaker(split);

        this.Add(row.date, row);
    }

    private static StockRow rowMaker(string[] data)
    {
        StockRow row = new StockRow(
                toDate(data[1]),
                Convert.ToDecimal(data[2]),
                Convert.ToDecimal(data[3]),
                Convert.ToDecimal(data[4]),
                Convert.ToDecimal(data[5]),
                Convert.ToInt32(data[6])
            );

        return row;
    }

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
}

public struct StockRow
{
    public DateTime date;
    public decimal open;
    public decimal close;
    public decimal high;
    public decimal low;
    public int volume;

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


