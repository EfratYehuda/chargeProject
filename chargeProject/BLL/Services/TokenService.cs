using DAL.Classes;
using DTO;
using DTO.USERDTO;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MyApp.BLL.Services;

public interface ITokenService
{
    AuthResponseDto CreateToken(Users user);
}

public class TokenService : ITokenService

{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config) => _config = config;

    public AuthResponseDto CreateToken(Users user)
    {
        ArgumentNullException.ThrowIfNull(user);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Everyone is "User". Only the database value "Admin" makes an admin.
        var role = user.Access == UserRoles.Admin ? UserRoles.Admin : UserRoles.User;

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Name, user.Name ?? string.Empty),
            new Claim(ClaimTypes.Role, role)
        };

        if (user.DriverProfile != null)
            claims.Add(new Claim("driverId", user.DriverProfile.Id.ToString()));
        if (user.OwnerProfile != null)
            claims.Add(new Claim("ownerId", user.OwnerProfile.Id.ToString()));

        var minutes = int.TryParse(_config["Jwt:ExpiresMinutes"], out var m) && m > 0 ? m : 60; var expires = DateTime.UtcNow.AddMinutes(minutes);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return new AuthResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expires
        };
    }
}