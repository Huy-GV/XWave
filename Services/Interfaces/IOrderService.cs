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
        Task<ServiceResult> CreateOrderAsync(PurchaseViewModel purchaseViewModel, string customerId);
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync(string customerId);
        Task<OrderDto> GetOrderByIdAsync(string customerId, int orderId);
        Task<OrderDetail> GetDetailsByOrderIdsAsync(int orderId, int productId);
        Task<IEnumerable<OrderDetail>> GetAllOrderDetailsAsync();
    }
}
