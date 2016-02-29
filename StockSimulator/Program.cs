using System;
using System.Linq;
using System.Threading;

namespace StockSimulator
{
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (var game = new Game1())
                game.Run();
        }
    }
#endif

#if DEBUG
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
        gl.wallet.Add(new Stock("JPM", now, 200.0M, 10));

        gl.sellStock(new DateTime(), "AAPL", 6);

        Console.WriteLine(gl.cash);
    }

    public void testDate()
    {

    }

    public void testRandom()
    {
        //string json = WebInterface.queryAPI("JPM", "2015-02-18", "2015-02-18");
        //Utilities.arrayify("JPM", json, gl.ex);
        decimal[] array = Utilities.generatePrices(58.84M, 57.25M, 58.86M, 57.81M);
        foreach (decimal d in array)
        {
            Console.WriteLine(d);
        }
    }

    public void testIncrementHour()
    {
        GameLogic gl = new GameLogic(new Exchange(), DateTime.Now);
        while (true)
        {
            Console.WriteLine(gl.currentDate.ToString());
            gl.incrementTime();
            Thread.Sleep(1500);
        }
    }
}
#endif
}