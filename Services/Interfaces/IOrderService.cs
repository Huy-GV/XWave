using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.DTOs;
using XWave.Models;
using XWave.Services.ResultTemplate;
using XWave.ViewModels.Customer;

namespace XWave.Services.Interfaces
{
    public interface IOrderService
    {
        Task<ServiceResult> CreateOrderAsync(PurchaseViewModel purchaseViewModel, string customerID);
        Task<IEnumerable<OrderDTO>> GetAllOrdersAsync(string customerID);
        Task<OrderDTO> GetOrderByIDAsync(string customerID, int orderID);
        Task<OrderDetail> GetDetailsByOrderIDsAsync(int orderID, int productID);
        Task<IEnumerable<OrderDetail>> GetAllOrderDetailsAsync();
    }
}
