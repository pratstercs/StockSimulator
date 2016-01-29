using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

namespace StockSimulator
{
    class ConsoleMenu
    {
        //Exchange NYSE;
        GameLogic gl;

        static void Main(string[] args)
        {
            ConsoleMenu menu = new ConsoleMenu();
            menu.menu();
        }

        void menu()
        {
            gl = new GameLogic();

            //testExchange();
            //testGrouping();

            //string response = 
            gl.parseJSON("JPM");
            //Console.Write(response);

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
            gl.NYSE = new Exchange();
            //NYSE.Add("JPM");

            string symbol = "JPM";

            String path = @"C:\downloads\JPM_20150101_20160128.txt";

            DateTime startTime = DateTime.Now;
            try
            {
                gl.readFile(path, gl.NYSE);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
            //readFile(path, NYSE);
            DateTime stopTime = DateTime.Now;

            StockRow start = gl.NYSE[symbol][Utilities.toDate("20160101")];
            StockRow end = gl.NYSE[symbol][Utilities.toDate("20160128")];

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

        void testGrouping()
        {
            testExchange();

            //next line from StackOverflow - used to extract data from the SortedDictionary ordered by propery of value, not by key
            //http://stackoverflow.com/a/1332
            var sortedDic = from entry in gl.NYSE["JPM"] orderby entry.Value.high descending select entry;

            foreach (var line in sortedDic.ToList())
            {
                Console.WriteLine(line.Value.date + " - $" + line.Value.high);
            }
        }
    }
}

        
