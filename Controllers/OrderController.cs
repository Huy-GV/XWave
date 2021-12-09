using Microsoft.AspNetCore.Mvc;
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
        [HttpGet("{id}")]
        //[Authorize(Policy ="StaffOnly")]
        public async Task<ActionResult<OrderDetail>> GetOrderAsync(int id)
        {
            //TODO: get order, order detail and construct DTO
            return Ok(await DbContext.OrderDetail.ToListAsync());
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
                if (customer == null)
                    return NotFound();

                var payment = await DbContext.Payment
                    .SingleAsync(p => p.ID == purchaseVM.PaymentID);
                if (payment == null)
                    return NotFound();

                var order = new Order()
                {
                    Date = DateTime.Now,
                    CustomerID = customerID,
                    PaymentID = purchaseVM.PaymentID,
                };

                List<Product> purchasedProducts = new();
                List<OrderDetail> orderDetails = new();

                foreach (var productPurchase in purchaseVM.ProductCart)
                {
                    var product = await DbContext.Product
                        .SingleOrDefaultAsync(p => p.ID == productPurchase.ProductID);
                    if (product == null)
                        return NotFound("Ordered product not found");
                    if (product.Quantity < productPurchase.Quantity)
                        return BadRequest("Quantity exceeded existing stock");

                    product.Quantity -= productPurchase.Quantity;
                    purchasedProducts.Add(product);
                    orderDetails.Add(new OrderDetail
                    {
                        Quantity = productPurchase.Quantity,
                        ProductID = productPurchase.ProductID,
                        PriceAtOrder = productPurchase.Price,
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
                //replace with 500 status
                return StatusCode(500, ResponseTemplate.InternalServerError());
            }
        }
        private string GetCustomerID()
        {
            var customerID = string.Empty;
            ClaimsIdentity identity = (ClaimsIdentity)HttpContext.User.Identity;
            if (identity != null)
                customerID = identity.FindFirst(CustomClaim.CustomerID).Value;

            Logger.LogCritical($"Customer id in jwt claim: {customerID}");
            return customerID ?? string.Empty;

        }
        private void AssignOrderID(int orderID, List<OrderDetail> orderDetails)
        {
            foreach (var orderDetail in orderDetails)
            {
                orderDetail.OrderID = orderID;
            }
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
