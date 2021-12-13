﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using XWave.Data;
using XWave.Data.Constants;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using XWave.Models;
using XWave.ViewModels.Purchase;
using System.Security.Claims;
using XWave.DTOs;

namespace XWave.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : AbstractController<OrderController>
    {
        public OrderController(
            XWaveDbContext dbContext,
            ILogger<OrderController> logger) : base(dbContext, logger)
        {

        }
        [HttpGet]
        [Authorize(Roles = "customer")]
        public ActionResult<OrderDetail> GetOrders()
        {
            string customerID = GetCustomerID();
            if (customerID == string.Empty)
                return BadRequest();

            //works without ThenIncludee()
            var orders = DbContext.Order
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
                .ToList();

            return Ok(orders);
        }
        [HttpGet("detail")]
        //[Authorize(Policy ="StaffOnly")]
        public async Task<ActionResult<OrderDetail>> GetOrderDetailsAsync()
        {
            return Ok(await DbContext.OrderDetail.ToListAsync());
        }
        [HttpGet("detail/{orderID}/{productID}")]
        //[Authorize(Policy ="StaffOnly")]
        public async Task<ActionResult<OrderDetail>> GetOrderDetailAsync(int orderID, int productID)
        {
            OrderDetail orderDetail = await DbContext.OrderDetail
                    .FirstOrDefaultAsync(od =>
                    od.ProductID == productID && od.OrderID == orderID);

            if (orderDetail == null)
                return NotFound();

            return Ok(orderDetail);
        }
        [HttpPost]
        [Authorize(Roles ="customer")]
        public async Task<IActionResult> CreateOrder([FromBody] PurchaseVM purchaseVM)
        {
            string customerID = GetCustomerID();
            if (customerID == string.Empty)
                return BadRequest();

            using var transaction = DbContext.Database.BeginTransaction();
            string savepoint = "BeforePurchaseConfirmation";

            transaction.CreateSavepoint(savepoint);
            try
            {
                var customer = await DbContext.Customer
                    .SingleAsync(c => c.CustomerID == customerID);

                var payment = await DbContext.Payment
                    .SingleAsync(p => p.ID == purchaseVM.PaymentID);

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
                    var product = await DbContext.Product
                        .Include(p => p.Discount)
                        .SingleOrDefaultAsync(p => p.ID == purchasedProduct.ProductID);
                    if (product == null)
                        return NotFound("Ordered product not found");
                    if (product.Quantity < purchasedProduct.Quantity)
                        return BadRequest("Quantity exceeded existing stock");

                    //prevent customers from ordering based on incorrect data
                    if (product.Price != purchasedProduct.DisplayedPrice ||
                        product.Discount.Percentage != purchasedProduct.DiscountPercentage)
                        return BadRequest("Conflicting data about product");

                    product.Quantity -= purchasedProduct.Quantity;
                    purchasedProducts.Add(product);
                    orderDetails.Add(new OrderDetail
                    {
                        Quantity = purchasedProduct.Quantity,
                        ProductID = purchasedProduct.ProductID,
                        PriceAtOrder = product.Price - product.Price * product.Discount.Percentage / 100,
                    });
                }

                DbContext.Order.Add(order);
                //call SaveChanges to get the generated ID
                await DbContext.SaveChangesAsync();

                AssignOrderID(order.ID, orderDetails);

                DbContext.OrderDetail.AddRange(orderDetails);
                DbContext.Product.UpdateRange(purchasedProducts);
                await UpdatePaymentDetailAsync(purchaseVM.PaymentID, customerID);
                await DbContext.SaveChangesAsync();

                transaction.Commit();

                return Ok(ResponseTemplate
                    .Created($"https://localhost:5001/api/order/detail/{order.ID}/pro"));

            }
            catch (Exception exception)
            {
                await transaction.RollbackToSavepointAsync(savepoint);
                Logger.LogError(exception.Message);
                Logger.LogError(exception.StackTrace);

                return StatusCode(500, ResponseTemplate.InternalServerError());
            }
        }
        private string GetCustomerID()
        {
            var customerID = string.Empty;
            ClaimsIdentity identity = (ClaimsIdentity)HttpContext.User.Identity;

            customerID = identity?.FindFirst(CustomClaim.CustomerID)?.Value;

            Logger.LogCritical($"Customer id in jwt claim: {customerID}");
            return customerID;

        }
        private void AssignOrderID(int orderID, List<OrderDetail> orderDetails)
        {
            foreach (var orderDetail in orderDetails)
                orderDetail.OrderID = orderID;
            
        }

        private async Task UpdatePaymentDetailAsync(
            int paymentID,
            string customerID)
        {
            var paymentDetail = await DbContext.PaymentDetail
                .SingleAsync(pd =>
                pd.PaymentID == paymentID && pd.CustomerID == customerID);

            paymentDetail.PurchaseCount++;
            paymentDetail.LatestPurchase = DateTime.Now;
            DbContext.PaymentDetail.Update(paymentDetail);
        }
    }
}
