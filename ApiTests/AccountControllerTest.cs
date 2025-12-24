using Api.DTOs.Account;
using Api.Models;
using Castle.Core.Smtp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Raven.Database.Server.Controllers;

namespace ApiTests
{
    public class AccountControllerTest
    {
        private IdentityController _controller;
        private Mock<UserManager<User>> _mockUserManager;
        private Mock<SignInManager<User>> _mockSignInManager;
        private Mock<IEmailSender> _mockEmailSender;

        public IdentityControllerTests()
        {
            _mockUserManager = new Mock<UserManager<User>>(MockBehavior.Strict, null, null, null, null, null, null, null, null);
            _mockSignInManager = new Mock<SignInManager<User>>(_mockUserManager.Object,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<User>>().Object,
                null, null, null);
            _mockEmailSender = new Mock<IEmailSender>();

            _controller = new IdentityController(_mockUserManager.Object, _mockSignInManager.Object, _mockEmailSender.Object);
        }

        [Fact]
        public async Task Login_ValidModel_ReturnsUserDto()
        {
            // Arrange
            var loginDto = new LoginDto { UserName = "testuser", Password = "Test@123" };
            var user = new User { UserName = "testuser", EmailConfirmed = true };
            _mockUserManager.Setup(m => m.FindByNameAsync(loginDto.UserName)).ReturnsAsync(user);
            _mockSignInManager.Setup(m => m.CheckPasswordSignInAsync(user, loginDto.Password, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<UserDto>>(result);
            var okObjectResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var model = Assert.IsType<UserDto>(okObjectResult.Value);
            Assert.Equal(user.UserName, model.UserName);
            // Add more assertions as needed
        }

        [Fact]
        public async Task RegisterUser_ValidModel_ReturnsOk()
        {
            // Arrange
            var registerDto = new RegisterDto { FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", Password = "Test@123" };
            _mockUserManager.Setup(m => m.CreateAsync(It.IsAny<User>(), registerDto.Password)).ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(m => m.AddToRoleAsync(It.IsAny<User>(), "User")).ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(m => m.FindByEmailAsync(registerDto.Email)).ReturnsAsync((User)null);
            _mockEmailSender.Setup(m => m.SendConfirmEmailAsync(It.IsAny<User>())).ReturnsAsync(true);

            // Act
            var result = await _controller.RegisterUser(registerDto);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            dynamic data = actionResult.Value;
            Assert.Equal("Account Created", data.title);
            Assert.Equal("Your account has been created, please confrim your email address", data.message);
            // Add more assertions as needed
        }
    }
}
