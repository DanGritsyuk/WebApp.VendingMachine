using HireWise.Common.Entities.LoginModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VendingMachine.BLL.Logic.Contracts;
using VendingMachine.WebAPI.Options;

namespace VendingMachine.BLL.Logic
{
    public class AuthenticationLogic : IAuthenticationLogic
    {
        private readonly AdminAuthOptions _authOptions;

        private readonly ILogger<AuthenticationLogic> _logger;

        public AuthenticationLogic(AdminAuthOptions authOptions,
            ILogger<AuthenticationLogic> logger)
        {
            _authOptions = authOptions;
            _logger = logger;
        }

        public IResult LoginAsync(LoginModel loginModel, CancellationToken cancellationToken = default)
        {
            if (loginModel.Login != _authOptions.UserName || 
                loginModel.Password != _authOptions.Password)
                return Results.Unauthorized();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, loginModel.Login),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, _authOptions.Role)
            };


            // создаем JWT-токен
            var jwt = new JwtSecurityToken(
                issuer: _authOptions.Issuer,
                audience: _authOptions.Audience,
                claims: claims,
                expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(_authOptions.AccessTokenLifetimeMinutes)),
                signingCredentials: new SigningCredentials
                (
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authOptions.SecretKey)), 
                    SecurityAlgorithms.HmacSha256Signature
                )
             );

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt).ToString();//.Replace("Bearer", string.Empty);
            _logger.LogInformation($"Generated token for user: {loginModel.Login}: {jwt}");

            // формируем ответ
            var response = new
            {
                AccessToken = encodedJwt,
                ExpiresAtUtc = DateTime.UtcNow
            };

            return Results.Json(response);
        }
    }
}
