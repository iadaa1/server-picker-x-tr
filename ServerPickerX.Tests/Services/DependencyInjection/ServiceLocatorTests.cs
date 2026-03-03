using Microsoft.Extensions.DependencyInjection;
using Moq;
using ServerPickerX.Services.DependencyInjection;
using Xunit;

namespace ServerPickerX.Services.DependencyInjection.Tests
{
    public class ServiceLocatorTests
    {
        [Fact]
        public void Initialize_SetsServiceProvider()
        {
            // Arrange
            var mockProvider = new Mock<IServiceProvider>();
            ServiceLocator.Initialize(mockProvider.Object);

            // Act & Assert
            Assert.NotNull(ServiceLocator.Provider);
            Assert.Same(mockProvider.Object, ServiceLocator.Provider);
        }

        [Fact]
        public void GetService_ReturnsService_WhenInitialized()
        {
            // Arrange
            var mockProvider = new Mock<IServiceProvider>();
            var mockService = new Mock<IServiceProvider>();
            mockProvider.Setup(p => p.GetService(typeof(IServiceProvider))).Returns(mockService.Object);
            ServiceLocator.Initialize(mockProvider.Object);

            // Act
            var result = ServiceLocator.GetService<IServiceProvider>();

            // Assert
            Assert.NotNull(result);
            Assert.Same(mockService.Object, result);
        }

        [Fact]
        public void GetService_Throws_WhenNotInitialized()
        {
            // Arrange
            ServiceLocator.Initialize(null);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => ServiceLocator.GetService<IServiceProvider>());
        }

        [Fact]
        public void GetRequiredService_ReturnsService_WhenInitialized()
        {
            // Arrange
            var mockProvider = new Mock<IServiceProvider>();
            var mockService = new Mock<IServiceProvider>();
            mockProvider.Setup(p => p.GetService(typeof(IServiceProvider))).Returns(mockService.Object);
            ServiceLocator.Initialize(mockProvider.Object);

            // Act
            var result = ServiceLocator.GetRequiredService<IServiceProvider>();

            // Assert
            Assert.NotNull(result);
            Assert.Same(mockService.Object, result);
        }

        [Fact]
        public void GetRequiredService_Throws_WhenNotInitialized()
        {
            // Arrange
            ServiceLocator.Initialize(null);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => ServiceLocator.GetRequiredService<IServiceProvider>());
        }
    }
}
