using System.Data;
using System.Security.Claims;

namespace ChatRoomAPI.TokenService
{
    public interface ITokenService
    {
        string GenerateJwt(IEnumerable<Claim> claims);

        string GenerateRefreshToken();

        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
