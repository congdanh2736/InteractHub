using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InteractHub.Api.Dtos.Posts;
using InteractHub.Api.Entities;
using InteractHub.Api.Hubs;
using InteractHub.Api.Repositories;
using InteractHub.Api.Services;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Xunit;

namespace InteractHub.Tests.Services
{
    public class PostServiceTests
    {
        private readonly Mock<IPostRepository> _postRepositoryMock;
        private readonly Mock<IHubContext<NotificationHub>> _hubContextMock;
        private readonly Mock<IHashtagService> _hashtagServiceMock;
        private readonly PostService _postService;

        public PostServiceTests()
        {
            _postRepositoryMock = new Mock<IPostRepository>();
            _hashtagServiceMock = new Mock<IHashtagService>();

            // Setup Mock cho SignalR (IHubContext -> Clients -> All -> SendAsync)
            _hubContextMock = new Mock<IHubContext<NotificationHub>>();
            var mockClients = new Mock<IHubClients>();
            var mockClientProxy = new Mock<IClientProxy>();

            mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);
            _hubContextMock.Setup(hub => hub.Clients).Returns(mockClients.Object);

            _postService = new PostService(
                _postRepositoryMock.Object,
                _hubContextMock.Object,
                _hashtagServiceMock.Object);
        }

        #region GetAllPostAsync Tests

        [Fact]
        public async Task GetAllPostAsync_ReturnsPaginatedResult()
        {
            // Arrange
            var keyword = "test";
            var pageNumber = 1;
            var pageSize = 10;
            var currentUserId = "user1";
            var expectedTotalItems = 15;

            var expectedPosts = new List<PostDetailResponse>
            {
                new PostDetailResponse { Id = 1, Content = "Test post 1" }
            };

            _postRepositoryMock.Setup(repo => repo.CountAsync(keyword))
                .ReturnsAsync(expectedTotalItems);

            _postRepositoryMock.Setup(repo => repo.GetMoreDetailPostsByUserIdAsync(keyword, pageNumber, pageSize, currentUserId))
                .ReturnsAsync(expectedPosts);

            // Act
            var result = await _postService.GetAllPostAsync(keyword, pageNumber, pageSize, currentUserId);

            // Assert
            Assert.NotNull(result);
            var type = result.GetType();
            Assert.Equal(expectedTotalItems, type.GetProperty("totalItems")?.GetValue(result));
            Assert.Equal(2, type.GetProperty("totalPages")?.GetValue(result)); // 15 items / 10 size = 2 pages
            Assert.Equal(expectedPosts, type.GetProperty("items")?.GetValue(result));
        }

        #endregion

        #region GetPostByIdAsync Tests

        [Fact]
        public async Task GetPostByIdAsync_ReturnsPostDetailResponse()
        {
            // Arrange
            var postId = 1;
            var currentUserId = "user1";
            var expectedPost = new PostDetailResponse { Id = postId, Content = "Content" };

            _postRepositoryMock.Setup(repo => repo.GetPostsByUserIdAndIdAsync(postId, currentUserId))
                .ReturnsAsync(expectedPost);

            // Act
            var result = await _postService.GetPostByIdAsync(postId, currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(postId, result.Id);
        }

        #endregion

        #region CreatePostAsync Tests

        [Fact]
        public async Task CreatePostAsync_ValidRequest_CreatesPostAndSendsNotification()
        {
            // Arrange
            var userId = "user1";
            var request = new CreatePostRequest
            {
                Content = "  Hello World  ", // Test whitespace trim
                ImageUrl = "http://image.url"
            };

            _postRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Post>()))
                .ReturnsAsync((Post p) => { p.Id = 99; return p; });

            // Act
            var result = await _postService.CreatePostAsync(request, userId);

            // Assert
            Assert.NotNull(result);

            // 1. Kiểm tra Repository đã được gọi để Add
            _postRepositoryMock.Verify(repo => repo.AddAsync(It.Is<Post>(p =>
                p.Content == "Hello World" &&
                p.UserId == userId &&
                p.ImageUrl == request.ImageUrl
            )), Times.Once);

            // 2. Kiểm tra Hashtag Service được gọi
            _hashtagServiceMock.Verify(service => service.ProcessHashtagsAsync(request.Content), Times.Once);

