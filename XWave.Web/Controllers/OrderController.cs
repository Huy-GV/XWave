using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XWave.Web.Extensions;
using XWave.Core.Data.Constants;
using XWave.Core.DTOs.Customers;
using XWave.Core.Models;
using XWave.Core.Services.Interfaces;
using XWave.Core.ViewModels.Customers;
using XWave.Web.Utils;

namespace XWave.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly IAuthenticationHelper _authenticationHelper;
    private readonly IOrderService _orderService;

    public OrderController(
        IOrderService orderService,
        IAuthenticationHelper authenticationHelper)
    {
        _orderService = orderService;
        _authenticationHelper = authenticationHelper;
    }

    [HttpGet]
    [Authorize(Roles = nameof(RoleNames.Customer))]
    public async Task<ActionResult<OrderDetails>> GetOrdersAsync()
    {
        var customerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _orderService.FindAllOrdersAsync(customerId);

        return result.OnSuccess(Ok);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = nameof(RoleNames.Customer))]
    public async Task<ActionResult<OrderDto>> GetOrderById(int id)
    {
        var customerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        var result = await _orderService.FindOrderByIdAsync(customerId, id);

        return result.OnSuccess(Ok);
    }

    [HttpPost]
    [Authorize(Roles = nameof(RoleNames.Customer))]
    public async Task<IActionResult> CreateOrder([FromBody] PurchaseViewModel purchaseViewModel)
    {
        var customerId = _authenticationHelper.GetUserId(HttpContext.User.Identity);
        if (string.IsNullOrEmpty(customerId))
        {
            return Forbid();
        }

        var result = await _orderService.AddOrderAsync(purchaseViewModel, customerId);
        return result.OnSuccess(x => this.Created($"{this.ApiUrl()}/order/{x}"));
    }
}