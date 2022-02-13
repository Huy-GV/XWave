using Microsoft.AspNetCore.Mvc;
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
        public async Task<ActionResult<OrderDetail>> GetOrdersAsync()
        {
            string customerID = _authenticationHelper.GetUserID(HttpContext.User.Identity);
            if (string.IsNullOrEmpty(customerID))
            {
                return BadRequest(XWaveResponse.Failed("Customer ID not found"));
            }
                
            return Ok(await _orderService.GetAllOrdersAsync(customerID));
        }
        [HttpGet("{id:int}")]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult<OrderDTO>> GetOrderByID(int id)
        {
            string customerID = _authenticationHelper.GetUserID(HttpContext.User.Identity);
            if (string.IsNullOrEmpty(customerID))
            {
                return BadRequest(XWaveResponse.Failed("Customer ID not found"));
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
        public async Task<IActionResult> CreateOrder([FromBody] PurchaseViewModel purchaseViewModel)
        {
            string customerID = _authenticationHelper.GetUserID(HttpContext.User.Identity);
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
