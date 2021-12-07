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
            try
            {
                var customer = DbContext.Customer
                    .SingleAsync(c => c.ID == purchaseVM.CustomerID);
                if (customer == null)
                    return NotFound();

                var payment = DbContext.Payment
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
                List<OrderDetail> orderDetailCollection = new();

                foreach (var productPurchase in purchaseVM.ProductCart)
                {
                    var product = await DbContext.Product
                        .SingleOrDefaultAsync(p => p.ID == productPurchase.ProductID);
                    if (product == null)
                        return NotFound();
                    if (product.Quantity < productPurchase.Quantity)
                        return BadRequest();

                    product.Quantity -= productPurchase.Quantity;
                    purchasedProducts.Add(product);
                    orderDetailCollection.Add(new OrderDetail
                    {
                        //TODO: fix model to uint
                        Quantity = (uint)Math.Abs(productPurchase.Quantity),
                        ProductID = productPurchase.ProductID,
                        PriceAtOrder = productPurchase.Price,
                    });

                }

                DbContext.Order.Add(order);
                await DbContext.SaveChangesAsync();
                foreach (var orderDetail in orderDetailCollection)
                {
                    orderDetail.OrderID = order.ID;
                }
 
                DbContext.OrderDetail.AddRange(orderDetailCollection);
                DbContext.Product.UpdateRange(purchasedProducts);

                await UpdatePaymentDetailAsync(purchaseVM.PaymentID, purchaseVM.CustomerID);
                await DbContext.SaveChangesAsync();
                transaction.Commit();

                return Ok(ResponseTemplate
                    .Created(""));

            } catch (Exception exception)
            {
                transaction.Rollback();
                //replace with 500 status
                return BadRequest();
            }
        }

        private async Task UpdatePaymentDetailAsync(
            int paymentID,
            int customerID)
        {
            var paymentDetail = await DbContext.PaymentDetail.FirstOrDefaultAsync(pd => pd.PaymentID == paymentID && pd.CustomerID == customerID);

            paymentDetail.PurchaseCount++;
            paymentDetail.LatestPurchase = DateTime.Now;
            DbContext.PaymentDetail.Update(paymentDetail);
            

            await DbContext.SaveChangesAsync();
            
        }


        //TODO: validate before creating order
    }
}
