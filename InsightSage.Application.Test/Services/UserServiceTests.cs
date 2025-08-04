using InsightSage.Application.Services;
using InsightSage.Shared.Interfaces.DataContexts;
using InsightSage.Shared.Interfaces.Others;
using InsightSage.Shared.Interfaces.Services;
using InsightSage.Shared.Models.Entities;
using InsightSage.Shared.Models.Responses;
using System.Net;

namespace InsightSage.Application.Test.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserContext> _mockUserContext;
        private readonly Mock<IUserDataContext> _mockUserDataContext;
        private readonly IUserService<UserResponse<List<User>>, UserResponse<User>, User> _userService;
        private readonly Fixture _fixture;

        public UserServiceTests()
        {
            _mockUserContext = new Mock<IUserContext>();
            _mockUserDataContext = new Mock<IUserDataContext>();
            _userService = new UserService(_mockUserContext.Object, _mockUserDataContext.Object);
            _fixture = new Fixture();
        }

        [Fact]
        public async Task AddUpdateAsync_WhenAddingNewUser_ShouldReturnSuccessResponse()
        {
            // Arrange
            var user = _fixture.Build<User>()
                .With(u => u.Id, 0)
                .With(u => u.Email, "test@example.com")
                .Create();

            var expectedId = 1;
            _mockUserDataContext.Setup(x => x.AddAsync(It.IsAny<User>()))
                .ReturnsAsync(expectedId);

            // Act
            var result = await _userService.AddUpdateAsync(user);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(expectedId);
            result.Status.Should().Be(HttpStatusCode.OK);
            result.Errors.Should().BeEmpty();

            _mockUserDataContext.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
            _mockUserDataContext.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task AddUpdateAsync_WhenExceptionOccurs_ShouldReturnErrorResponse()
        {
            // Arrange
            var user = _fixture.Build<User>()
                .With(u => u.Id, 0)
                .Create();

            var exceptionMessage = "Database error";
            _mockUserDataContext.Setup(x => x.AddAsync(It.IsAny<User>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _userService.AddUpdateAsync(user);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(0);
            result.Status.Should().Be(HttpStatusCode.InternalServerError);
            result.Errors.Should().HaveCount(1);
            result.Errors.First().Should().Be(exceptionMessage);
        }

        [Fact]
        public async Task GetCurrentUser_WhenUserContextHasData_ShouldReturnCurrentUser()
        {
            // Arrange
            var expectedUserId = "user-123";
            var expectedEmail = "test@example.com";
            var expectedName = "Test User";
            var expectedTenantId = "tenant-123";

            _mockUserContext.Setup(x => x.UserId).Returns(expectedUserId);
            _mockUserContext.Setup(x => x.Email).Returns(expectedEmail);
            _mockUserContext.Setup(x => x.Name).Returns(expectedName);
            _mockUserContext.Setup(x => x.TenantId).Returns(expectedTenantId);

            // Act
            var result = await _userService.GetCurrentUser();

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().NotBeNull();
            result.Result.UserId.Should().Be(expectedUserId);
            result.Result.Email.Should().Be(expectedEmail);
            result.Result.Name.Should().Be(expectedName);
            result.Result.TenantId.Should().Be(expectedTenantId);
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public async Task LoginAsync_WhenEmailIsNull_ShouldReturnValidationError()
        {
            // Arrange
            var user = _fixture.Build<User>()
                .With(u => u.Email, (string?)null)
                .Create();

            // Act
            var result = await _userService.LoginAsync(user);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeNull();
            result.Errors.Should().HaveCount(1);
            result.Errors.First().Should().Be("Email is required for login.");

            _mockUserDataContext.Verify(x => x.GetByEmailAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_WhenUserExists_ShouldUpdateAndReturnExistingUser()
        {
            // Arrange
            var email = "test@example.com";
            var inputUser = _fixture.Build<User>()
                .With(u => u.Email, email)
                .Create();

            var existingUser = _fixture.Build<User>()
                .With(u => u.Id, 1)
                .With(u => u.Email, email)
                .Create();

            _mockUserDataContext.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(existingUser);

            _mockUserDataContext.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(existingUser.Id);

            // Act
            var result = await _userService.LoginAsync(inputUser);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().NotBeNull();
            result.Result.Should().BeEquivalentTo(existingUser);
            result.Errors.Should().BeEmpty();

            _mockUserDataContext.Verify(x => x.GetByEmailAsync(email), Times.Once);
            _mockUserDataContext.Verify(x => x.UpdateAsync(It.Is<User>(u => u.Email == email)), Times.Once);
            _mockUserDataContext.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_WhenUserDoesNotExist_ShouldCreateNewUser()
        {
            // Arrange
            var email = "newuser@example.com";
            var inputUser = _fixture.Build<User>()
                .With(u => u.Email, email)
                .With(u => u.Id, 5)
                .Create();

            _mockUserDataContext.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync((User?)null);

            var newUserId = 1;
            _mockUserDataContext.Setup(x => x.AddAsync(It.IsAny<User>()))
                .ReturnsAsync(newUserId)
                .Callback<User>(u => u.Id = newUserId);

            // Act
            var result = await _userService.LoginAsync(inputUser);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().NotBeNull();
            result.Result.Email.Should().Be(email);
            result.Result.UserId.Should().Be(inputUser.UserId);
            result.Result.Name.Should().Be(inputUser.Name);
            result.Result.TenantId.Should().Be(inputUser.TenantId);
            result.Errors.Should().BeEmpty();

            _mockUserDataContext.Verify(x => x.GetByEmailAsync(email), Times.Once);
            _mockUserDataContext.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
            _mockUserDataContext.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_WhenUniqueConstraintViolationOccurs_ShouldReturnGracefulError()
        {
            // Arrange
            var email = "test@example.com";
            var inputUser = _fixture.Build<User>()
                .With(u => u.Email, email)
                .Create();

            _mockUserDataContext.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync((User?)null);

            _mockUserDataContext.Setup(x => x.AddAsync(It.IsAny<User>()))
                .ThrowsAsync(new InvalidOperationException("A user with email 'test@example.com' already exists."));

            // Act
            var result = await _userService.LoginAsync(inputUser);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeNull();
            result.Errors.Should().HaveCount(1);
            result.Errors.First().Should().Be("A user with this email already exists. Please try again.");
        }

        [Fact]
        public async Task LoginAsync_WhenGeneralExceptionOccurs_ShouldReturnErrorResponse()
        {
            // Arrange
            var email = "test@example.com";
            var inputUser = _fixture.Build<User>()
                .With(u => u.Email, email)
                .Create();

            var exceptionMessage = "Database connection failed";
            _mockUserDataContext.Setup(x => x.GetByEmailAsync(email))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _userService.LoginAsync(inputUser);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeNull();
            result.Errors.Should().HaveCount(1);
            result.Errors.First().Should().Be(exceptionMessage);
        }
    }
}