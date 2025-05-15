using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ChatRoomAPI.TokenService
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// 產生jwt
        /// </summary>
        /// <param name="dtbResult"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public string GenerateJwt(IEnumerable<Claim> claims)
        {
            try
            {
                JwtSecurityTokenHandler jwtTokenHandler = new JwtSecurityTokenHandler();
                IConfigurationSection jwtConfig = _configuration.GetSection("JwtConfig");
                SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig["Secret"]));
                SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                JwtSecurityToken token = new JwtSecurityToken(
                    issuer: jwtConfig["Issuer"],
                    audience: jwtConfig["Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtConfig["ExpiryMinute"])),
                    signingCredentials: creds);

                return jwtTokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                throw new Exception("GenerateJwt Error : " + ex.Message);
            }
        }

        /// <summary>
        /// 產生refreshToken
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public string GenerateRefreshToken()
        {
            try
            {
                byte[] ramdomNumber = new byte[32];

                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(ramdomNumber);

                    return Convert.ToBase64String(ramdomNumber);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("GenerateRefreshToken Error : " + ex.Message);
            }
        }

        /// <summary>
        /// 從過期的token中取得用戶主體
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            try
            {
                IConfigurationSection jwtConfig = _configuration.GetSection("JwtConfig");
                var tokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateAudience = false,
                    ValidateIssuer = true,
                    ValidAudience = jwtConfig["Audience"],
                    ValidIssuer = jwtConfig["Issuer"],
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = false,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig["Secret"])),
                    ClockSkew = TimeSpan.Zero
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                SecurityToken securityToken;
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
                var jwtSecurityToken = securityToken as JwtSecurityToken;

                if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("Invalid token");
                }

                return principal;
            }
            catch (Exception ex)
            {
                throw new Exception("GetPrincipalFromExpiredToken Error: " + ex.Message);
            }
        }
    }
}
