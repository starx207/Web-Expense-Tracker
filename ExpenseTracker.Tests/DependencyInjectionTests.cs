using ExpenseTracker.Controllers;
using ExpenseTracker.Repository;
using ExpenseTracker.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ExpenseTracker.Tests
{
    [TestClass]
    public class DependencyInjectionTests
    {
        #region Private Members

        private IServiceCollection _services;

        #endregion // Private Members

        #region Test Initialization

        [TestInitialize]
        public void InitializeTestObjects() {
            // Arrange (all tests use this same arrangement)
            var mockConfigurationSection = new Mock<IConfigurationSection>();
            mockConfigurationSection.Setup(m => m["DefaultConnection"]).Returns("TestConnectionString");
            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(m => m.GetSection("ConnectionStrings")).Returns(mockConfigurationSection.Object);

            _services = new ServiceCollection();
            var target = new Startup(mockConfiguration.Object);

            // Act (partial)
            target.ConfigureServices(_services);
        }

        #endregion // Test Initialization

        #region Tests

        #region Repository Injection Tests

        [TestMethod]
        public void BudgetRepo_is_injected_with_BudgetContext() {
            // Act
            var serviceProvider = _services.BuildServiceProvider();
            var repo = serviceProvider.GetService<IBudgetRepo>();

            // Assert
            Assert.IsNotNull(repo, "No repository is available through the service provider");
        }

        #endregion // Repository Injection Tests

        #region Service Injection Tests

        [TestMethod]
        public void AliasManagerService_is_injected_with_IBudgetRepo() {
            // Act
            var serviceProvider = _services.BuildServiceProvider();
            var service = serviceProvider.GetService<IAliasManagerService>();

            // Assert
            Assert.IsNotNull(service, "No AliasManagerService is available through the service provider");
        }

        [TestMethod]
        public void CategoryManagerService_is_injected_with_IBudgetRepo() {
            // Act
            var serviceProvider = _services.BuildServiceProvider();
            var service = serviceProvider.GetService<ICategoryManagerService>();

            // Assert
            Assert.IsNotNull(service, "No CategoryManagerService is available through the service provider");
        }

        [TestMethod]
        public void PayeeManagerService_is_injected_with_IBudgetRepo() {
            // Act
            var serviceProvider = _services.BuildServiceProvider();
            var service = serviceProvider.GetService<IPayeeManagerService>();

            // Assert
            Assert.IsNotNull(service, "No PayeeManagerService is available through the service provider");
        }

        [TestMethod]
        public void TransactionManagerService_is_injected_with_IBudgetRepo() {
            // Act
            var serviceProvider = _services.BuildServiceProvider();
            var service = serviceProvider.GetService<ITransactionManagerService>();

            // Assert
            Assert.IsNotNull(service, "No TransactionManagerService is available through the service provider");
        }

        #endregion // Service Injection Tests

        #region Controller Injection Tests

        [TestMethod]
        public void AliasController_is_injected_with_IAliasManagerService() {
            // Act
            _services.AddTransient<AliasController>();
            var serviceProvider = _services.BuildServiceProvider();
            var controller = serviceProvider.GetService<AliasController>();

            // Assert
            Assert.IsNotNull(controller, "No AliasController could be created through the service provider");
        }

        [TestMethod]
        public void BudgetCategoryController_is_injected_with_IAliasManagerService() {
            // Act
            _services.AddTransient<BudgetCategoryController>();
            var serviceProvider = _services.BuildServiceProvider();
            var controller = serviceProvider.GetService<BudgetCategoryController>();

            // Assert
            Assert.IsNotNull(controller, "No BudgetCategoryController could be created through the service provider");
        }

        [TestMethod]
        public void PayeeController_is_injected_with_IAliasManagerService() {
            // Act
            _services.AddTransient<PayeeController>();
            var serviceProvider = _services.BuildServiceProvider();
            var controller = serviceProvider.GetService<PayeeController>();

            // Assert
            Assert.IsNotNull(controller, "No PayeeController could be created through the service provider");
        }

        [TestMethod]
        public void TransactionController_is_injected_with_IAliasManagerService() {
            // Act
            _services.AddTransient<TransactionController>();
            var serviceProvider = _services.BuildServiceProvider();
            var controller = serviceProvider.GetService<TransactionController>();

            // Assert
            Assert.IsNotNull(controller, "No TransactionController could be created through the service provider");
        }

        #endregion // Controller Injection Tests

        #endregion // Tests
    }
}