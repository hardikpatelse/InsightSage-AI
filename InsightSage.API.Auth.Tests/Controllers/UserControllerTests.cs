using InsightSage.API.Auth.Controllers;
using InsightSage.Shared.Interfaces.Services;
using InsightSage.Shared.Models.Entities;
using InsightSage.Shared.Models.Responses;
using System.Net;

namespace InsightSage.Tests.Controllers
{
    /// <summary>
    /// Unit tests for UserController - tests the current implementation behavior
    /// Note: Some tests document existing bugs in the controller that should be fixed
    /// </summary>
    public class UserControllerTests
    {
        private readonly Mock<IUserService<UserResponse<List<User>>, UserResponse<User>, User>> _mockUserService;
        private readonly UserController _controller;
        private readonly Fixture _fixture;

        public UserControllerTests()
        {
            _mockUserService = new Mock<IUserService<UserResponse<List<User>>, UserResponse<User>, User>>();
            _controller = new UserController(_mockUserService.Object);
            _fixture = new Fixture();
        }

        #region GetUser Tests

        [Fact]
        public async Task GetUser_WhenServiceReturnsSuccess_ShouldReturnSuccessResponse()
        {
            // Arrange
            var expectedUser = _fixture.Build<User>()
                .With(u => u.Id, 1)
                .With(u => u.Email, "test@example.com")
                .With(u => u.Name, "Test User")
                .Create();

            var serviceResponse = new UserResponse<User>
            {
                Result = expectedUser,
                Errors = new List<string>()
            };

            _mockUserService.Setup(x => x.GetCurrentUser())
                .ReturnsAsync(serviceResponse);

            // Act
            var result = await _controller.GetUser();

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeEquivalentTo(expectedUser);
            result.Status.Should().Be(HttpStatusCode.OK);
            result.Errors.Should().BeEmpty();

            _mockUserService.Verify(x => x.GetCurrentUser(), Times.Once);
        }

        [Fact]
        public async Task GetUser_WhenServiceReturnsUserWithErrors_ShouldReturnResponseWithErrors()
        {
            // Arrange
            var serviceResponse = new UserResponse<User>
            {
                Result = null,
                Errors = new List<string> { "User not found" }
            };

            _mockUserService.Setup(x => x.GetCurrentUser())
                .ReturnsAsync(serviceResponse);

            // Act
            var result = await _controller.GetUser();

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeNull();
            result.Status.Should().Be(HttpStatusCode.OK);
            result.Errors.Should().HaveCount(1);
            result.Errors.First().Should().Be("User not found");

            _mockUserService.Verify(x => x.GetCurrentUser(), Times.Once);
        }

        [Fact]
        public async Task GetUser_WhenServiceThrowsException_ShouldReturnInternalServerError()
        {
            // Arrange
            var exceptionMessage = "Database connection failed";
            var exception = new Exception(exceptionMessage);

            _mockUserService.Setup(x => x.GetCurrentUser())
                .ThrowsAsync(exception);

            // Act
            var result = await _controller.GetUser();

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeNull();
            result.Status.Should().Be(HttpStatusCode.InternalServerError);
            result.Errors.Should().HaveCount(1);
            result.Errors.First().Should().Be(exceptionMessage);
            result.ExceptionDetails.Should().NotBeNull();
            result.ExceptionDetails.Should().Contain("Database connection failed");

            _mockUserService.Verify(x => x.GetCurrentUser(), Times.Once);
        }

        [Fact]
        public async Task GetUser_WhenServiceThrowsNestedExceptionWithInnerException_ShouldReturnBaseExceptionMessage()
        {
            // Arrange
            var innerException = new InvalidOperationException("Inner exception message");
            var outerException = new Exception("Outer exception message", innerException);

            _mockUserService.Setup(x => x.GetCurrentUser())
                .ThrowsAsync(outerException);

            // Act
            var result = await _controller.GetUser();

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(HttpStatusCode.InternalServerError);
            result.Errors.Should().HaveCount(1);
            result.Errors.First().Should().Be("Inner exception message");
            result.ExceptionDetails.Should().NotBeNull();

            _mockUserService.Verify(x => x.GetCurrentUser(), Times.Once);
        }

        #endregion

        #region Login Tests - Positive Scenarios

