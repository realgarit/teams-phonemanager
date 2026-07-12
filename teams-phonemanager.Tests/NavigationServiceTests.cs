using System.ComponentModel;
using Moq;
using teams_phonemanager.Services;
using teams_phonemanager.Services.Interfaces;

namespace teams_phonemanager.Tests
{
    public class NavigationServiceTests
    {
        [Fact]
        public void CurrentPage_DefaultsToWelcomePage()
        {
            var logger = new Mock<ILoggingService>();

            var service = new NavigationService(logger.Object);

            Assert.Equal(ConstantsService.Pages.Welcome, service.CurrentPage);
        }

        [Fact]
        public void Constructor_LogsInitialization()
        {
            var logger = new Mock<ILoggingService>();

            _ = new NavigationService(logger.Object);

            logger.Verify(l => l.Log("Navigation service initialized", LogLevel.Info), Times.Once);
        }

        [Fact]
        public void NavigateTo_UpdatesCurrentPage()
        {
            var logger = new Mock<ILoggingService>();
            var service = new NavigationService(logger.Object);

            service.NavigateTo(ConstantsService.Pages.Variables);

            Assert.Equal(ConstantsService.Pages.Variables, service.CurrentPage);
        }

        [Fact]
        public void NavigateTo_LogsNavigation()
        {
            var logger = new Mock<ILoggingService>();
            var service = new NavigationService(logger.Object);

            service.NavigateTo(ConstantsService.Pages.Variables);

            logger.Verify(l => l.Log($"Navigated to {ConstantsService.Pages.Variables} page", LogLevel.Info), Times.Once);
        }

        [Fact]
        public void NavigateTo_SamePageTwice_DoesNotRaisePropertyChangedTwice()
        {
            var logger = new Mock<ILoggingService>();
            var service = new NavigationService(logger.Object);
            service.NavigateTo(ConstantsService.Pages.Variables);

            var raiseCount = 0;
            service.PropertyChanged += (_, _) => raiseCount++;

            service.NavigateTo(ConstantsService.Pages.Variables);

            Assert.Equal(0, raiseCount);
        }

        [Fact]
        public void NavigateTo_SamePageTwice_DoesNotLogNavigationTwice()
        {
            var logger = new Mock<ILoggingService>();
            var service = new NavigationService(logger.Object);
            service.NavigateTo(ConstantsService.Pages.Variables);
            logger.Invocations.Clear();

            service.NavigateTo(ConstantsService.Pages.Variables);

            logger.Verify(l => l.Log(It.Is<string>(s => s.StartsWith("Navigated to")), It.IsAny<LogLevel>()), Times.Never);
        }

        [Fact]
        public void NavigateTo_DifferentPage_RaisesPropertyChangedOnce()
        {
            var logger = new Mock<ILoggingService>();
            var service = new NavigationService(logger.Object);

            var raisedProperties = new List<string?>();
            service.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName);

            service.NavigateTo(ConstantsService.Pages.CallQueues);

            Assert.Single(raisedProperties);
            Assert.Equal(nameof(NavigationService.CurrentPage), raisedProperties[0]);
        }
    }
}
