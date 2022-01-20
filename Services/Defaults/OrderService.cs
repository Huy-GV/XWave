using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.Models;
using XWave.Data;
using XWave.Services.Interfaces;
using System.Linq;
using XWave.DTOs;
using System;
using XWave.ViewModels.Purchase;
using Microsoft.Extensions.Logging;

namespace XWave.Services.Defaults
{
    public class OrderService : IOrderService
    {
        //TODO: remove db context
        private readonly DbSet<Order> _orderContext;
        private readonly XWaveDbContext _dbContext;
        private readonly ILogger<OrderService> _logger;
        public OrderService(
            XWaveDbContext dbContext,
            ILogger<OrderService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
            //TODO: replace dbcontext with respective sets
            _orderContext = dbContext.Order;
        }
        public async Task<Tuple<bool, string>> CreateOrderAsync(
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
                    return Tuple.Create(false, "Customer not found");

                var payment = await _dbContext.Payment
                    .SingleOrDefaultAsync(p => p.ID == purchaseVM.PaymentID);

                if (payment == null)
                    return Tuple.Create(false, "Payment not found");

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
                        return Tuple.Create(false, "Ordered product not found");

                    if (product.Quantity < purchasedProduct.Quantity)
                        return Tuple.Create(false, "Quantity exceeded existing stock");

                    //prevent customers from ordering based on incorrect data
                    if (product.Price != purchasedProduct.DisplayedPrice ||
                        product.Discount.Percentage != purchasedProduct.DisplayedDiscount)
                        return Tuple.Create(false, "Conflicting data about product");

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

                return Tuple.Create(false, string.Empty);

            }
            catch (Exception exception)
            {
                await transaction.RollbackToSavepointAsync(savepoint);
                _logger.LogError(exception.Message);
                _logger.LogError(exception.StackTrace);
                return Tuple.Create(false, "Unknown error");
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

        Task<IEnumerable<OrderDTO>> IOrderService.GetAllOrdersAsync(string customerID)
        {
            
            var orderDTOs =  _orderContext
                .Include(o => o.OrderDetailCollection)
                    .ThenInclude(od => od.Product)
                .Include(o => o.Payment)
                .Where(o => o.CustomerID == customerID)
                .Select(o => new OrderDTO()
                {
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
