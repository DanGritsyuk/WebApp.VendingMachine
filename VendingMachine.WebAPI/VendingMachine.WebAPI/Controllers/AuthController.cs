using HireWise.Common.Entities.LoginModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendingMachine.BLL.Logic.Contracts;
using VendingMachine.WebAPI.Contracts.Auth;

namespace VendingMachine.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationLogic _adminTokenService;

        public AuthController(IAuthenticationLogic adminTokenService)
        {
            _adminTokenService = adminTokenService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public ActionResult<LoginResponse> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            LoginModel loginData = new LoginModel()
            {
                Login = request.UserName,
                Password = request.Password,
            };

            var response = _adminTokenService.LoginAsync(loginData, cancellationToken);
            if (response == null)
            {
                return Unauthorized(new { message = "Invalid credentials." });
            }

            return Ok(response);
        }
    }
}
