using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Article_Backend.Services
{
    public class JwtService
    {
        private readonly JwtSetting _jwt;
        public JwtService(IOptions<JwtSetting> jwt)
        {
            _jwt = jwt.Value;
        }
        public string CreateToken(UserDetail userDetail)
        {
            byte[] key = Encoding.ASCII.GetBytes(_jwt.Key);
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("Id",userDetail.Id.ToString(),ClaimValueTypes.Integer32),
                    new Claim("Name",userDetail.Name),
                    new Claim("Status",userDetail.Status.ToString(),ClaimValueTypes.Integer32)
                }),
                Expires = DateTime.Now.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken = tokenHandler.CreateToken(tokenDescriptor);
            string token = tokenHandler.WriteToken(securityToken);
            return token;
        }

        public ClaimsPrincipal DecodeToken(String token)
        {
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            TokenValidationParameters tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ClockSkew = TimeSpan.Zero,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwt.Key))
            };
            ClaimsPrincipal claimsPrincipal = handler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            return claimsPrincipal;
        }
    }
}