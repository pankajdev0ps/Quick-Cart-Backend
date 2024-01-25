using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.SqlServer.Server;
using QuickKart_DataAccessLayer;
using QuickKart_DataAccessLayer.Models;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Azure.Core.HttpHeader;

namespace QuickKartWebService.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> logger;

        public AdminController(ILogger<AdminController> _logger)
        {
            logger = _logger;
        }

        [HttpPost, DisableRequestSizeLimit]
        public async Task<IActionResult> Upload()
        {
            try
            {
                var formCollection = await Request.ReadFormAsync();
                //logger.LogInformation($"Form Collection: {formCollection.Count}");
                var file = formCollection.Files.GetFile("image");
                var fileName = file.FileName;
                //logger.LogInformation("FileName: " + fileName);

                var product = new Product
                {
                    ProductName = Convert.ToString(formCollection["productName"]),
                    ProductPrice = Convert.ToDouble(formCollection["productPrice"]),
                    Discount = Convert.ToDouble(formCollection["productQuantity"]),
                    ProductImage = Convert.ToString(fileName)
                };

                /*foreach (var kv in formCollection)
                {
                    logger.LogInformation($"Form data: {kv.Key}: {kv.Value}");
                }*/

                string url = "";
                if (file.Length > 0)
                {
                    string connectionString = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build().GetConnectionString("StorageConnectionString");
                    string containerName = "products";
                    BlobContainerClient container = new BlobContainerClient(connectionString, containerName);

                    try
                    {
                        // Get a reference to a blob
                        BlobClient blob = container.GetBlobClient(fileName);

                        // Open the file and upload its data
                        using (Stream file1 = file.OpenReadStream())
                        {
                            blob.Upload(file1);
                        }

                        url = blob.Uri.AbsoluteUri;
                    }
                    catch(Exception e)
                    {
                        logger.LogInformation("Exception: " + e.Message);
                        return BadRequest();
                    }
                    
                    logger.LogInformation($"{url}");

                    if(this.AddProduct(product) == 0)
                    {
                        return BadRequest();
                    }
                  
                    return new OkObjectResult(new {bloburl=url});
                }
                else
                {
                    /*if (file.Length <= 0)
                    {
                        logger.LogInformation("File Length Error");
                    }
                    else
                    {
                        logger.LogInformation("Form Data Error");
                    }*/
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }

        private int AddProduct(Product p)
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            var config = builder.Build();
            var connectionString = config.GetConnectionString("DBConnectionString");
            
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            SqlCommand sqlCommand = new SqlCommand();

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
