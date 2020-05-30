using Dapper;
using StockDashboard.Tables;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace StockDashboard.Features.Connections
{
    public class BaseRepository : DbContext
    {


        public async Task<List<RootSymbolIndex>> FindProcessedStocks()
        {
            List<RootSymbolIndex> stockSymbols;
            var sqlQuery = "SELECT RSI.* FROM InitialProcess IP, RootSymbolIndex RSI WHERE IP.SymbolId = RSI.Id AND IP.SuccessFlag = 'Y' ORDER BY RSI.Id;";
            using (IDbConnection cn = Connection)
            {
                cn.Open();
                var result = await cn.QueryAsync<RootSymbolIndex>(sqlQuery);
                cn.Close();
                stockSymbols = result.ToList();
            }
            return stockSymbols;
        }

        public async Task<List<RootSymbolIndex>> FindUnprocessedStocks()
        {
            List<RootSymbolIndex> stockSymbols;
            var sqlQuery = "SELECT * FROM RootSymbolIndex WHERE Id NOT IN (SELECT SymbolId FROM InitialProcess)";
            using (IDbConnection cn = Connection)
            {
                cn.Open();
                var result = await cn.QueryAsync<RootSymbolIndex>(sqlQuery);
                cn.Close();
                stockSymbols = result.ToList();
            }
            return stockSymbols;
        }

        public async Task<List<RootSymbolIndex>> StocksToUpdate(DateTime availableDate)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@AvailableDate", availableDate);
            List<RootSymbolIndex> stockSymbols;
            var sqlQuery = @"SELECT RSI.* FROM RootSymbolIndex RSI, DailyProcess DP 
                              WHERE DP.SymbolId = RSI.Id 
                                AND CONVERT(DATE,MAX(DP.LastestDate)) < CONVERT(DATE, @AvailableDate) 
                           ORDER BY RSI.Id";
            using (IDbConnection cn = Connection)
            {
                cn.Open();
                var result = await cn.QueryAsync<RootSymbolIndex>(sqlQuery);
                cn.Close();
                stockSymbols = result.ToList();
            }
            return stockSymbols;
        }
        public async Task InsertInitialProcess(int symbolId, DateTime processDate, string successFlag)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@SymbolId", symbolId);
            parameters.Add("@ProcessDate", processDate);
            parameters.Add("@SuccessFlag", successFlag);
            var sqlCommand = $"INSERT INTO InitialProcess (SymbolId, ProcessDate, SuccessFlag) VALUES (@SymbolId, @ProcessDate, @SuccessFlag)";
            using (IDbConnection cn = Connection)
            {
                cn.Open();
                await cn.ExecuteAsync(sqlCommand, parameters);
                cn.Close();
            }
        }

        //public List<YahooStockSymbols> UpdateYahooDate(DateTime Date)
        //{
        //    //SELECT * FROM YahooStockSymbols WHERE DataEndDate < '3/1/2020' AND InitialProcessFlag = 'Y'
        //    List<YahooStockSymbols> stockSymbols;
        //    var sqlQuery = $"SELECT * FROM YahooStockSymbols WHERE DataEndDate < '{Date.Month}/{Date.Day}/{Date.Year}' AND InitialProcessFlag = 'Y';";
        //    using (IDbConnection cn = Connection)
        //    {
        //        cn.Open();
        //        stockSymbols = cn.Query<YahooStockSymbols>(sqlQuery).ToList();
        //        cn.Close();
        //    }
        //    return stockSymbols;
        //}

        public void YahooDataDateUpdate(string symbol, DateTime dataEndDate)
        {
            var end = $"{dataEndDate.Month}/{dataEndDate.Day}/{dataEndDate.Year}";
            var sqlCommand = $"UPDATE YahooStockSymbols SET DataEndDate = '{end}' WHERE Symbol = '{symbol}' ";
            using (IDbConnection cn = Connection)
            {
                cn.Open();
                cn.Execute(sqlCommand);
                cn.Close();
            }
        }

        public void UpdateStockTable(string symbol, DateTime dataStartDate, DateTime dataEndDate)
        {
            var start = $"{dataStartDate.Month}/{dataStartDate.Day}/{dataStartDate.Year}";
            var end = $"{dataEndDate.Month}/{dataEndDate.Day}/{dataEndDate.Year}";
            var sqlCommand = $"UPDATE YahooStockSymbols SET DataStartDate = '{start}', DataEndDate = '{end}', InitialProcessFlag = 'Y' WHERE Symbol = '{symbol}' ";
            using (IDbConnection cn = Connection)
            {
                cn.Open();
                cn.Execute(sqlCommand);
                cn.Close();
            }
        }

        public string ReturnCompanyName(string symbol)
        {
            string companyName = "";
            var sqlQuery = $"SELECT CompanyName FROM YahooStockSymbols WHERE Symbol = '{symbol}' ;";
            using (IDbConnection cn = Connection)
            {
                cn.Open();
                var returns = cn.Query<string>(sqlQuery).ToList();
                companyName = returns[0];
                cn.Close();
            }
            return companyName;
        }
    }
}
