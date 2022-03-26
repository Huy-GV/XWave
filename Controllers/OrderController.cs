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
    public class OrderController : XWaveBaseController
    {
        private readonly IOrderService _orderService;
        private readonly AuthenticationHelper _authenticationHelper;
        public OrderController(
            IOrderService orderService,
            AuthenticationHelper authenticationHelper)
        {
            _orderService = orderService;
            _authenticationHelper = authenticationHelper;   
        }
        [HttpGet]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult<OrderDetails>> GetOrdersAsync()
        {
            string customerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
            if (string.IsNullOrEmpty(customerId))
            {
                return BadRequest(XWaveResponse.Failed("Customer ID not found"));
            }
                
            return Ok(await _orderService.FindAllOrdersAsync(customerId));
        }
        [HttpGet("{id:int}")]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult<OrderDto>> GetOrderById(int id)
        {
            string customerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
            if (string.IsNullOrEmpty(customerId))
            {
                return BadRequest(XWaveResponse.Failed("Customer ID not found"));
            }

            var orderDTO = await _orderService.FindOrderByIdAsync(customerId, id);

            return Ok(orderDTO);
        }
        [HttpGet("detail/{orderId}/{productId}")]
        [Authorize(Policy ="StaffOnly")]
        public async Task<ActionResult<OrderDetails>> GetOrderDetailAsync(int orderId, int productId)
        {
            OrderDetails orderDetail = await _orderService.FindPurchasedProductDetailsByOrderId(orderId, productId);

            return Ok(orderDetail);
        }
        [HttpPost]
        [Authorize(Roles ="customer")]
        public async Task<IActionResult> CreateOrder([FromBody] PurchaseViewModel purchaseViewModel)
        {
            string customerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
            if (string.IsNullOrEmpty(customerId))
            {
                return XWaveBadRequest("Customer ID not found");
            }

            var result = await _orderService.AddOrderAsync(purchaseViewModel, customerId);

            if (!result.Succeeded)
            {
                return XWaveBadRequest(result.Error);
            }
            
            return Ok();
        }
    }
}