        [Fact]
        public async Task Login_WhenValidUserProvided_ShouldReturnSuccessResponse()
        {
            // Arrange
            var inputUser = _fixture.Build<User>()
                .With(u => u.Email, "test@example.com")
                .With(u => u.Name, "Test User")
                .Create();

            var expectedUser = _fixture.Build<User>()
                .With(u => u.Id, 1)
                .With(u => u.Email, "test@example.com")
                .With(u => u.Name, "Test User")
                .Create();

            var serviceResponse = new UserResponse<User>
            {
                Result = expectedUser,
                Errors = new List<string>()
            };

            _mockUserService.Setup(x => x.LoginAsync(It.IsAny<User>()))
                .ReturnsAsync(serviceResponse);

            // Act
            var result = await _controller.Login(inputUser);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeEquivalentTo(expectedUser);
            result.Status.Should().Be(HttpStatusCode.OK);
            result.Errors.Should().BeEmpty();

            _mockUserService.Verify(x => x.LoginAsync(It.Is<User>(u => u.Email == inputUser.Email)), Times.Once);
        }

        [Fact]
        public async Task Login_WhenServiceReturnsNewUser_ShouldReturnSuccessResponse()
        {
            // Arrange
            var inputUser = _fixture.Build<User>()
                .With(u => u.Email, "newuser@example.com")
                .With(u => u.Name, "New User")
                .Create();

            var createdUser = _fixture.Build<User>()
                .With(u => u.Id, 1)
                .With(u => u.Email, "newuser@example.com")
                .With(u => u.Name, "New User")
                .Create();

            var serviceResponse = new UserResponse<User>
            {
                Result = createdUser,
                Errors = new List<string>()
            };

            _mockUserService.Setup(x => x.LoginAsync(It.IsAny<User>()))
                .ReturnsAsync(serviceResponse);

            // Act
            var result = await _controller.Login(inputUser);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeEquivalentTo(createdUser);
            result.Status.Should().Be(HttpStatusCode.OK);
            result.Errors.Should().BeEmpty();

            _mockUserService.Verify(x => x.LoginAsync(It.IsAny<User>()), Times.Once);
        }

        #endregion

        #region Login Tests - Current Controller Behavior (Documents Existing Issues)

