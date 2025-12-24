using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet("TestAdmin")]
        [Authorize(Roles = "Admin")]
        public void TestAdmin()
        {
            Console.WriteLine("Controller AdminTest tested successfully");
            
        }

        [HttpGet("TestUser")]
        [Authorize(Roles = "User")]
        public void TestUser()
        {
            Console.WriteLine("Controller UserTest tested successfully");
        }
    }
}
