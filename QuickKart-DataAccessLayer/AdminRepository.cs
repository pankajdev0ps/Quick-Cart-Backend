using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QuickKart_DataAccessLayer.Models;

namespace QuickKart_DataAccessLayer
{
    public class AdminRepository
    {
        private readonly SqlConnection sqlConnection;
        private readonly SqlCommand sqlCommand;

        private readonly ILogger<AdminRepository> logger;

        public AdminRepository(ILogger<AdminRepository> _logger)
        {
            logger = _logger;
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            var config = builder.Build();
            var connectionString = config.GetConnectionString("DBConnectionString");
            
            sqlConnection = new SqlConnection(connectionString);
            sqlCommand = new SqlCommand();
        }

        public int AddProduct(Product p)
        {
            string cmdString = "INSERT INTO product VALUES (@val1, @val2, @val3, @val4, @val5)";
            
            sqlCommand.Connection = sqlConnection;
            sqlCommand.CommandText = cmdString;
            sqlCommand.Parameters.AddWithValue("@val1", p.ProductName);
            sqlCommand.Parameters.AddWithValue("@val2", p.ProductPrice);
            sqlCommand.Parameters.AddWithValue("@val3", "TMC India");
            sqlCommand.Parameters.AddWithValue("@val4", p.Discount);
            sqlCommand.Parameters.AddWithValue("@val5", p.ProductImage);

            try
            {
                sqlConnection.Open();
                sqlCommand.ExecuteNonQuery();
            }
            catch(SqlException ex)
            {
                logger.LogInformation($"SQl Error:{ex.Message}");
                return 0;
            }
            finally { sqlConnection.Close(); }

            // other codes.
            return 1;
        }
    }
}
