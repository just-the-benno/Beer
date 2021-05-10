using Beer.Identity.ApiControllers;
using Beer.Identity.Commands.LocalUsers;
using Beer.Identity.Services;
using Beer.TestHelper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static Beer.Identity.Shared.Requests.LocalUserRequests.V1;
using static Beer.Identity.Shared.Responses.LocalUsersResponses.V1;

namespace Beer.Identity.UnitTests.ApiControllers
{
    public class LocalUserControllerTester
    {
        [Fact]
        public async Task IsLocalStoreInitilized_TrueDueUsers()
        {
            Random random = new Random();

            List<LocalUserOverview> expectedResult = new List<LocalUserOverview>
            {
                new LocalUserOverview{ Id = random.NextGuid().ToString(), LoginName = random.GetAlphanumericString()  },
            };

            Mock<ILocalUserService> userServiceMock = new Mock<ILocalUserService>(MockBehavior.Strict);
            userServiceMock.Setup(x => x.GetAllUsersSortedByName()).ReturnsAsync(expectedResult).Verifiable();

            var controller = new LocalUserController(
                Mock.Of<IMediator>(MockBehavior.Strict),
                userServiceMock.Object,
                Mock.Of<IProfilePictureService>(MockBehavior.Strict),
                Mock.Of<ILogger<LocalUserController>>()
                );

            var actionResult = await controller.IsLocalStoreInitilized();
            var actual = actionResult.EnsureOkObjectResult<Boolean>(true);
            Assert.True(actual);

            userServiceMock.Verify();
        }

        [Fact]
        public async Task IsLocalStoreInitilized_FalseDueNoUsers()
        {
            List<LocalUserOverview> expectedResult = new List<LocalUserOverview>();

            Mock<ILocalUserService> userServiceMock = new Mock<ILocalUserService>(MockBehavior.Strict);
            userServiceMock.Setup(x => x.GetAllUsersSortedByName()).ReturnsAsync(expectedResult).Verifiable();

            var controller = new LocalUserController(
                Mock.Of<IMediator>(MockBehavior.Strict),
                userServiceMock.Object,
                Mock.Of<IProfilePictureService>(MockBehavior.Strict),
                Mock.Of<ILogger<LocalUserController>>()
                );

            var actionResult = await controller.IsLocalStoreInitilized();
            var actual = actionResult.EnsureOkObjectResult<Boolean>(true);
            Assert.False(actual);

            userServiceMock.Verify();
        }