        [Fact]
        public async Task Login_WhenUserIsNull_ShouldCatchNullReferenceExceptionAndReturnInternalServerError()
        {
            // Note: This test documents the current behavior of the controller
            // The controller checks for null user but doesn't return early, then tries to access user.Email
            // This causes a NullReferenceException which is caught and handled

            // Arrange
            User? nullUser = null;

            // Act
            var result = await _controller.Login(nullUser!);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(HttpStatusCode.InternalServerError);
            result.Errors.Should().HaveCount(2);
            result.Errors.Should().Contain("User data is required");
            result.Errors.Should().Contain("Object reference not set to an instance of an object.");

            _mockUserService.Verify(x => x.LoginAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Login_WhenEmailIsNullButUserIsValid_ShouldStillCallServiceDespiteValidationError()
        {
            // Note: This test documents the current problematic behavior
            // BUG: Controller validation doesn't prevent service call when validation fails
            // RECOMMENDATION: Return early when validation fails

            // Arrange
            var userWithoutEmail = _fixture.Build<User>()
                .With(u => u.Email, (string?)null)
                .With(u => u.Name, "Test User")
                .Create();

            var serviceResponse = new UserResponse<User>
            {
                Result = null,
                Errors = new List<string> { "Email is required for login." }
            };

            _mockUserService.Setup(x => x.LoginAsync(It.IsAny<User>()))
                .ReturnsAsync(serviceResponse);

            // Act
            var result = await _controller.Login(userWithoutEmail);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(HttpStatusCode.OK); // BUG: Should be BadRequest
            result.Errors.Should().Contain("Email is required for login.");

            // BUG: Service should not be called when validation fails
            _mockUserService.Verify(x => x.LoginAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task Login_WhenEmailIsEmpty_ShouldStillCallServiceDespiteValidationError()
        {
            // Note: This test documents the current problematic behavior
            // BUG: Controller validation doesn't prevent service call when validation fails

            // Arrange
            var userWithEmptyEmail = _fixture.Build<User>()
                .With(u => u.Email, string.Empty)
                .With(u => u.Name, "Test User")
                .Create();

            var serviceResponse = new UserResponse<User>
            {
                Result = null,
                Errors = new List<string> { "Email is required for login." }
            };

            _mockUserService.Setup(x => x.LoginAsync(It.IsAny<User>()))
                .ReturnsAsync(serviceResponse);

            // Act
            var result = await _controller.Login(userWithEmptyEmail);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(HttpStatusCode.OK); // BUG: Should be BadRequest
            result.Errors.Should().Contain("Email is required for login.");

            // BUG: Service should not be called when validation fails
            _mockUserService.Verify(x => x.LoginAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task Login_WhenServiceReturnsErrors_ShouldOverrideStatusToOK()
        {
            // Note: This test documents the current buggy behavior
            // BUG: Status is always set to OK at the end, overriding InternalServerError
            // RECOMMENDATION: Don't override status when there are errors

            // Arrange
            var inputUser = _fixture.Build<User>()
                .With(u => u.Email, "test@example.com")
                .Create();

            var serviceResponse = new UserResponse<User>
            {
                Result = null,
                Errors = new List<string> { "Database error", "Validation failed" }
            };

            _mockUserService.Setup(x => x.LoginAsync(It.IsAny<User>()))
                .ReturnsAsync(serviceResponse);

            // Act
            var result = await _controller.Login(inputUser);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeNull();
            result.Status.Should().Be(HttpStatusCode.OK); // BUG: Should remain InternalServerError
            result.Errors.Should().HaveCount(2);
            result.Errors.Should().Contain("Database error");
            result.Errors.Should().Contain("Validation failed");

            _mockUserService.Verify(x => x.LoginAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task Login_WhenServiceThrowsException_ShouldReturnInternalServerError()
        {
            // This case works correctly - exception handling is proper

            // Arrange
            var inputUser = _fixture.Build<User>()
                .With(u => u.Email, "test@example.com")
                .Create();

            var exceptionMessage = "Unexpected database error";
            var exception = new Exception(exceptionMessage);

            _mockUserService.Setup(x => x.LoginAsync(It.IsAny<User>()))
                .ThrowsAsync(exception);

            // Act
            var result = await _controller.Login(inputUser);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeNull();
            result.Status.Should().Be(HttpStatusCode.InternalServerError);
            result.Errors.Should().HaveCount(1);
            result.Errors.First().Should().Be(exceptionMessage);
            result.ExceptionDetails.Should().NotBeNull();
            result.ExceptionDetails.Should().Contain("Unexpected database error");

            _mockUserService.Verify(x => x.LoginAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task Login_WhenValidUserWithCompleteData_ShouldPassAllDataToService()
        {
            // This case works correctly

            // Arrange
            var inputUser = new User
            {
                Id = 0,
                Email = "test@example.com",
                Name = "Test User",
                UserId = "user123",
                TenantId = "tenant456"
            };

            var serviceResponse = new UserResponse<User>
            {
                Result = inputUser,
                Errors = new List<string>()
            };

            _mockUserService.Setup(x => x.LoginAsync(It.IsAny<User>()))
                .ReturnsAsync(serviceResponse);

            // Act
            var result = await _controller.Login(inputUser);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(HttpStatusCode.OK);

            _mockUserService.Verify(x => x.LoginAsync(It.Is<User>(u =>
                u.Email == inputUser.Email &&
                u.Name == inputUser.Name &&
                u.UserId == inputUser.UserId &&
                u.TenantId == inputUser.TenantId)), Times.Once);
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WhenUserServiceIsNull_ShouldNotThrowException()
        {
            // Note: The current controller doesn't validate constructor parameters
            // This is not necessarily a bug, as dependency injection typically handles this

            // Arrange & Act & Assert
            var controller = new UserController(null!);
            controller.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WhenValidUserServiceProvided_ShouldCreateInstance()
        {
            // Arrange
            var mockUserService = new Mock<IUserService<UserResponse<List<User>>, UserResponse<User>, User>>();

            // Act
            var controller = new UserController(mockUserService.Object);

            // Assert
            controller.Should().NotBeNull();
        }

        #endregion
    }
}