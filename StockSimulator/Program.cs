using System;
using System.Linq;

namespace StockSimulator
{
    class ConsoleMenu
    {
        //Exchange NYSE;
        GameLogic gl;

        static void Main(string[] args)
        {
            //Debug test code
            /*
            TestClass tc = new TestClass(new GameLogic());

            tc.testWriting();

            Console.WriteLine("Test Finished");
            Console.In.Read();
            */

            //Actual program operation
            
            ConsoleMenu menu = new ConsoleMenu();
            menu.startMenu();
            
        }

        void startMenu()
        {
            gl = new GameLogic();
            int input;

            do
            {
                Console.WriteLine("01. Sandbox");
                Console.WriteLine("02. Scenario 1");
                Console.WriteLine("00. Exit");

                input = Int32.Parse(Console.In.ReadLine());

                switch (input)
                {
                    case 01:
                        //startSandbox();
                        break;
                    case 02:
                        startScenario1();
                        break;
                    case 00:
                        Environment.Exit(0);
                        break;
                    default:
                        break;
                }
            } while (input != 00);
        }

        void startScenario1()
        {
            Exchange scenario1 = new Exchange();

            string[] companies =
            {
                "FB",   //Facebook
                "GOOG", //Google
                "NFLX", //Netflix
                "AAPL", //Apple
                "MSFT", //Microsoft
                "EBAY", //eBay
                "AMZN", //Amazon
                "TWTR", //Twitter
                "PYPL", //Paypal
                "LNKD"  //LinkedIn
            };

            foreach(string symbol in companies)
            {
                scenario1.Add(symbol);
                Utilities.arrayify(symbol, WebInterface.queryAPI(symbol), scenario1);
            }
        }
    }

    class TestClass
    {
        GameLogic gl;

        public TestClass(GameLogic gamelogic)
        {
            gl = gamelogic;
        }

        public void testWriting()
        {
            string path = @"C:\Users\Phil\Desktop\out.txt";
            Exchange ex = new Exchange();
            string response = WebInterface.queryAPI("JPM");
            Utilities.arrayify("JPM", response, ex);
            FileInterface.writeExchangeToFile(path, ex);
        }

        public void testTicker()
        {
            Ticker JPM = new Ticker();
            JPM.AddDay("JPM,20150102,62.62,62.96,62.07,62.49,12599900");

            StockRow row = JPM[Utilities.toDate("20150102")];

            string date = "Date: " + row.date;
            string high = "High: $" + row.high;

            Console.Out.WriteLine(date);
            Console.Out.WriteLine(high);
        } //raw hardcoded single string

        public void testExchange()
        {
            gl.NYSE = new Exchange();
            //NYSE.Add("JPM");

            string symbol = "JPM";

            String path = @"C:\downloads\JPM_20150101_20160128.txt";

            DateTime startTime = DateTime.Now;
            try
            {
                FileInterface.readFile(path, gl.NYSE);
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

        public void testGrouping()
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

    class GameMenu {

    }
}