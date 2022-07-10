using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using XWave.Core.Services.Communication;
using XWave.Core.Services.Implementations;
using XWave.Core.Services.Interfaces;
using XWave.Core.ViewModels.Customers;

namespace XWave.Test.Services
{
    [TestClass]
    public class OrderServiceTest : BaseTest
    {
        private readonly IOrderService _orderService;
        private readonly Mock<IAuthenticationService> _mockAuthService = new();
        private readonly Mock<ICustomerAccountService> _mockCustomerAccountService = new();
        private readonly Mock<IProductService> _mockProductService = new();
        private readonly Mock<IPaymentService> _mockPaymentService = new();
        private readonly Mock<ILogger<OrderService>> _mockLog = new();

        public OrderServiceTest()
        {
            var dbContext = CreateDbContext();
            _orderService = new OrderService(
                dbContext,
                _mockLog.Object,
                _mockAuthService.Object,
                _mockProductService.Object,
                _mockCustomerAccountService.Object,
                _mockPaymentService.Object);
        }

        [TestMethod]
        public void AddOrder_ShouldFail_IfCustomerAccountDoesNotExist()
        {
            var purchaseViewModel = new PurchaseViewModel();
            var customerId = new Guid().ToString();
            _mockCustomerAccountService.Setup(x => x.CustomerAccountExists(customerId).Result).Returns(false);
            _orderService.AddOrderAsync(purchaseViewModel, customerId)
                .Result
                .Should()
                .BeEquivalentTo(ServiceResult<int>.Failure(new Error()
                {
                    ErrorCode = ErrorCode.EntityNotFound,
                    Message = "Customer account not found"
                }));
        }

        [TestMethod]
        public void AddOrder_ShouldFail_IfPaymentAccountIsInvalid()
        {
            var purchaseViewModel = new PurchaseViewModel() { PaymentAccountId = new Random().Next() };
            var customerId = new Guid().ToString();
            _mockCustomerAccountService.Setup(x => x.CustomerAccountExists(customerId).Result).Returns(true);
            _mockPaymentService
                .Setup(x => x.CustomerHasPaymentAccount(customerId, purchaseViewModel.PaymentAccountId, false).Result)
                .Returns(false);
            _orderService.AddOrderAsync(purchaseViewModel, customerId)
                .Result
                .Should()
                .BeEquivalentTo(ServiceResult<int>.Failure(new Error
                {
                    ErrorCode = ErrorCode.EntityNotFound,
                    Message = "Valid payment account not found"
                }));
        }

        [TestMethod]
        public void AddOrder_ShouldFail_IfPurchasedProductsNotFound()
        {
        }
    }
}