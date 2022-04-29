using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.DTOs.Customers;
using XWave.Models;
using XWave.Services.ResultTemplate;
using XWave.ViewModels.Customers;

namespace XWave.Services.Interfaces
{
    public interface IOrderService
    {
        /// <summary>
        /// Add an order when the customer makes a purchase.
        /// </summary>
        /// <param name="purchaseViewModel">ViewModel containing details of a purchase.</param>
        /// <param name="customerId">ID of customer who made the purchase.</param>
        /// <returns></returns>
        Task<(ServiceResult, int? OrderId)> AddOrderAsync(PurchaseViewModel purchaseViewModel, string customerId);

        /// <summary>
        /// Find all orders made by a customer.
        /// </summary>
        /// <param name="customerId">ID of customer.</param>
        /// <returns></returns>
        Task<IEnumerable<OrderDto>> FindAllOrdersAsync(string customerId);

        /// <summary>
        /// Find a specific order of a customer
        /// </summary>
        /// <param name="customerId">ID of customer.</param>
        /// <param name="orderId">ID of order.</param>
        /// <returns></returns>
        Task<OrderDto?> FindOrderByIdAsync(string customerId, int orderId);
    }
}