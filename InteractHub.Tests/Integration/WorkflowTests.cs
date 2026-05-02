using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InteractHub.Api.Dtos.Posts;
using InteractHub.Api.Entities;
using InteractHub.Api.Hubs;
using InteractHub.Api.Repositories;
using InteractHub.Api.Services;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Xunit;

namespace InteractHub.Tests.Integration
{
	public class WorkflowTests
	{
		[Fact]
		public async Task CriticalWorkflow_AddFriendAndCreatePost_ShouldIntegrateSuccessfully()
		{
			// =========================================================
			// 1. SETUP CHUNG (Shared Mocks cho toàn bộ các Services)
			// =========================================================
			var userRepositoryMock = new Mock<IUserRepository>();
			var friendshipRepoMock = new Mock<IFriendshipRepository>();
			var notificationRepoMock = new Mock<INotificationRepository>();
			var postRepoMock = new Mock<IPostRepository>();
			var hashtagServiceMock = new Mock<IHashtagService>();

			// Mock SignalR
			var hubContextMock = new Mock<IHubContext<NotificationHub>>();
			var mockClients = new Mock<IHubClients>();
			var mockClientProxy = new Mock<IClientProxy>();
			mockClients.Setup(c => c.All).Returns(mockClientProxy.Object);
			mockClients.Setup(c => c.User(It.IsAny<string>())).Returns(mockClientProxy.Object);
			hubContextMock.Setup(h => h.Clients).Returns(mockClients.Object);

			// Khởi tạo các Service thực tế sẽ tương tác với nhau
			var friendService = new FriendService(
				userRepositoryMock.Object,
				friendshipRepoMock.Object,
				notificationRepoMock.Object,
				hubContextMock.Object);

			var postService = new PostService(
				postRepoMock.Object,
				hubContextMock.Object,
				hashtagServiceMock.Object);

			var userService = new UserService(
				userRepositoryMock.Object,
				postRepoMock.Object);

			// Dữ liệu giả lập dùng chung cho toàn bộ Workflow
			var userAId = "userA";
			var userBId = "userB";

			userRepositoryMock.Setup(repo => repo.FindUserAsync(userAId))
				.ReturnsAsync(new ApplicationUser { Id = userAId, DisplayName = "Alice" });
			userRepositoryMock.Setup(repo => repo.FindUserAsync(userBId))
				.ReturnsAsync(new ApplicationUser { Id = userBId, DisplayName = "Bob" });

			// =========================================================
			// 2. THỰC THI WORKFLOW VÀ ASSERT TỪNG BƯỚC
			// =========================================================

			// BƯỚC 1: User A gửi lời mời kết bạn cho User B
			var pendingFriendship = new Friendship
			{
				RequesterId = userAId,
				ReceiverId = userBId,
				Status = "Pending"
			};

			// Ban đầu chưa là bạn
			friendshipRepoMock.Setup(repo => repo.CheckFriendshipAsync(userBId, userAId))
				.ReturnsAsync((Friendship?)null);

			var sendReqResult = await friendService.SendFriendRequestAsync(userAId, userBId);
			Assert.NotNull(sendReqResult);
			var sendReqType = sendReqResult.GetType();
			Assert.Equal("Friend request sent successfully.", sendReqType.GetProperty("message")?.GetValue(sendReqResult));

			// BƯỚC 2: User B chấp nhận lời mời
			// Giả lập repo lúc này tìm thấy Pending Request
			friendshipRepoMock.Setup(repo => repo.FindPendingAsync(userBId, userAId))
				.ReturnsAsync(pendingFriendship);

			var acceptReqResult = await friendService.RespondToRequestAsync(userBId, userAId, "Accepted");
			Assert.NotNull(acceptReqResult);

			// Kiểm tra record đã được Update sang Accepted
			friendshipRepoMock.Verify(repo => repo.UpdateAsync(It.Is<Friendship>(f => f.Status == "Accepted")), Times.Once);

			// BƯỚC 3: User A tạo một bài viết mới chia sẻ niềm vui
			var newPostReq = new CreatePostRequest { Content = "Hello my new friend!", ImageUrl = null };

			// Giả lập lưu vào DB
			postRepoMock.Setup(repo => repo.AddAsync(It.IsAny<Post>()))
				.ReturnsAsync((Post p) => { p.Id = 1; return p; });

			var createPostResult = await postService.CreatePostAsync(newPostReq, userAId);
			Assert.NotNull(createPostResult);

			var createPostType = createPostResult.GetType();
			Assert.Equal("Post created successfully", createPostType.GetProperty("message")?.GetValue(createPostResult));
			Assert.Equal("Hello my new friend!", createPostType.GetProperty("Content")?.GetValue(createPostResult));

			// BƯỚC 4: User B vào xem Profile của User A
			// Giả lập User A đang có 1 bài viết (vừa tạo)
			postRepoMock.Setup(repo => repo.CountUserAsync(userAId)).ReturnsAsync(1);

			var userAProfile = await userService.GetUserProfileAsync(userAId);
			Assert.NotNull(userAProfile);

			var profileType = userAProfile.GetType();
			Assert.Equal(userAId, profileType.GetProperty("Id")?.GetValue(userAProfile));
			Assert.Equal(1, profileType.GetProperty("totalPosts")?.GetValue(userAProfile)); // Đảm bảo đếm đúng 1 bài viết

			// Kiểm chứng xem các phương thức của 3 Service có thực sự nối tiếp nhau logic không
			notificationRepoMock.Verify(repo => repo.AddNotificationAsync(It.IsAny<Notification>()), Times.Once);
			hashtagServiceMock.Verify(service => service.ProcessHashtagsAsync("Hello my new friend!"), Times.Once);
		}
	}
}