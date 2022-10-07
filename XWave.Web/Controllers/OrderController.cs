using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XWave.Web.Extensions;
using XWave.Core.Data.Constants;
using XWave.Core.DTOs.Customers;
using XWave.Core.Models;
using XWave.Core.Services.Interfaces;
using XWave.Core.ViewModels.Customers;
using XWave.Web.Data;
using XWave.Web.Utils;

namespace XWave.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly AuthenticationHelper _authenticationHelper;
    private readonly IOrderService _orderService;

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
        return Ok(await _orderService.FindAllOrdersAsync(customerId));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = nameof(Roles.Customer))]
    public async Task<ActionResult<OrderDto>> GetOrderById(int id)
    {
        var customerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var orderDto = await _orderService.FindOrderByIdAsync(customerId, id);
        return orderDto is not null ? Ok(orderDto) : NotFound();
    }

    [HttpPost]
    [Authorize(Roles = nameof(Roles.Customer))]
    public async Task<IActionResult> CreateOrder([FromBody] PurchaseViewModel purchaseViewModel)
    {
        var customerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        if (string.IsNullOrEmpty(customerId))
        {
            return Forbid();
        }

        var result = await _orderService.AddOrderAsync(purchaseViewModel, customerId);

        if (!result.Succeeded) 
        {
            return UnprocessableEntity(result.Errors);
        }

        return this.Created($"https://localhost:5001/api/order/{result.Value}");
    }
}