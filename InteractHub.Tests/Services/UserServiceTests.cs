using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using InteractHub.Api.Dtos.Users;
using InteractHub.Api.Entities;
using InteractHub.Api.Repositories;
using InteractHub.Api.Services;
using Moq;
using Xunit;

namespace InteractHub.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IPostRepository> _postRepositoryMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            // Khởi tạo các Mock object
            _userRepositoryMock = new Mock<IUserRepository>();
            _postRepositoryMock = new Mock<IPostRepository>();

            // Inject các mock dependencies vào UserService
            _userService = new UserService(_userRepositoryMock.Object, _postRepositoryMock.Object);
        }

        #region GetUserProfileAsync Tests

        [Fact]
        public async Task GetUserProfileAsync_UserExists_ReturnsUserProfileObject()
        {
            // Arrange
            var userId = "user-123";
            var mockUser = new ApplicationUser
            {
                Id = userId,
                DisplayName = "John Doe",
                Email = "john@example.com"
            };
            var expectedPostCount = 5;

            // Setup Mock trả về user và số lượng post
            _userRepositoryMock.Setup(repo => repo.FindUserAsync(userId)).ReturnsAsync(mockUser);
            _postRepositoryMock.Setup(repo => repo.CountUserAsync(userId)).ReturnsAsync(expectedPostCount);

            // Act
            var result = await _userService.GetUserProfileAsync(userId);

            // Assert
            Assert.NotNull(result);

            // Vì hàm trả về Anonymous Object (new { Id, DisplayName, Email, totalPosts }), 
            // chúng ta dùng Reflection để kiểm tra giá trị các thuộc tính
            Type type = result.GetType();
            Assert.Equal(userId, type.GetProperty("Id")?.GetValue(result));
            Assert.Equal("John Doe", type.GetProperty("DisplayName")?.GetValue(result));
            Assert.Equal("john@example.com", type.GetProperty("Email")?.GetValue(result));
            Assert.Equal(expectedPostCount, type.GetProperty("totalPosts")?.GetValue(result));
        }

        [Fact]
        public async Task GetUserProfileAsync_UserDoesNotExist_ReturnsNull()
        {
            // Arrange
            var userId = "non-existent-user";

            // Setup Mock trả về null (không tìm thấy user)
            _userRepositoryMock.Setup(repo => repo.FindUserAsync(userId)).ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _userService.GetUserProfileAsync(userId);

            // Assert
            Assert.Null(result);
            // Đảm bảo CountUserAsync không bao giờ được gọi đến nếu user không tồn tại
            _postRepositoryMock.Verify(repo => repo.CountUserAsync(It.IsAny<string>()), Times.Never);
        }

        #endregion

        #region SearchUsersAsync Tests

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task SearchUsersAsync_SearchTermIsNullOrWhiteSpace_ReturnsEmptyEnumerable(string? searchTerm)
        {
            // Act
            var result = await _userService.SearchUsersAsync(searchTerm);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            // Đảm bảo không gọi xuống repository khi chuỗi rỗng
            _userRepositoryMock.Verify(repo => repo.SearchUsersAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SearchUsersAsync_ValidSearchTerm_ConvertsToLowercaseAndCallsRepo()
        {
            // Arrange
            var searchTerm = "JOhN";
            var expectedSearchTerm = "john"; // Hàm service sẽ chuyển sang chữ thường
            var expectedUsers = new List<UserDtos> { new UserDtos { Id = "1", Email = "john@example.com" } };

            _userRepositoryMock.Setup(repo => repo.SearchUsersAsync(expectedSearchTerm)).ReturnsAsync(expectedUsers);

            // Act
            var result = await _userService.SearchUsersAsync(searchTerm);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            // Xác nhận repository thực sự được gọi với tham số chữ thường
            _userRepositoryMock.Verify(repo => repo.SearchUsersAsync(expectedSearchTerm), Times.Once);
        }

        #endregion

        #region GetAllUsersAsync Tests

        [Fact]
        public async Task GetAllUsersAsync_ReturnsAllUsers()
        {
            // Arrange
            var expectedUsers = new List<UserDtos>
            {
                new UserDtos { Id = "1", Email = "user1@test.com" },
                new UserDtos { Id = "2", Email = "user2@test.com" }
            };

            _userRepositoryMock.Setup(repo => repo.GetAllUsersAsync()).ReturnsAsync(expectedUsers);

            // Act
            var result = await _userService.GetAllUsersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _userRepositoryMock.Verify(repo => repo.GetAllUsersAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllUsersAsync_NoUsersFound_ReturnsEmptyList()
        {
            // Arrange
            _userRepositoryMock.Setup(repo => repo.GetAllUsersAsync()).ReturnsAsync(new List<UserDtos>());

            // Act
            var result = await _userService.GetAllUsersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region DeleteUserAsync Tests

        [Fact]
        public async Task DeleteUserAsync_Success_ReturnsTrue()
        {
            // Arrange
            var userId = "user-123";
            _userRepositoryMock.Setup(repo => repo.DeleteUserAsync(userId)).ReturnsAsync(true);

            // Act
            var result = await _userService.DeleteUserAsync(userId);

            // Assert
            Assert.True(result);
            _userRepositoryMock.Verify(repo => repo.DeleteUserAsync(userId), Times.Once);
        }

        [Fact]
        public async Task DeleteUserAsync_FailsOrNotFound_ReturnsFalse()
        {
            // Arrange
            var userId = "non-existent-user";
            _userRepositoryMock.Setup(repo => repo.DeleteUserAsync(userId)).ReturnsAsync(false);

            // Act
            var result = await _userService.DeleteUserAsync(userId);

            // Assert
            Assert.False(result);
            _userRepositoryMock.Verify(repo => repo.DeleteUserAsync(userId), Times.Once);
        }

        #endregion
    }
}