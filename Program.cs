using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Analysis;
using ServiceStack;
using ServiceStack.Text;


namespace StockPricesProject
{ 
    class Program
    {
        static void Main(string[] args)
        {
            /* use GLW for company Corning, Inc. , on other side can be use GrubHub, Inc. (GRUB US)! 
             I used Api to extract data
            */
            AVConnection conn = new AVConnection("Portfolio BI");
            List<SecurityData> prices = conn.GetDailyPrices("GLW");
            prices.Reverse();
            PrimitiveDataFrameColumn<DateTime> date = new PrimitiveDataFrameColumn<DateTime>("Date", prices.Select(sd => sd.Timestamp));
            PrimitiveDataFrameColumn<decimal> priceCol = new PrimitiveDataFrameColumn<decimal>("Close Price", prices.Select(sd => sd.Close));
            DataFrame df = new DataFrame(date, priceCol);

            PrimitiveDataFrameColumn<decimal> pctChange = new PrimitiveDataFrameColumn<decimal>("Percent Change", prices.Count);

            for (int i = 1; i < prices.Count; i++)
            {
                decimal prevPrice = (decimal)df.Columns["Close Price"][i - 1];
                decimal currPrice = (decimal)df.Columns["Close Price"][i];
                decimal delta = ((currPrice / prevPrice) - 1) * 100;
                pctChange[i] = Math.Round(delta, 3);
            }
            df.Columns.Add(pctChange);
            Console.WriteLine(df);
        }
    }

    public class SecurityData
    {
        
        public DateTime Timestamp { get; set; }
        public decimal Close { get; set; }
    }

    public class AVConnection
    {
        private readonly string _apiKey;

        public AVConnection(string apiKey)
        {
            this._apiKey = apiKey;
        }

        public List<SecurityData> GetDailyPrices(string symbol)
        {
            const string FUNCTION = "TIME_SERIES_DAILY";
            string connectionString = "https://" + $@"www.alphavantage.co/query?function={FUNCTION}&symbol={symbol}&apikey={this._apiKey}&datatype=csv";
            List<SecurityData> prices = connectionString.GetStringFromUrl().FromCsv<List<SecurityData>>();
            return prices;
        }
    }
}
