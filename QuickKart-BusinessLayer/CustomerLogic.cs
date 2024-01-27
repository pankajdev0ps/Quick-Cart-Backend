using Microsoft.Extensions.Logging;
using QuickKart_DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickKart_BusinessLayer
{
    public class CustomerLogic
    {

        private readonly ILogger<CustomerRepository> logger;
        private CustomerRepository cust_repository;

        public CustomerLogic(ILogger<CustomerRepository> _logger)
        {
            logger = _logger;
            cust_repository = new CustomerRepository(_logger);

        }

        public bool AddSubscriberBL(string emailID)
        {
            try
            {
                bool result = cust_repository.AddSubscriberDAL(emailID);
                return result;

            }
            catch (Exception e)
            {

                return false;
            }
        }


    }
}
