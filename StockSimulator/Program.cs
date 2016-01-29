using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator
{
    class ConsoleMenu
    {
        //        Exchange NYSE;
        GameLogic gl;

        static void Main(string[] args)
        {
            ConsoleMenu menu = new ConsoleMenu();
            menu.menu();
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

            StockRow row = JPM[Utilities.toDate("20150102")];

            string date = "Date: " + row.date;
            string high = "High: $" + row.high;

            Console.Out.WriteLine(date);
            Console.Out.WriteLine(high);
        } //raw hardcoded single string

        void testExchange()
        {
            Exchange NYSE = new Exchange();
            //NYSE.Add("JPM");

            string symbol = "JPM";

            String path = @"C:\Users\Phil\Downloads\JPM_20150101_20160127.txt";

            DateTime startTime = DateTime.Now;
            gl.readFile(path, NYSE);
            //readFile(path, NYSE);
            DateTime stopTime = DateTime.Now;

            StockRow start = NYSE[symbol][Utilities.toDate("20150102")];
            StockRow end = NYSE[symbol][Utilities.toDate("20150520")];

            decimal high = start.high > end.high ? start.high : end.high;
            decimal low = start.low < end.low ? start.low : end.low;

            decimal change = ((start.open - end.close) / end.close);

            Console.Out.WriteLine("Symbol: " + symbol);
            Console.Out.WriteLine("High: $" + high);
            Console.Out.WriteLine("Low: $" + low);
            Console.Out.WriteLine("Change: " + Math.Round(change, 2) + "%");

            Console.Out.WriteLine("");
            TimeSpan time = stopTime - startTime;
            Console.Out.WriteLine("Time taken: " + time.TotalMilliseconds + "ms");
        }
    }
}

        
