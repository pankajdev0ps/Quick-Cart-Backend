using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QuickKart_DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;

namespace QuickKart_DataAccessLayer
{
    public class CustomerRepository
    {
        public SqlConnection conObj;
        public SqlCommand cmdObj;
        private readonly ILogger<CustomerRepository> logger;
        private readonly string storageUri;

        public CustomerRepository(ILogger<CustomerRepository> _logger)
        {
            logger = _logger;
            conObj = new SqlConnection(GetConnectionString("DBConnectionString"));
            storageUri = GetConnectionString("StorageUri");
            logger.LogInformation($"KeyVault: {GetConnectionStringFromKeyVault()}");
        }



        public string GetConnectionString(string name)
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            var config = builder.Build();
            var connectionString = config.GetConnectionString(name);

            // logger.LogInformation(connectionString);

            //   System.IO.File.AppendAllText(@"E:\stepTrack.txt", Directory.GetCurrentDirectory());
            return connectionString;

        }

        public string GetConnectionStringFromKeyVault()
        {
            string tenantID = "23bd226c-01e2-4de3-969b-c939d40fe877";
            string clientID = "bf59e962-717c-45a9-843f-cd29f7d334d8";
            string clientSecret = "CJw8Q~SEncgqQsBXM2Uz1SvtgiV2anGtDY~FybWl";
            string KeyVaultUrl = "https://keyvault34x.vault.azure.net/";
            ClientSecretCredential clientCredentials = new ClientSecretCredential(tenantID, clientID, clientSecret);
            SecretClient secretClient = new SecretClient(new Uri(KeyVaultUrl), clientCredentials);

            var secret = secretClient.GetSecret("SqlConnectionString");

            return secret.Value.Value;

        }

        //ADO.net
        public List<Product> GetProductsFromDatabase()
        {
            List<Product> lstProduct = new List<Product>();
            DataTable productLst_Dt = new DataTable();
            try
            {
                SqlCommand cmdFetchProducts = new SqlCommand("select * from product", conObj);
                
                SqlDataAdapter sdaFetchProducts = new SqlDataAdapter(cmdFetchProducts);
                sdaFetchProducts.Fill(productLst_Dt);
                lstProduct = (productLst_Dt.AsEnumerable().Select(x => new Product
                {
                    ProductID = Convert.ToInt32(x["ProductID"]),
                    ProductName = Convert.ToString(x["ProductName"]),
                    ProductPrice = Convert.ToDouble(x["ProductPrice"]),
                    Vendor = Convert.ToString(x["Vendor"]),
                    Discount = Convert.ToDouble(x["Discount"]),
                    ProductImage = storageUri + "products/" + Convert.ToString(x["ProductImage"]),
                })).ToList();
                
                logger.LogInformation("Successfully fetched the products from DB");

            }
            catch (Exception e)
            {

                lstProduct = null;
                logger.LogInformation("Failed in fetching the products "+e.Message);
            }
            finally
            {
                conObj.Close();
            }
            return lstProduct;

        }


        public bool AddSubscriberDAL(string emailID)
        {
            bool result = false;
            cmdObj = new SqlCommand("usp_AddSubscriber", conObj);
            cmdObj.CommandType = CommandType.StoredProcedure;
            SqlParameter prmEmailID = new SqlParameter("@emailID", emailID);
            prmEmailID.Direction = ParameterDirection.Input;
            cmdObj.Parameters.Add(prmEmailID);

            try
            {
                SqlParameter prmReturnValue = new SqlParameter();
                prmReturnValue.Direction = ParameterDirection.ReturnValue;
                cmdObj.Parameters.Add(prmReturnValue);
                conObj.Open();
                cmdObj.ExecuteNonQuery();
                int res = Convert.ToInt32(prmReturnValue.Value);
                if (res == 1)
                    result = true;//it means added
                else
                    result = false;//error
            }
            catch (Exception e)
            {
                result = false;
                    
            }
            finally
            {
                conObj.Close();
            }
            return result;

        }


    }
}