        [Fact]
        public async Task GetAllLocalUsers()
        {
            List<LocalUserOverview> expectedResult = new List<LocalUserOverview>();

            Mock<ILocalUserService> userServiceMock = new Mock<ILocalUserService>(MockBehavior.Strict);
            userServiceMock.Setup(x => x.GetAllUsersSortedByName()).ReturnsAsync(expectedResult).Verifiable();

            var controller = new LocalUserController(
                Mock.Of<IMediator>(MockBehavior.Strict),
                userServiceMock.Object,
                Mock.Of<IProfilePictureService>(MockBehavior.Strict),
                Mock.Of<ILogger<LocalUserController>>()
                );

            var actionResult = await controller.GetAllLocalUsers();
            var actual = actionResult.EnsureOkObjectResult<IEnumerable<LocalUserOverview>>(true);
            Assert.Equal(expectedResult, actual);

            userServiceMock.Verify();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CheckIfUserNameExists(Boolean shouldExsits)
        {
            Random random = new();
            String username = random.GetAlphanumericString();

            Mock<ILocalUserService> userServiceMock = new Mock<ILocalUserService>(MockBehavior.Strict);
            userServiceMock.Setup(x => x.CheckIfUsernameExists(username)).ReturnsAsync(shouldExsits).Verifiable();

            var controller = new LocalUserController(
                Mock.Of<IMediator>(MockBehavior.Strict),
                userServiceMock.Object,
                Mock.Of<IProfilePictureService>(MockBehavior.Strict),
                Mock.Of<ILogger<LocalUserController>>()
                );

            var actionResult = await controller.CheckIfUsernameExists(username);
            var actual = actionResult.EnsureOkObjectResult<Boolean>(true);
            Assert.Equal(shouldExsits, actual);

            userServiceMock.Verify();
        }

        [Fact]
        public void GetAvailableAvatars()
        {
            Random random = new();

            IEnumerable<String> expectedResult = new List<String>
            {
                $"/{random.GetAlphanumericString()}.png",
                $"/{random.GetAlphanumericString()}.png",
                $"/{random.GetAlphanumericString()}.png",
            };

            Mock<IProfilePictureService> profileServiceMock = new Mock<IProfilePictureService>(MockBehavior.Strict);
            profileServiceMock.Setup(x => x.GetPossibleProfilePicture()).Returns(new List<(String Url, String Name)>{
                (expectedResult.ElementAt(0),"blub"),
                (expectedResult.ElementAt(1),"bli"),
                (expectedResult.ElementAt(2),"bla"),
            }).Verifiable();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Scheme = "https";
            httpContext.Request.Host = new HostString("testlocalhost.local", 72);

            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };

            var controller = new LocalUserController(
                Mock.Of<IMediator>(MockBehavior.Strict),
                Mock.Of<ILocalUserService>(MockBehavior.Strict),
                profileServiceMock.Object,
                Mock.Of<ILogger<LocalUserController>>()
                )
            {
                ControllerContext = controllerContext,
            };

            var actionResult = controller.GetAvailableAvatars();
            var actual = actionResult.EnsureOkObjectResult<IEnumerable<String>>(true);
            Assert.Equal(expectedResult.Select(x => $"https://testlocalhost.local:72{x}"), actual);

            profileServiceMock.Verify();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ResetUserPassword(Boolean mediatorResult)
        {
            Random random = new Random();
            String userId = random.GetAlphanumericString(5);
            String password = random.GetAlphanumericString(15);

            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock.Setup(x => x.Send(It.Is<ResetLocalUserPasswordCommand>(y =>
           y.UserId == userId &&
           y.Password == password), It.IsAny<CancellationToken>())).ReturnsAsync(mediatorResult).Verifiable();

            var controller = new LocalUserController(
                mediatorMock.Object,
                Mock.Of<ILocalUserService>(MockBehavior.Strict),
                Mock.Of<IProfilePictureService>(MockBehavior.Strict),
                Mock.Of<ILogger<LocalUserController>>()
                );

            var actionResult = await controller.ResetUserPassword(userId, new ResetPasswordRequest { Password = password });
            if (mediatorResult == true)
            {
                actionResult.EnsureNoContentResult();
            }
            else
            {
                actionResult.EnsureBadRequestObjectResult("unable to complete service operation");
            }

            mediatorMock.Verify();
        }

        private async Task CheckModelState(Func<LocalUserController, Task<IActionResult>> controllerExecuter)
        {
            Random random = new Random();

            var controller = new LocalUserController(
                Mock.Of<IMediator>(MockBehavior.Strict),
                Mock.Of<ILocalUserService>(MockBehavior.Strict),
                Mock.Of<IProfilePictureService>(MockBehavior.Strict),
                Mock.Of<ILogger<LocalUserController>>()
                );

            String modelErrorKey = "a" + random.GetAlphanumericString();
            String modelErrorMessage = random.GetAlphanumericString();
            controller.ModelState.AddModelError(modelErrorKey, modelErrorMessage);

            var result = await controllerExecuter(controller);

            result.EnsureBadRequestObjectResultForError(modelErrorKey, modelErrorMessage);
        }

        [Fact]
        public async Task ResetUserPassword_ModelStateError()
        {
            await CheckModelState((controller) => controller.ResetUserPassword("", null));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task DeleteUser(Boolean mediatorResult)
        {
            Random random = new Random();
            String userId = random.GetAlphanumericString(5);

            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock.Setup(x => x.Send(It.Is<DeleteLocalUserCommand>(y =>
           y.UserId == userId), It.IsAny<CancellationToken>())).ReturnsAsync(mediatorResult).Verifiable();

            var controller = new LocalUserController(
                mediatorMock.Object,
                Mock.Of<ILocalUserService>(MockBehavior.Strict),
                Mock.Of<IProfilePictureService>(MockBehavior.Strict),
                Mock.Of<ILogger<LocalUserController>>()
                );

            var actionResult = await controller.DeleteUser(userId);
            if (mediatorResult == true)
            {
                actionResult.EnsureNoContentResult();
            }
            else
            {
                actionResult.EnsureBadRequestObjectResult("unable to complete service operation");
            }

            mediatorMock.Verify();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CreateUser(Boolean mediatorResult)
        {
            Random random = new Random();
            String userId = mediatorResult == true ? random.GetAlphanumericString(5) : null;

            String username = random.GetAlphanumericString();
            String password = random.GetAlphanumericString();
            String displayName = random.GetAlphanumericString();
            String avatarUrl = random.GetAlphanumericString();

            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock.Setup(x => x.Send(It.Is<CreateLocalUserCommand>(y =>
           y.Username == username &&
           y.DisplayName == displayName &&
           y.ProfilePictureUrl == avatarUrl &&
           y.Password == password), It.IsAny<CancellationToken>())).ReturnsAsync(userId).Verifiable();

            var controller = new LocalUserController(
                mediatorMock.Object,
                Mock.Of<ILocalUserService>(MockBehavior.Strict),
                Mock.Of<IProfilePictureService>(MockBehavior.Strict),
                Mock.Of<ILogger<LocalUserController>>()
                );

            var actionResult = await controller.CreateUser(new CreateUserRequest { Username = username, Password = password, DisplayName = displayName, ProfilePictureUrl = avatarUrl });
            if (mediatorResult == true)
            {
                String actualId = actionResult.EnsureOkObjectResult<String>(true);
                Assert.Equal("\"" + userId + "\"", actualId);
            }
            else
            {
                actionResult.EnsureBadRequestObjectResult("unable to complete service operation");
            }

            mediatorMock.Verify();
        }

        [Fact]
        public async Task CreateUser_ModelStateError()
        {
            await CheckModelState((controller) => controller.CreateUser(null));
        }

    }
}
