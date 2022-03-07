﻿using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using XWave.Models;
using XWave.ViewModels.Customers;
using XWave.DTOs.Customers;
using XWave.Helpers;
using XWave.Services.Interfaces;
using XWave.Data.Constants;

namespace XWave.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : AbstractController<OrderController>
    {
        private readonly IOrderService _orderService;
        private readonly AuthenticationHelper _authenticationHelper;
        public OrderController(
            ILogger<OrderController> logger,
            IOrderService orderService,
            AuthenticationHelper authenticationHelper) : base(logger)
        {
            _orderService = orderService;
            _authenticationHelper = authenticationHelper;   
        }
        [HttpGet]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult<OrderDetails>> GetOrdersAsync()
        {
            string customerID = _authenticationHelper.GetUserId(HttpContext.User.Identity);
            if (string.IsNullOrEmpty(customerID))
            {
                return BadRequest(XWaveResponse.Failed("Customer ID not found"));
            }
                
            return Ok(await _orderService.GetAllOrdersAsync(customerID));
        }
        [HttpGet("{id:int}")]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult<OrderDto>> GetOrderByID(int id)
        {
            string customerID = _authenticationHelper.GetUserId(HttpContext.User.Identity);
            if (string.IsNullOrEmpty(customerID))
            {
                return BadRequest(XWaveResponse.Failed("Customer ID not found"));
            }

            var orderDTO = await _orderService.GetOrderByIdAsync(customerID, id);

            return Ok(orderDTO);
        }
        [HttpGet("detail")]
        //[Authorize(Policy ="StaffOnly")]
        public async Task<ActionResult<OrderDetails>> GetOrderDetailsAsync()
        {
            return Ok(await _orderService.GetAllOrderDetailsAsync());
        }
        [HttpGet("detail/{orderID}/{productID}")]
        [Authorize(Policy ="StaffOnly")]
        public async Task<ActionResult<OrderDetails>> GetOrderDetailAsync(int orderID, int productID)
        {
            OrderDetails orderDetail = await _orderService.GetOrderDetailsByIdAsync(orderID, productID);

            return Ok(orderDetail);
        }
        [HttpPost]
        [Authorize(Roles ="customer")]
        public async Task<IActionResult> CreateOrder([FromBody] PurchaseViewModel purchaseViewModel)
        {
            string customerID = _authenticationHelper.GetUserId(HttpContext.User.Identity);
            if (string.IsNullOrEmpty(customerID))
            {
                return XWaveBadRequest("Customer ID not found");
            }

            var result = await _orderService.CreateOrderAsync(purchaseViewModel, customerID);

            if (!result.Succeeded)
            {
                return XWaveBadRequest(result.Error);
            }
            
            return Ok();
        }
    }
}
