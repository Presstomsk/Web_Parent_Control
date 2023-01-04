using System;
using Web_Parent_Control.Services.Abstractions;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Web_Parent_Control.Services
{
    public class Token : IToken
    {
       

        public string GenerateToken()
        {            
            var jwt = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)), // время действия 2 минуты
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }

    public class AuthOptions
    {
        public const string ISSUER = "Web_Parent_Control"; // издатель токена
        public const string AUDIENCE = "Parent_Spy"; // потребитель токена
        const string KEY = "mySecretKey_mysuperkey";   // ключ для шифрации
        public static SymmetricSecurityKey GetSymmetricSecurityKey() =>
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
    }
}
