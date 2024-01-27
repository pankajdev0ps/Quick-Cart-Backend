using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QuickKart_DataAccessLayer;
using QuickKart_DataAccessLayer.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using static Azure.Core.HttpHeader;

namespace QuickKartWebService.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminRepository> logger;

        private AdminRepository admin_repository;

        public AdminController(ILogger<AdminRepository> _logger)
        {
            logger = _logger;
            admin_repository = new AdminRepository(logger);
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

                    if(admin_repository.AddProduct(product) == 0)
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

    }
}
