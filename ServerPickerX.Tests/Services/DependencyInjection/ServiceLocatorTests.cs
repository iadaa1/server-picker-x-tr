using Moq;
using ServerPickerX.Services.MessageBoxes;
using System.Reflection;
using Xunit;

namespace ServerPickerX.Services.DependencyInjection.Tests
{
    public class ServiceLocatorTests
    {
        public object ReflectGetStaticField(Type classType, string propertyName)
        {
            FieldInfo prop = classType.GetField(
                propertyName,
                BindingFlags.Static | BindingFlags.NonPublic
                )!;

            return prop?.GetValue(null);
        }

        [Fact]
        public void Initialize_SetsServiceProvider()
        {
            // Arrange
            var mockProvider = new Mock<IServiceProvider>();

            ServiceLocator.Initialize(mockProvider.Object);

            // Act & Assert
            var serviceProvider = ReflectGetStaticField(typeof(ServiceLocator), "_provider");

            Assert.NotNull(serviceProvider);
            Assert.Same(mockProvider.Object, serviceProvider);
        }

        [Fact]
        public void GetService_ReturnsService_WhenInitialized()
        {
            // Arrange
            var mockProvider = new Mock<IServiceProvider>();
            var mockService = new Mock<IMessageBoxService>();

            mockProvider.Setup(p => p.GetService(typeof(IMessageBoxService))).Returns(mockService.Object);

            ServiceLocator.Initialize(mockProvider.Object);

            // Act
            var result = ServiceLocator.GetService<IMessageBoxService>();

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
            Assert.Throws<InvalidOperationException>(() => ServiceLocator.GetService<IMessageBoxService>());
        }

        [Fact]
        public void GetRequiredService_ReturnsService_WhenInitialized()
        {
            // Arrange
            var mockProvider = new Mock<IServiceProvider>();
            var mockService = new Mock<IMessageBoxService>();
            
            mockProvider.Setup(p => p.GetService(typeof(IServiceProvider))).Returns(mockService.Object);
            
            ServiceLocator.Initialize(mockProvider.Object);

            // Act
            var result = ServiceLocator.GetRequiredService<IMessageBoxService>();

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
            Assert.Throws<InvalidOperationException>(() => ServiceLocator.GetRequiredService<IMessageBoxService>());
        }
    }
}
