using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace StockDashboard.Features.Connections
{
    public abstract class DbContext
    {
        internal IDbConnection Connection
        {
            get
            {
                return new SqlConnection(Startup.Configuration.GetConnectionString("SqlStockProject"));
            }
        }
        internal SqlConnection SqlConnect
        {
            get
            {
                return new SqlConnection(Startup.Configuration.GetConnectionString("SqlStockProject"));
            }
        }
    }
}
