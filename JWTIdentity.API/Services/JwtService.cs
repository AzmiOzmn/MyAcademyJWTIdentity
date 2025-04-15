using JWTIdentity.API.Entities;
using jwt = JWTIdentity.API.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

namespace JWTIdentity.API.Services
{
    public class JwtService : IJwtService
    {
        private readonly jwt.TokenOptions _tokenOptions;
        private readonly UserManager<AppUser> _userManager;

        public JwtService(IOptions<jwt.TokenOptions> tokenoptions, UserManager<AppUser> userManager)
        {
            _tokenOptions = tokenoptions.Value;
            _userManager = userManager;
        }

        public async Task<string> CreateTokenAsync(AppUser user)
        {
            #region Key Creation
            SymmetricSecurityKey symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenOptions.Key));
            #endregion

            #region Role Retrieval
            var userRoles = await _userManager.GetRolesAsync(user);
            #endregion

            #region Claims Creation
            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("FullName", user.Fullname),
                new Claim(ClaimTypes.Role, userRoles.FirstOrDefault()),
            };
            #endregion

            #region Token Configuration
            JwtSecurityToken jwtSecurityToken = new(
                issuer: _tokenOptions.Issuer,
                audience: _tokenOptions.Audience,
                claims: claims,
                notBefore: DateTime.Now,
                expires: DateTime.Now.AddMinutes(_tokenOptions.ExpireInMunites),
                signingCredentials: new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256)
            );
            #endregion

            #region Token Generation
            JwtSecurityTokenHandler handler = new();
            string token = handler.WriteToken(jwtSecurityToken);
            return token;
            #endregion
        }
    }
}
