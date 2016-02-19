using System;
using System.Linq;

namespace StockSimulator
{
    class ConsoleMenu
    {
        //Exchange ex;
        GameLogic gl;

        static void Main(string[] args)
        {
            //Debug test code
            
            TestClass tc = new TestClass(new GameLogic());

            tc.testSell();

            Console.WriteLine("Test Finished");
            Console.In.Read();
            

            //Actual program operation
            /*
            ConsoleMenu menu = new ConsoleMenu();
            menu.startMenu();
            */
        }

        void startMenu()
        {
            //gl = new GameLogic();
            int input;

            do
            {
                Console.WriteLine("01. Sandbox");
                Console.WriteLine("02. Scenario 1");
                Console.WriteLine("00. Exit");

                Console.Write("Selection: ");
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
            Exchange scenario1ex = new Exchange();

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
                "YHOO"  //Yahoo
            };

            Console.WriteLine("Downloading data...");
            foreach(string symbol in companies)
            {
                scenario1ex.Add(symbol);
                Utilities.arrayify(symbol, WebInterface.queryAPI(symbol), scenario1ex);
            }
            Console.WriteLine("Download complete!");

            GameLogic gl = new GameLogic(scenario1ex);

            //test code
            TestClass tc = new TestClass(gl);
            tc.testBuying();
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
            gl.ex = new Exchange();
            //ex.Add("JPM");

            string symbol = "JPM";

            String path = @"C:\downloads\JPM_20150101_20160128.txt";

            DateTime startTime = DateTime.Now;
            try
            {
                FileInterface.readFile(path, gl.ex);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            //readFile(path, ex);
            DateTime stopTime = DateTime.Now;

            StockRow start = gl.ex[symbol][Utilities.toDate("20160101")];
            StockRow end = gl.ex[symbol][Utilities.toDate("20160128")];

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
            var sortedDic = from entry in gl.ex["JPM"] orderby entry.Value.high descending select entry;

            foreach (var line in sortedDic.ToList())
            {
                Console.WriteLine(line.Value.date + " - $" + line.Value.high);
            }
        }

        public void testBuying()
        {
            gl.cash = 1000000.0M;
            gl.buyStock(new DateTime(2016, 1, 4), "GOOG", 100);
            gl.buyStock(new DateTime(2016, 1, 4), "AAPL", 73);
            decimal[] results = gl.calculateProfitLoss(new DateTime(2016, 2, 4));
            Console.WriteLine("Profit: " + results[0]); //numberformat.currencynegativepattern?
            Console.WriteLine("Percentage change: " + results[1] + "%");
        }

        public void testSell()
        {
            gl.cash = 0.00M;

            DateTime now = new DateTime();

            gl.ex.Add("AAPL");
            gl.ex["AAPL"].Add(now, new StockRow(now, 3.0M, 3.0M, 3.0M, 3.0M, 1000));

            gl.wallet.Add(new Stock("AAPL", now, 10.0M, 5));
            gl.wallet.Add(new Stock("AAPL", now, 4.0M, 2));
            gl.wallet.Add(new Stock("JPM",  now, 200.0M, 10));

            gl.sellStock(new DateTime(), "AAPL", 6);

            Console.WriteLine(gl.cash);
        }
    }

    class GameMenu {

    }
}