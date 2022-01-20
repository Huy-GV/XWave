using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.DTOs;
using XWave.Models;
using XWave.ViewModels.Purchase;

namespace XWave.Services.Interfaces
{
    public interface IOrderService
    {
        Task<Tuple<bool, string>> CreateOrderAsync(PurchaseVM purchaseVM, string customerID);
        Task<IEnumerable<OrderDTO>> GetAllOrdersAsync(string customerID);
        Task<OrderDetail> GetDetailsByOrderIDsAsync(int orderID, int productID);
        Task<IEnumerable<OrderDetail>> GetAllOrderDetailsAsync();
    }
}
