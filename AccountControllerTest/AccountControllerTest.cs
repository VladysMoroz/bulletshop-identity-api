using System;
using System.Threading.Tasks;
using Api.Controllers;
using Api.DTOs.Account;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace AccountControllerTest
{
    public class AccountControllerTests
    {
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<SignInManager<User>> _mockSignInManager;
        private readonly Mock<JWTService> _mockJwtService;
        private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;
        private readonly Mock<EmailService> _mockEmailService;
        private readonly Mock<IConfiguration> _mockConfig;

        public AccountControllerTests()
        {
            _mockUserManager = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object,
                null, null, null, null, null, null, null);

            _mockSignInManager = new Mock<SignInManager<User>>(
                _mockUserManager.Object,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<User>>().Object,
                null, null, null, null);

            _mockJwtService = new Mock<JWTService>();
            _mockRoleManager = new Mock<RoleManager<IdentityRole>>(
                new Mock<IRoleStore<IdentityRole>>().Object,
                null, null, null, null);
            _mockEmailService = new Mock<EmailService>();
            _mockConfig = new Mock<IConfiguration>();
        }

        [Fact]
        public async Task RegisterUser_EmailAlreadyExists_ReturnsBadRequest()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Password = "Password123!"
            };

            _mockUserManager.Setup(x => x.FindByEmailAsync(registerDto.Email))
                            .ReturnsAsync(new User());

            var controller = new AccountController(
                _mockJwtService.Object,
                _mockSignInManager.Object,
                _mockUserManager.Object,
                _mockRoleManager.Object,
                _mockEmailService.Object,
                _mockConfig.Object);

            // Act
            var result = await controller.RegisterUser(registerDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal($"An existing account is using {registerDto.Email}, email adress. Please try with another email adress", badRequestResult.Value);
        }

        [Fact]
        public async Task RegisterUser_SuccessfulRegistration_ReturnsOk()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Password = "Password123!"
            };

            _mockUserManager.Setup(x => x.FindByEmailAsync(registerDto.Email))
                            .ReturnsAsync((User)null);

            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), registerDto.Password))
                            .ReturnsAsync(IdentityResult.Success);

            _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                            .ReturnsAsync(IdentityResult.Success);

            _mockEmailService.Setup(x => x.SendEmailAsync(It.IsAny<EmailSendDto>()))
                             .ReturnsAsync(true);

            var controller = new AccountController(
                _mockJwtService.Object,
                _mockSignInManager.Object,
                _mockUserManager.Object,
                _mockRoleManager.Object,
                _mockEmailService.Object,
                _mockConfig.Object);

            // Act
            var result = await controller.RegisterUser(registerDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<JsonResult>(okResult.Value);
            Assert.Equal("Your account has been created, please confrim your email address", response.Value.ToString());
        }

        [Fact]
        public async Task Login_InvalidUsernameOrPassword_ReturnsUnauthorized()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                UserName = "invalid",
                Password = "password"
            };

            _mockUserManager.Setup(x => x.FindByNameAsync(loginDto.UserName))
                            .ReturnsAsync((User)null);

            var controller = new AccountController(
                _mockJwtService.Object,
                _mockSignInManager.Object,
                _mockUserManager.Object,
                _mockRoleManager.Object,
                _mockEmailService.Object,
                _mockConfig.Object);

            // Act
            var result = await controller.Login(loginDto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Equal("Invalid username or password", unauthorizedResult.Value);
        }

        [Fact]
        public async Task Login_UnconfirmedEmail_ReturnsUnauthorized()
        {
            // Arrange
            var user = new User
            {
                UserName = "test",
                EmailConfirmed = false
            };

            var loginDto = new LoginDto
            {
                UserName = "test",
                Password = "password"
            };

            _mockUserManager.Setup(x => x.FindByNameAsync(loginDto.UserName))
                            .ReturnsAsync(user);

            var controller = new AccountController(
                _mockJwtService.Object,
                _mockSignInManager.Object,
                _mockUserManager.Object,
                _mockRoleManager.Object,
                _mockEmailService.Object,
                _mockConfig.Object);

            // Act
            var result = await controller.Login(loginDto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Equal("Please confirm your email", unauthorizedResult.Value);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsUserDto()
        {
            // Arrange
            var user = new User
            {
                UserName = "test",
                EmailConfirmed = true
            };

            var loginDto = new LoginDto
            {
                UserName = "test",
                Password = "password"
            };

            _mockUserManager.Setup(x => x.FindByNameAsync(loginDto.UserName))
                            .ReturnsAsync(user);

            _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, loginDto.Password, false))
                              .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            _mockJwtService.Setup(x => x.CreateJWT(user))
                           .ReturnsAsync("mockJwtToken");

            var controller = new AccountController(
                _mockJwtService.Object,
                _mockSignInManager.Object,
                _mockUserManager.Object,
                _mockRoleManager.Object,
                _mockEmailService.Object,
                _mockConfig.Object);

            // Act
            var result = await controller.Login(loginDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<UserDto>>(result);
            var userDto = Assert.IsType<UserDto>(actionResult.Value);
            Assert.Equal(loginDto.UserName, userDto.FirstName); // Assuming FirstName is set to UserName in this test
            Assert.Equal("mockJwtToken", userDto.JWT);
        }
    }
}
