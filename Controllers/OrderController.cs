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
using XWave.ViewModels.Customer;
using System.Security.Claims;
using XWave.DTOs;
using XWave.Services;
using XWave.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace XWave.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : AbstractController<OrderController>
    {
        private readonly IAuthenticationService _authService;
        private readonly IOrderService _orderService;

        public OrderController(
            ILogger<OrderController> logger,
            IAuthenticationService authService,
            IOrderService orderService) : base(logger)
        {
            _authService = authService;
            _orderService = orderService;
        }
        [HttpGet]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult<OrderDetail>> GetOrdersAsync()
        {
            string customerID = _authService.GetUserID(HttpContext.User.Identity);
            if (customerID == string.Empty)
                return BadRequest();

            return Ok(_orderService.GetAllOrdersAsync(customerID).Result);
        }
        [HttpGet("{id:int}")]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult<OrderDTO>> GetOrderByID(int id)
        {
            string customerID = _authService.GetUserID(HttpContext.User.Identity);
            if (customerID == string.Empty)
            {
                return BadRequest();
            }

            var orderDTO = await _orderService.GetOrderByIDAsync(customerID, id);

            return Ok(orderDTO);
        }
        [HttpGet("detail")]
        //[Authorize(Policy ="StaffOnly")]
        public async Task<ActionResult<OrderDetail>> GetOrderDetailsAsync()
        {
            return Ok(await _orderService.GetAllOrderDetailsAsync());
        }
        [HttpGet("detail/{orderID}/{productID}")]
        [Authorize(Policy ="StaffOnly")]
        public async Task<ActionResult<OrderDetail>> GetOrderDetailAsync(int orderID, int productID)
        {
            OrderDetail orderDetail = await _orderService.GetDetailsByOrderIDsAsync(orderID, productID);

            return Ok(orderDetail);
        }
        [HttpPost]
        [Authorize(Roles ="customer")]
        public async Task<IActionResult> CreateOrder([FromBody] PurchaseVM purchaseVM)
        {
            string customerID = _authService.GetUserID(HttpContext.User.Identity);
            if (customerID == string.Empty)
            {
                return BadRequest();
            }
                
            var result = await _orderService.CreateOrderAsync(purchaseVM, customerID);

            if (!result.Succeeded)
            {
                return BadRequest(result.Error);
            }
            
            return Ok();
        }
    }
}