            // 3. Kiểm tra kết quả trả về đúng properties
            var type = result.GetType();
            Assert.Equal("Hello World", type.GetProperty("Content")?.GetValue(result));
            Assert.Equal(userId, type.GetProperty("UserId")?.GetValue(result));
        }

        #endregion

        #region UpdatePostAsync Tests

        [Fact]
        public async Task UpdatePostAsync_PostNotFound_ReturnsNull()
        {
            // Arrange
            var postId = 1;
            _postRepositoryMock.Setup(repo => repo.GetByIdAsync(postId)).ReturnsAsync((Post?)null);

            // Act
            var result = await _postService.UpdatePostAsync(postId, new UpdatePostRequest(), "user1");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdatePostAsync_UserNotOwner_ReturnsNull()
        {
            // Arrange
            var postId = 1;
            var post = new Post { Id = postId, UserId = "ownerId" }; // Người sở hữu là ownerId
            _postRepositoryMock.Setup(repo => repo.GetByIdAsync(postId)).ReturnsAsync(post);

            // Act
            var result = await _postService.UpdatePostAsync(postId, new UpdatePostRequest(), "differentUser");

            // Assert
            Assert.Null(result);
            _postRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Post>()), Times.Never); // Không được update
        }

        [Fact]
        public async Task UpdatePostAsync_PostExistsAndUserIsOwner_UpdatesPostSuccessfully()
        {
            // Arrange
            var postId = 1;
            var userId = "user1";
            var existingPost = new Post { Id = postId, UserId = userId, Content = "Old Content" };
            var updateRequest = new UpdatePostRequest { Content = "  New Content  ", ImageUrl = "new.jpg" };

            _postRepositoryMock.Setup(repo => repo.GetByIdAsync(postId)).ReturnsAsync(existingPost);

            // Act
            var result = await _postService.UpdatePostAsync(postId, updateRequest, userId);

            // Assert
            Assert.NotNull(result);
            _postRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<Post>(p =>
                p.Content == "New Content" &&
                p.ImageUrl == "new.jpg")), Times.Once);
        }

        #endregion

        #region DeletePostAsync Tests

        [Fact]
        public async Task DeletePostAsync_PostNotFound_ReturnsFalse()
        {
            // Arrange
            var postId = 1;
            _postRepositoryMock.Setup(repo => repo.GetByIdAsync(postId)).ReturnsAsync((Post?)null);

            // Act
            var result = await _postService.DeletePostAsync(postId, "user1");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeletePostAsync_UserNotOwnerAndNotAdmin_ReturnsFalse()
        {
            // Arrange
            var postId = 1;
            var existingPost = new Post { Id = postId, UserId = "ownerId" };
            _postRepositoryMock.Setup(repo => repo.GetByIdAsync(postId)).ReturnsAsync(existingPost);

            // Act
            var result = await _postService.DeletePostAsync(postId, "hacker", isAdmin: false);

            // Assert
            Assert.False(result);
            _postRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task DeletePostAsync_UserIsOwner_DeletesPostAndReturnsTrue()
        {
            // Arrange
            var postId = 1;
            var userId = "ownerId";
            var existingPost = new Post { Id = postId, UserId = userId };
            _postRepositoryMock.Setup(repo => repo.GetByIdAsync(postId)).ReturnsAsync(existingPost);

            // Act
            var result = await _postService.DeletePostAsync(postId, userId, isAdmin: false);

            // Assert
            Assert.True(result);
            _postRepositoryMock.Verify(repo => repo.DeleteAsync(postId), Times.Once);
        }

        [Fact]
        public async Task DeletePostAsync_UserIsAdminButNotOwner_DeletesPostAndReturnsTrue()
        {
            // Arrange
            var postId = 1;
            var existingPost = new Post { Id = postId, UserId = "ownerId" };
            _postRepositoryMock.Setup(repo => repo.GetByIdAsync(postId)).ReturnsAsync(existingPost);

            // Act
            var result = await _postService.DeletePostAsync(postId, "adminId", isAdmin: true);

            // Assert
            Assert.True(result);
            _postRepositoryMock.Verify(repo => repo.DeleteAsync(postId), Times.Once);
        }

        #endregion
    }
}