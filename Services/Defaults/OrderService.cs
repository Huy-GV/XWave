using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.Models;
using XWave.Data;
using XWave.Services.Interfaces;
using System.Linq;
using XWave.DTOs.Customers;
using System;
using XWave.ViewModels.Customers;
using Microsoft.Extensions.Logging;
using XWave.Services.ResultTemplate;
using XWave.DTOs.Customers;

namespace XWave.Services.Defaults
{
    public class OrderService : IOrderService
    {
        private readonly XWaveDbContext _dbContext;
        private readonly ILogger<OrderService> _logger;
        public OrderService(
            XWaveDbContext dbContext,
            ILogger<OrderService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }
        public async Task<OrderDTO> GetOrderByIDAsync(string customerID, int orderID)
        {
            var orderDTOs = await GetAllOrdersAsync(customerID);
            return orderDTOs.FirstOrDefault(o => o.ID == orderID);
        }
        public async Task<ServiceResult> CreateOrderAsync(
            PurchaseViewModel purchaseViewModel, 
            string customerID)
        {
            using var transaction = _dbContext.Database.BeginTransaction();
            string savepoint = "BeforePurchaseConfirmation";

            transaction.CreateSavepoint(savepoint);
            try
            {
                var customer = await _dbContext.Customer
                    .SingleOrDefaultAsync(c => c.CustomerId == customerID);

                if (customer == null)
                {
                    return ServiceResult.Failure("Customer not found");
                }
                 
                var payment = await _dbContext.Payment
                    .SingleOrDefaultAsync(p => p.Id == purchaseViewModel.PaymentID);

                if (payment == null)
                {
                    return ServiceResult.Failure("Payment not found");
                }
                    
                var order = new Order()
                {
                    Date = DateTime.Now,
                    CustomerId = customerID,
                    PaymentAccountId = purchaseViewModel.PaymentID,
                };

                List<Product> purchasedProducts = new();
                List<OrderDetail> orderDetails = new();

                foreach (var purchasedProduct in purchaseViewModel.ProductCart)
                {
                    var product = await _dbContext.Product
                        .Include(p => p.Discount)
                        .SingleOrDefaultAsync(p => p.Id == purchasedProduct.ProductID);
                    if (product == null)
                    {
                        return ServiceResult.Failure("Ordered product not found");
                    }

                    if (product.Quantity < purchasedProduct.Quantity)
                    {
                        return ServiceResult.Failure("Quantity exceeded existing stock");
                    }

                    //prevent customers from ordering based on incorrect data
                    if (product.Price != purchasedProduct.DisplayedPrice ||
                        product.Discount?.Percentage != purchasedProduct.DisplayedDiscount)
                    {
                        return ServiceResult.Failure("Conflicting data about product");
                    }

                    product.Quantity -= purchasedProduct.Quantity;
                    purchasedProducts.Add(product);
                    orderDetails.Add(new OrderDetail
                    {
                        Quantity = purchasedProduct.Quantity,
                        ProductId = purchasedProduct.ProductID,
                        PriceAtOrder = product.Price - product.Price * product.Discount.Percentage / 100,
                    });
                }

                _dbContext.Order.Add(order);
                //call SaveChanges to get the generated ID
                await _dbContext.SaveChangesAsync();

                orderDetails = AssignOrderID(order.Id, orderDetails);

                _dbContext.OrderDetail.AddRange(orderDetails);
                _dbContext.Product.UpdateRange(purchasedProducts);
                await UpdatePaymentDetailAsync(purchaseViewModel.PaymentID, customerID);
                await _dbContext.SaveChangesAsync();

                transaction.Commit();

                return ServiceResult.Success(order.Id.ToString());

            }
            catch (Exception exception)
            {
                await transaction.RollbackToSavepointAsync(savepoint);
                _logger.LogError(exception.Message);
                return ServiceResult.Failure(exception.Message);
            }
        }
        private static List<OrderDetail> AssignOrderID(int orderID, List<OrderDetail> orderDetails)
        {
            foreach (var orderDetail in orderDetails)
            {
                orderDetail.OrderId = orderID;
            }
                
            return orderDetails;
        }

        private async Task UpdatePaymentDetailAsync(
            int paymentID,
            string customerID)
        {
            var paymentDetail = await _dbContext.PaymentDetail.SingleAsync(
                pd => pd.PaymentAccountId == paymentID && pd.CustomerId == customerID);

            paymentDetail.PurchaseCount++;
            paymentDetail.LatestPurchase = DateTime.Now;
            _dbContext.PaymentDetail.Update(paymentDetail);
        }

        public async Task<IEnumerable<OrderDetail>> GetAllOrderDetailsAsync()
        {
            return await _dbContext.OrderDetail.ToListAsync();
        }

        public Task<IEnumerable<OrderDTO>> GetAllOrdersAsync(string customerID)
        {
            
            var orderDTOs =  _dbContext.Order
                .Include(o => o.OrderDetailCollection)
                    .ThenInclude(od => od.Product)
                .Include(o => o.Payment)
                .Where(o => o.CustomerId == customerID)
                .Select(o => new OrderDTO()
                {
                    ID = o.Id,
                    OrderDate = o.Date,
                    AccountNo = o.Payment.AccountNo,
                    OrderDetailCollection = o
                        .OrderDetailCollection
                        .Select(od => new OrderDetailDTO()
                        {
                            Quantity = od.Quantity,
                            Price = od.PriceAtOrder,
                            ProductName = od.Product.Name
                        })
                })
                .AsEnumerable();

            return Task.FromResult(orderDTOs);
        }

        public async Task<OrderDetail> GetDetailsByOrderIDsAsync(int orderID, int productID)
        {
            return await _dbContext.OrderDetail.FirstOrDefaultAsync(
                od => od.ProductId == productID && od.OrderId == orderID);

        }
    }
}
