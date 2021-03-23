using Beer.Identity.Commands.LocalUsers;
using Beer.Identity.Services;
using Beer.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Beer.Identity.UnitTests.Commands.LocalUsers
{
    public class CreateLocalUserCommandHandlerTester
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Handle(Boolean userServiceResult)
        {
            Random random = new Random();
            String username = random.GetAlphanumericString();
            String password = random.GetAlphanumericString();
            String displayName = random.GetAlphanumericString();
            String profilePictureUrl = "/myProfile/piger.png";

            Guid? userId = userServiceResult == true ? random.NextGuid() : new Guid?();

            var localUserServiceMock = new Mock<ILocalUserService>(MockBehavior.Strict);
            localUserServiceMock.Setup(x => x.CreateUser(username, password, displayName, profilePictureUrl)).ReturnsAsync(userId).Verifiable();

            var profilePictureMock = new Mock<IProfilePictureService>(MockBehavior.Strict);
            profilePictureMock.Setup(x => x.GetPossibleProfilePicture()).Returns(new List<(String Url, String Name)> {
             (profilePictureUrl,random.GetAlphanumericString())
            }).Verifiable();

            var handler = new CreateLocalUserCommandHandler(localUserServiceMock.Object, profilePictureMock.Object,
                Mock.Of<ILogger<CreateLocalUserCommandHandler>>());

            String result = await handler.Handle(new CreateLocalUserCommand(username, displayName, password, profilePictureUrl), CancellationToken.None);
            if (userServiceResult == true)
            {
                Assert.Equal(userId.ToString(), result);
            }
            else
            {
                Assert.True(String.IsNullOrEmpty(result));
            }

            localUserServiceMock.Verify();
            profilePictureMock.Verify();
        }

        [Fact]
        public async Task Handle_Failed_NotMatchtingProfilePictureUrl()
        {
            Random random = new();
            String username = random.GetAlphanumericString();
            String password = random.GetAlphanumericString();
            String displayName = random.GetAlphanumericString();
            String profilePictureUrl = "/myProfile/piger.png";

            var profilePictureMock = new Mock<IProfilePictureService>(MockBehavior.Strict);
            profilePictureMock.Setup(x => x.GetPossibleProfilePicture()).Returns(new List<(String Url, String Name)> {
             (random.GetAlphanumericString(),random.GetAlphanumericString())
            }).Verifiable();

            var handler = new CreateLocalUserCommandHandler(Mock.Of<ILocalUserService>(MockBehavior.Strict), profilePictureMock.Object,
                Mock.Of<ILogger<CreateLocalUserCommandHandler>>());

            String result = await handler.Handle(new CreateLocalUserCommand(username, displayName, password, profilePictureUrl), CancellationToken.None);

            Assert.True(String.IsNullOrEmpty(result));
            profilePictureMock.Verify();
        }
    }
}
