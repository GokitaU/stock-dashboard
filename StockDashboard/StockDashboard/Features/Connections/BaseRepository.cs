using Dapper;
using StockDashboard.Tables;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using YahooFinanceApi;

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
        public async Task InsertDailyProcess(int symbolId, DateTime lastestDate, string successFlag)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@SymbolId", symbolId);
            parameters.Add("@LastestDate", lastestDate);
            parameters.Add("@SuccessFlag", successFlag);
            var sqlCommand = $"INSERT INTO DailyProcess (SymbolId, LastestDate, SuccessFlag) VALUES (@SymbolId, @LastestDate, @SuccessFlag)";
            using (IDbConnection cn = Connection)
            {
                cn.Open();
                await cn.ExecuteAsync(sqlCommand, parameters);
                cn.Close();
            }
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

        public async Task BulkCandleInsert(List<Candle> data, int symbolId)
        {
            var dataTable = ListToDataTable(data, symbolId);
            using (SqlConnection sqlConn = SqlConnect)
            {
                sqlConn.Open();
                using (SqlBulkCopy sqlbc = new SqlBulkCopy(sqlConn))
                {
                    sqlbc.DestinationTableName = "DailyHistoricalPriceData";
                    sqlbc.ColumnMappings.Add("SymbolId", "SymbolId");
                    sqlbc.ColumnMappings.Add("MarketDate", "MarketDate");
                    sqlbc.ColumnMappings.Add("Open", "Open");
                    sqlbc.ColumnMappings.Add("High", "High");
                    sqlbc.ColumnMappings.Add("Low", "Low");
                    sqlbc.ColumnMappings.Add("Close", "Close");
                    sqlbc.ColumnMappings.Add("Volume", "Volume");
                    sqlbc.ColumnMappings.Add("AdjustedClose", "AdjustedClose");
                    await sqlbc.WriteToServerAsync(dataTable);
                }
                sqlConn.Close();
            }
        }

        public DataTable ListToDataTable(List<Candle> data, int symbolId)
        {
            var table = new DataTable();
            table.Columns.Add("SymbolId", typeof(int));
            table.Columns.Add("MarketDate", typeof(DateTime));
            table.Columns.Add("Open", typeof(decimal));
            table.Columns.Add("High", typeof(decimal));
            table.Columns.Add("Low", typeof(decimal));
            table.Columns.Add("Close", typeof(decimal));
            table.Columns.Add("Volume", typeof(long));
            table.Columns.Add("AdjustedClose", typeof(decimal));
            foreach (var candle in data)
            {
                var row = new Object[]
                {
                    symbolId,
                    candle.DateTime,
                    candle.Open,
                    candle.High,
                    candle.Low,
                    candle.Close,
                    candle.Volume,
                    candle.AdjustedClose
                };
                table.Rows.Add(row);
            }
            return table;
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
