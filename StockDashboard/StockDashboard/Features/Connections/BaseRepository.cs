using Dapper;
using Microsoft.AspNetCore.Mvc.Formatters;
using NLog;
using NLog.Web;
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
        public Logger Logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
        public async Task<SystemDefaults> GetSystemDefault(string attributeName)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@AttributeName", attributeName);
            SystemDefaults index;
            var sqlQuery = @"SELECT * FROM SystemDefaults WHERE AttributeName = @AttributeName;";
            try
            {
                using (IDbConnection cn = Connection)
                {
                    cn.Open();
                    var result = cn.QueryAsync<SystemDefaults>(sqlQuery, parameters).GetAwaiter().GetResult();
                    cn.Close();
                    index = result.FirstOrDefault();
                }
            }
            catch (Exception exc)
            {
                index = null;
                Logger.Info(exc.ToString());
            }

            return index;
        }

        public async Task<List<TradeNotifications>> GetTradeNotifications()
        {
            List<TradeNotifications> notifications;
            var sqlQuery = "SELECT * FROM TradeNotifications";
            using (IDbConnection cn = Connection)
            {
                cn.Open();
                var result = await cn.QueryAsync<TradeNotifications>(sqlQuery);
                cn.Close();
                notifications = result.ToList();
            }
            return notifications;
        }

        public async Task<List<AppUsers>> GetAppUsers()
        {
            List<AppUsers> appUsers;
            var sqlQuery = "SELECT * FROM AppUsers";
            using (IDbConnection cn = Connection)
            {
                cn.Open();
                var result = await cn.QueryAsync<AppUsers>(sqlQuery);
                cn.Close();
                appUsers = result.ToList();
            }
            return appUsers;
        }

        public async Task<List<ChangeDataReport>> LoadTopDailyLosers(DateTime marketDate)
        {
            List<ChangeDataReport> stockSymbols;
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@MarketDate", marketDate);
                var sqlQuery = @"SELECT TOP(250) RSI.Id, RSI.Symbol, PCD.PercentChange, RSI.CompanyName, PCD.AbsoluteChange, PCD.MarketDate 
                         FROM RootSymbolIndex RSI, PercentChangeData PCD 
                         WHERE PCD.MarketDate = @MarketDate AND PCD.SymbolId = RSI.ID 
                         Order By PercentChange Asc;";
                using (IDbConnection cn = Connection)
                {
                    cn.Open();
                    var result = await cn.QueryAsync<ChangeDataReport>(sqlQuery, parameters);
                    cn.Close();
                    stockSymbols = result.ToList();
                }
                return stockSymbols;
            }
            catch (Exception exc)
            {
                stockSymbols = new List<ChangeDataReport>();
                return stockSymbols;
            } 
        }

        public async Task<List<ChangeDataReport>> LoadTopDailyGainers(DateTime marketDate)
        {
            List<ChangeDataReport> stockSymbols;
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@MarketDate", marketDate);              
                var sqlQuery = @"SELECT TOP(250) RSI.Id, RSI.Symbol, PCD.PercentChange, RSI.CompanyName, PCD.AbsoluteChange, PCD.MarketDate 
                         FROM RootSymbolIndex RSI, PercentChangeData PCD 
                         WHERE PCD.MarketDate = @MarketDate AND PCD.SymbolId = RSI.ID 
                         Order By PercentChange Desc;";
                using (IDbConnection cn = Connection)
                {
                    cn.Open();
                    var result = await cn.QueryAsync<ChangeDataReport>(sqlQuery, parameters);
                    cn.Close();
                    stockSymbols = result.ToList();
                }
                return stockSymbols;
            }
            catch (Exception exc)
            {
                stockSymbols = new List<ChangeDataReport>();
                return stockSymbols;
            }          
        }
        public async Task<List<DailyHistoricalPriceData>> LoadPercentChangeList(int SymbolId)
        {
            List<DailyHistoricalPriceData> stockSymbols;
            var parameters = new DynamicParameters();
            parameters.Add("@SymbolId", SymbolId);
            var sqlQuery = "SELECT * FROM DailyHistoricalPriceData WHERE SymbolId = @SymbolId AND MarketDate >= (SELECT MAX(MarketDate) FROM PercentChangeData WHERE SymbolId = @SymbolId);";
            using (IDbConnection cn = Connection)
            {
                cn.Open();
                var result = await cn.QueryAsync<DailyHistoricalPriceData>(sqlQuery, parameters);
                cn.Close();
                stockSymbols = result.ToList();
            }
            return stockSymbols;
        }

        public async Task<List<DailyHistoricalPriceData>> LoadCandles(int SymbolId)
        {
            List<DailyHistoricalPriceData> stockSymbols;
            var parameters = new DynamicParameters();
            parameters.Add("@SymbolId", SymbolId);
            var sqlQuery = "SELECT * FROM DailyHistoricalPriceData WHERE SymbolId = @SymbolId;";
            using (IDbConnection cn = Connection)
            {
                cn.Open();
                var result = await cn.QueryAsync<DailyHistoricalPriceData>(sqlQuery, parameters);
                cn.Close();
                stockSymbols = result.ToList();
            }
            return stockSymbols;
        }
        public async Task<List<RootSymbolIndex>> QuerySymbols()
        {
            List<RootSymbolIndex> stockSymbols;
            var sqlQuery = "SELECT RSI.Id, RSI.Symbol, RSI.CompanyName FROM RootSymbolIndex RSI, InitialProcess IP WHERE RSI.Id = IP.SymbolId AND IP.SuccessFlag = 'Y';";
            using (IDbConnection cn = Connection)
            {
                cn.Open();
                var result = await cn.QueryAsync<RootSymbolIndex>(sqlQuery);
                cn.Close();
                stockSymbols = result.ToList();
            }
            return stockSymbols;
        }

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
        public async Task<List<RootSymbolIndex>> RetryInitialProcess()
        {
            List<RootSymbolIndex> stockSymbols;
            var sqlQuery = "SELECT * FROM RootSymbolIndex WHERE Id IN (SELECT SymbolId FROM InitialProcess WHERE SuccessFLag = 'N')";
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
        public async Task<RootSymbolIndex> StockInfoById(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);
            RootSymbolIndex index;
            var sqlQuery = @"SELECT * FROM RootSymbolIndex 
                              WHERE Id = @Id";
            using (IDbConnection cn = Connection)
            {
                cn.Open();
                var result = await cn.QueryFirstAsync<RootSymbolIndex>(sqlQuery, parameters);
                cn.Close();
                index = result;
            }
            return index;
        }

        public async Task<List<NightlyBarsModel>> QueryNightlyBars()
        {
            List<NightlyBarsModel> nighlyBars;
            var sqlQuery = @"SELECT DISTINCT (DHPD.SymbolId), MAX(DHPD.MarketDate) AS MaxDate, RSI.Symbol  
                                FROM [DailyHistoricalPriceData] DHPD, RootSymbolIndex RSI 
                                WHERE(DHPD.SymbolId = RSI.Id ) 
                                GROUP BY RSI.Symbol, DHPD.SymbolId;";
            using (IDbConnection cn = Connection)
            {
                cn.Open();
                var result = await cn.QueryAsync<NightlyBarsModel>(sqlQuery);
                cn.Close();
                nighlyBars = result.ToList();
            }
            return nighlyBars;
        }
        public async Task<List<DailyProcess>> StocksToUpdate(DateTime availableDate)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@AvailableDate", availableDate);
            List<DailyProcess> stockSymbols;
            var sqlQuery = @$"SELECT * FROM DailyProcess 
                              WHERE CONVERT(DATE, LastestDate) < CONVERT(DATE, @AvailableDate) 
                           ORDER BY SymbolId";
            using (IDbConnection cn = Connection)
            {
                cn.Open();
                var result = await cn.QueryAsync<DailyProcess>(sqlQuery, parameters);
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
                try
                {
                    await cn.ExecuteAsync(sqlCommand, parameters);
                }
                catch (Exception exc)
                {

                }

                cn.Close();
            }
        }

        public DataTable ListPercentChangeToDataTable(List<PercentChangeData> data)
        {
            var table = new DataTable();
            table.Columns.Add("SymbolId", typeof(int));
            table.Columns.Add("MarketDate", typeof(DateTime));
            table.Columns.Add("PastDate", typeof(DateTime));
            table.Columns.Add("PercentChange", typeof(decimal));
            table.Columns.Add("AbsoluteChange", typeof(decimal));
            foreach (var item in data)
            {
                var row = new Object[]
                {
                    item.SymbolId,
                    item.MarketDate,
                    item.PastDate,
                    item.PercentChange,
                    item.AbsoluteChange
                };
                table.Rows.Add(row);
            }
            return table;
        }
        public async Task BulkPercentChangeInsert(List<PercentChangeData> data)
        {
            try
            {
                var dataTable = ListPercentChangeToDataTable(data);
                using (SqlConnection sqlConn = SqlConnect)
                {
                    sqlConn.Open();
                    using (SqlBulkCopy sqlbc = new SqlBulkCopy(sqlConn))
                    {
                        sqlbc.DestinationTableName = "PercentChangeData";
                        sqlbc.ColumnMappings.Add("SymbolId", "SymbolId");
                        sqlbc.ColumnMappings.Add("MarketDate", "MarketDate");
                        sqlbc.ColumnMappings.Add("PastDate", "PastDate");
                        sqlbc.ColumnMappings.Add("PercentChange", "PercentChange");
                        sqlbc.ColumnMappings.Add("AbsoluteChange", "AbsoluteChange");
                        await sqlbc.WriteToServerAsync(dataTable);
                    }
                    sqlConn.Close();
                }
            }
            catch (Exception exc)
            {
                Logger.Info(exc.ToString());
            }
        }
        public async Task BulkBarInsert(List<StockDashboard.Models.Candle> data, int symbolId)
        {
            try
            {
                var dataTable = ListBarsToDataTable(data, symbolId);
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
            catch (Exception exc)
            {
                Logger.Info(exc.ToString());
            }
        }
        public async Task BulkCandleInsert(List<Candle> data, int symbolId)
        {
            try
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
            catch (Exception exc)
            {

            }
        }
        public DataTable ListBarsToDataTable(List<StockDashboard.Models.Candle> data, int symbolId)
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

        public async Task UpdateInitialProcessFlag(int symbolId, string successFlag)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@SymbolId", symbolId);
            parameters.Add("@SuccessFlag", successFlag);
            var sqlCommand = $"UPDATE InitialProcess SET SuccessFlag = @SuccessFlag WHERE SymbolId = @SymbolId";
            using (IDbConnection cn = Connection)
            {
                cn.Open();
                await cn.ExecuteAsync(sqlCommand, parameters);
                cn.Close();
            }
        }
        public async Task UpdateDailyProcessDate(int symbolId, DateTime lastestDate, string successFlag)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@SymbolId", symbolId);
            parameters.Add("@LastestDate", lastestDate);
            parameters.Add("@SuccessFlag", successFlag);
            var sqlCommand = $"UPDATE DailyProcess SET LastestDate = @LastestDate, SuccessFlag = @SuccessFlag WHERE SymbolId = @SymbolId";
            using (IDbConnection cn = Connection)
            {
                cn.Open();
                await cn.ExecuteAsync(sqlCommand, parameters);
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

    public class NightlyBarsModel 
    { 
        public int SymbolId { get; set; }
        public DateTime MaxDate { get; set; }
        public string Symbol { get; set; }
    }

    public class ChangeDataReport
    {
        public int Id { get; set; }
        public string Symbol { get; set; }
        public decimal PercentChange { get; set; }
        public string CompanyName { get; set; }
        public decimal AbsoluteChange { get; set; }
        public DateTime MarketDate { get; set; }
    }
}
