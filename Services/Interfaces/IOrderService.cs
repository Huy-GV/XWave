using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.DTOs;
using XWave.DTOs.Customers;
using XWave.Models;
using XWave.Services.ResultTemplate;
using XWave.ViewModels.Customers;

namespace XWave.Services.Interfaces
{
    public interface IOrderService
    {
        Task<ServiceResult> AddOrderAsync(PurchaseViewModel purchaseViewModel, string customerId);
        Task<IEnumerable<OrderDto>> FindAllOrdersAsync(string customerId);
        Task<OrderDto?> FindOrderByIdAsync(string customerId, int orderId);
        /// <summary>
        /// Find the details of a purchased product in a specific order.
        /// </summary>
        /// <param name="orderId">ID of order in which the product was purchased.</param>
        /// <param name="productId">Purchased product.</param>
        /// <returns></returns>
        Task<OrderDetails> FindPurchasedProductDetailsByOrderId(int orderId, int productId);
    }
}
