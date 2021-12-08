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
        [HttpGet("detail")]
        //[Authorize(Policy ="StaffOnly")]
        public async Task<ActionResult<Order>> GetOrderDetailAsync()
        {
            return Ok(await DbContext.OrderDetail.ToListAsync());
        }
        [HttpPost]
        //[Authorize()]
        public async Task<IActionResult> CreateOrder([FromBody] PurchaseVM purchaseVM)
        {
            //CHECK IF THE REQUEST IS SENT BY THE CORRECT CUSTOMER

            using var transaction = DbContext.Database.BeginTransaction();
            string savepoint = "BeforePurchaseConfirmation";
            transaction.CreateSavepoint(savepoint);
            try
            {
                var customer = await DbContext.Customer
                    .SingleAsync(c => c.ID == purchaseVM.CustomerID);
                if (customer == null)
                    return NotFound();

                var payment = await DbContext.Payment
                    .SingleAsync(p => p.ID == purchaseVM.PaymentID);
                if (payment == null)
                    return NotFound();

                var order = new Order()
                {
                    Date = DateTime.Now,
                    CustomerID = purchaseVM.CustomerID,
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
                await UpdatePaymentDetailAsync(purchaseVM.PaymentID, purchaseVM.CustomerID);
                await DbContext.SaveChangesAsync();

                transaction.Commit();

                return Ok(ResponseTemplate
                    .Created(""));

            } catch (Exception exception)
            {
                await transaction.RollbackToSavepointAsync(savepoint);
                Logger.LogError(exception.Message);
                //replace with 500 status
                return StatusCode(500, ResponseTemplate.InternalServerError());
            }
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
            int customerID)
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
