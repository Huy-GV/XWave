﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using XWave.Data.Constants;
using XWave.DTOs.Customers;
using XWave.Extensions;
using XWave.Utils;
using XWave.Models;
using XWave.Services.Interfaces;
using XWave.ViewModels.Customers;

namespace XWave.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
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
        [Authorize(Roles = nameof(Roles.Customer))]
        public async Task<ActionResult<OrderDetails>> GetOrdersAsync()
        {
            var customerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
            if (string.IsNullOrEmpty(customerId))
            {
                return BadRequest(XWaveResponse.Failed("Customer ID not found."));
            }

            return Ok(await _orderService.FindAllOrdersAsync(customerId));
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = nameof(Roles.Customer))]
        public async Task<ActionResult<OrderDto>> GetOrderById(int id)
        {
            var customerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
            if (string.IsNullOrEmpty(customerId))
            {
                return BadRequest(XWaveResponse.Failed("Customer ID not found."));
            }

            var orderDto = await _orderService.FindOrderByIdAsync(customerId, id);
            return orderDto != null ? Ok(orderDto) : NotFound();
        }

        [HttpPost]
        [Authorize(Roles = nameof(Roles.Customer))]
        public async Task<IActionResult> CreateOrder([FromBody] PurchaseViewModel purchaseViewModel)
        {
            var customerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
            if (string.IsNullOrEmpty(customerId))
            {
                return this.XWaveBadRequest("Customer ID not found.");
            }

            var (result, orderId) = await _orderService.AddOrderAsync(purchaseViewModel, customerId);

            if (!result.Succeeded)
            {
                return UnprocessableEntity(result.Errors);
            }

            return Ok("");
        }
    }
}