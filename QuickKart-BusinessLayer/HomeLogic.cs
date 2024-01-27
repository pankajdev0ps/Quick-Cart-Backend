using Microsoft.Extensions.Logging;
using QuickKart_DataAccessLayer;
using QuickKart_DataAccessLayer.Models;
using System;
using System.Collections.Generic;

namespace QuickKart_BusinessLayer
{
    public class HomeLogic
    {
        private readonly ILogger<CustomerRepository> logger;
        private CustomerRepository cust_repository;

        public HomeLogic(ILogger<CustomerRepository> _logger)
        {
            logger= _logger;
            cust_repository= new CustomerRepository(logger);
        }

        public List<Product> FetchProductsLogic()
        {
            List<Product> productLst = null;
            try
            {
                productLst = cust_repository.GetProductsFromDatabase();

            }
            catch (Exception e)
            {
                logger.LogError("Error thrown while fetching products in BL " + e.Message);
            }
            return productLst;

        }

    }
}
