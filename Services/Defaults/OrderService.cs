using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.Models;
using XWave.Data;
using XWave.Services.Interfaces;
using System.Linq;
using XWave.DTOs;
using System;
using XWave.ViewModels.Customer;
using Microsoft.Extensions.Logging;
using XWave.Services.ResultTemplate;

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
            PurchaseVM purchaseVM, 
            string customerID)
        {
            using var transaction = _dbContext.Database.BeginTransaction();
            string savepoint = "BeforePurchaseConfirmation";

            transaction.CreateSavepoint(savepoint);
            try
            {
                var customer = await _dbContext.Customer
                    .SingleOrDefaultAsync(c => c.CustomerID == customerID);

                if (customer == null)
                    return ServiceResult.Failure("Customer not found");

                var payment = await _dbContext.Payment
                    .SingleOrDefaultAsync(p => p.ID == purchaseVM.PaymentID);

                if (payment == null)
                    return ServiceResult.Failure("Payment not found");

                var order = new Order()
                {
                    Date = DateTime.Now,
                    CustomerID = customerID,
                    PaymentID = purchaseVM.PaymentID,
                };

                List<Product> purchasedProducts = new();
                List<OrderDetail> orderDetails = new();

                foreach (var purchasedProduct in purchaseVM.ProductCart)
                {
                    var product = await _dbContext.Product
                        .Include(p => p.Discount)
                        .SingleOrDefaultAsync(p => p.ID == purchasedProduct.ProductID);
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
                        product.Discount.Percentage != purchasedProduct.DisplayedDiscount)
                    {
                        return ServiceResult.Failure("Conflicting data about product");
                    }

                    product.Quantity -= purchasedProduct.Quantity;
                    purchasedProducts.Add(product);
                    orderDetails.Add(new OrderDetail
                    {
                        Quantity = purchasedProduct.Quantity,
                        ProductID = purchasedProduct.ProductID,
                        PriceAtOrder = product.Price - product.Price * product.Discount.Percentage / 100,
                    });
                }

                _dbContext.Order.Add(order);
                //call SaveChanges to get the generated ID
                await _dbContext.SaveChangesAsync();

                orderDetails = AssignOrderID(order.ID, orderDetails);

                _dbContext.OrderDetail.AddRange(orderDetails);
                _dbContext.Product.UpdateRange(purchasedProducts);
                await UpdatePaymentDetailAsync(purchaseVM.PaymentID, customerID);
                await _dbContext.SaveChangesAsync();

                transaction.Commit();

                return ServiceResult.Success(order.ID.ToString());

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
                orderDetail.OrderID = orderID;

            return orderDetails;
        }

        private async Task UpdatePaymentDetailAsync(
            int paymentID,
            string customerID)
        {
            var paymentDetail = await _dbContext.PaymentDetail.SingleAsync(
                pd => pd.PaymentID == paymentID && pd.CustomerID == customerID);

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
                .Where(o => o.CustomerID == customerID)
                .Select(o => new OrderDTO()
                {
                    ID = o.ID,
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
                od => od.ProductID == productID && od.OrderID == orderID);

        }
    }
}
