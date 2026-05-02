using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InteractHub.Api.Dtos.Friends;
using InteractHub.Api.Entities;
using InteractHub.Api.Hubs;
using InteractHub.Api.Repositories;
using InteractHub.Api.Services;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Xunit;

namespace InteractHub.Tests.Services
{
    public class FriendServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IFriendshipRepository> _friendshipRepositoryMock;
        private readonly Mock<INotificationRepository> _notificationRepositoryMock;
        private readonly Mock<IHubContext<NotificationHub>> _hubContextMock;
        private readonly FriendService _friendService;

        public FriendServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _friendshipRepositoryMock = new Mock<IFriendshipRepository>();
            _notificationRepositoryMock = new Mock<INotificationRepository>();

            // Cấu hình Mock cho SignalR: _hubContext.Clients.User(receiverId).SendAsync(...)
            _hubContextMock = new Mock<IHubContext<NotificationHub>>();
            var mockClients = new Mock<IHubClients>();
            var mockClientProxy = new Mock<IClientProxy>();

            mockClients.Setup(clients => clients.User(It.IsAny<string>())).Returns(mockClientProxy.Object);
            _hubContextMock.Setup(hub => hub.Clients).Returns(mockClients.Object);

            _friendService = new FriendService(
                _userRepositoryMock.Object,
                _friendshipRepositoryMock.Object,
                _notificationRepositoryMock.Object,
                _hubContextMock.Object);
        }

        #region SendFriendRequestAsync Tests

        [Fact]
        public async Task SendFriendRequestAsync_SameUser_ReturnsNull()
        {
            // Act
            var result = await _friendService.SendFriendRequestAsync("user1", "user1");

            // Assert
            Assert.Null(result);
            _userRepositoryMock.Verify(repo => repo.FindUserAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SendFriendRequestAsync_ReceiverDoesNotExist_ReturnsNull()
        {
            // Arrange
            _userRepositoryMock.Setup(repo => repo.FindUserAsync("receiverId")).ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _friendService.SendFriendRequestAsync("requesterId", "receiverId");

            // Assert
            Assert.Null(result);
            _friendshipRepositoryMock.Verify(repo => repo.CheckFriendshipAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SendFriendRequestAsync_FriendshipAlreadyExists_ReturnsMessage()
        {
            // Arrange
            _userRepositoryMock.Setup(repo => repo.FindUserAsync("receiverId")).ReturnsAsync(new ApplicationUser());
            _friendshipRepositoryMock.Setup(repo => repo.CheckFriendshipAsync("receiverId", "requesterId"))
                .ReturnsAsync(new Friendship()); // Trả về đã tồn tại

            // Act
            var result = await _friendService.SendFriendRequestAsync("requesterId", "receiverId");

            // Assert
            Assert.NotNull(result);
            var type = result.GetType();
            Assert.Equal("Friendship or request already exists.", type.GetProperty("message")?.GetValue(result));

            // Đảm bảo không add mới
            _friendshipRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Friendship>()), Times.Never);
        }

        [Fact]
        public async Task SendFriendRequestAsync_ValidRequest_CreatesFriendshipAndNotificationAndReturnsMessage()
        {
            // Arrange
            var requesterId = "requester";
            var receiverId = "receiver";
            var requesterUser = new ApplicationUser { Id = requesterId, DisplayName = "Alice" };

            // Người nhận tồn tại
            _userRepositoryMock.SetupSequence(repo => repo.FindUserAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApplicationUser { Id = receiverId }) // Lần 1 gọi cho receiver
                .ReturnsAsync(requesterUser); // Lần 2 gọi lấy thông tin requester

            _friendshipRepositoryMock.Setup(repo => repo.CheckFriendshipAsync(receiverId, requesterId)).ReturnsAsync((Friendship?)null);

            // Act
            var result = await _friendService.SendFriendRequestAsync(requesterId, receiverId);

            // Assert
            Assert.NotNull(result);
            var type = result.GetType();
            Assert.Equal("Friend request sent successfully.", type.GetProperty("message")?.GetValue(result));

            _friendshipRepositoryMock.Verify(repo => repo.AddAsync(It.Is<Friendship>(f =>
                f.RequesterId == requesterId &&
                f.ReceiverId == receiverId &&
                f.Status == "Pending"
            )), Times.Once);

            _notificationRepositoryMock.Verify(repo => repo.AddNotificationAsync(It.Is<Notification>(n =>
                n.UserId == receiverId &&
                n.SenderId == requesterId &&
                n.Type == "FriendRequest"
            )), Times.Once);
        }

        #endregion

        #region RespondToRequestAsync Tests

        [Fact]
        public async Task RespondToRequestAsync_RequestNotFound_ReturnsNull()
        {
            // Arrange
            _friendshipRepositoryMock.Setup(repo => repo.FindPendingAsync("user", "requester")).ReturnsAsync((Friendship?)null);

            // Act
            var result = await _friendService.RespondToRequestAsync("user", "requester", "Accepted");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task RespondToRequestAsync_StatusAccepted_UpdatesFriendshipAndReturnsMessage()
        {
            // Arrange
            var friendship = new Friendship { RequesterId = "requester", ReceiverId = "current", Status = "Pending" };
            _friendshipRepositoryMock.Setup(repo => repo.FindPendingAsync("current", "requester")).ReturnsAsync(friendship);

            // Act
            var result = await _friendService.RespondToRequestAsync("current", "requester", "Accepted");

            // Assert
            Assert.NotNull(result);
            var type = result.GetType();
            Assert.Equal("Friend request accepted.", type.GetProperty("message")?.GetValue(result));

            _friendshipRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<Friendship>(f => f.Status == "Accepted")), Times.Once);
        }

        [Fact]
        public async Task RespondToRequestAsync_StatusDeclined_RemovesFriendshipAndReturnsMessage()
        {
            // Arrange
            var friendship = new Friendship { RequesterId = "requester", ReceiverId = "current", Status = "Pending" };
            _friendshipRepositoryMock.Setup(repo => repo.FindPendingAsync("current", "requester")).ReturnsAsync(friendship);

            // Act
            var result = await _friendService.RespondToRequestAsync("current", "requester", "Declined");

            // Assert
            Assert.NotNull(result);
            var type = result.GetType();
            Assert.Equal("Friend request declined.", type.GetProperty("message")?.GetValue(result));

            _friendshipRepositoryMock.Verify(repo => repo.RemoveAsync(friendship), Times.Once);
        }

        [Fact]
        public async Task RespondToRequestAsync_InvalidStatus_ReturnsNull()
        {
            // Arrange
            var friendship = new Friendship { Status = "Pending" };
            _friendshipRepositoryMock.Setup(repo => repo.FindPendingAsync("current", "requester")).ReturnsAsync(friendship);

            // Act
            var result = await _friendService.RespondToRequestAsync("current", "requester", "InvalidStatus");

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetMyFriendAsync & GetPendingRequestAsync Tests

        [Fact]
        public async Task GetMyFriendAsync_ReturnsFriendList()
        {
            // Arrange
            // Đã đổi FriendId -> UserId, FriendName -> DisplayName cho khớp với thực tế DTO
            var expectedFriends = new List<FriendResponse> { new FriendResponse { UserId = "f1", DisplayName = "Bob" } };
            _friendshipRepositoryMock.Setup(repo => repo.GetMyFriendsAsync("user1")).ReturnsAsync(expectedFriends);

            // Act
            var result = await _friendService.GetMyFriendAsync("user1");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetPendingRequestAsync_ReturnsPendingList()
        {
            // Arrange
            // Đã đổi FriendId -> UserId, FriendName -> DisplayName cho khớp với thực tế DTO
            var expectedPending = new List<FriendResponse> { new FriendResponse { UserId = "f2", DisplayName = "Charlie" } };
            _friendshipRepositoryMock.Setup(repo => repo.GetPendingRequestAsync("user1")).ReturnsAsync(expectedPending);

            // Act
            var result = await _friendService.GetPendingRequestAsync("user1");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        #endregion

        #region GetFriendshipStatusAsync Tests

        [Fact]
        public async Task GetFriendshipStatusAsync_NoFriendship_ReturnsStatusNone()
        {
            // Arrange
            _friendshipRepositoryMock.Setup(repo => repo.CheckFriendshipAsync("u1", "u2")).ReturnsAsync((Friendship?)null);

            // Act
            var result = await _friendService.GetFriendshipStatusAsync("u1", "u2");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("None", result.GetType().GetProperty("status")?.GetValue(result));
        }

        [Fact]
        public async Task GetFriendshipStatusAsync_HasFriendship_ReturnsStatusAndRequesterId()
        {
            // Arrange
            var friendship = new Friendship { Status = "Accepted", RequesterId = "u1" };
            _friendshipRepositoryMock.Setup(repo => repo.CheckFriendshipAsync("u1", "u2")).ReturnsAsync(friendship);

            // Act
            var result = await _friendService.GetFriendshipStatusAsync("u1", "u2");

            // Assert
            Assert.NotNull(result);
            var type = result.GetType();
            Assert.Equal("Accepted", type.GetProperty("status")?.GetValue(result));
            Assert.Equal("u1", type.GetProperty("requesterId")?.GetValue(result));
        }

        #endregion

        #region UnfriendAsync Tests

        [Fact]
        public async Task UnfriendAsync_FriendshipDoesNotExist_ReturnsFalse()
        {
            // Arrange
            _friendshipRepositoryMock.Setup(repo => repo.CheckFriendshipAsync("u1", "u2")).ReturnsAsync((Friendship?)null);

            // Act
            var result = await _friendService.UnfriendAsync("u1", "u2");

            // Assert
            Assert.False(result);
            _friendshipRepositoryMock.Verify(repo => repo.RemoveAsync(It.IsAny<Friendship>()), Times.Never);
        }

        [Fact]
        public async Task UnfriendAsync_FriendshipExists_RemovesAndReturnsTrue()
        {
            // Arrange
            var friendship = new Friendship { Id = 1 };
            _friendshipRepositoryMock.Setup(repo => repo.CheckFriendshipAsync("u1", "u2")).ReturnsAsync(friendship);

            // Act
            var result = await _friendService.UnfriendAsync("u1", "u2");

            // Assert
            Assert.True(result);
            _friendshipRepositoryMock.Verify(repo => repo.RemoveAsync(friendship), Times.Once);
        }

        #endregion
    }
}