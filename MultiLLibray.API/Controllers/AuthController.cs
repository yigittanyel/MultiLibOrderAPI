using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MultiLLibray.API.Context;
using MultiLLibray.API.DTOs;
using MultiLLibray.API.MapperProfiles;
using MultiLLibray.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MultiLLibray.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly UserMapperProfile _userMapper;

        public AuthController(IConfiguration configuration, ApplicationDbContext context, UserMapperProfile userMapper)
        {
            _configuration = configuration;
            _context = context;
            _userMapper = userMapper;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserDto userDto)
        {
            User user= _userMapper.Map(userDto);
            user = await _context.Users.FirstOrDefaultAsync(x=>x.Username==userDto.username);

            if (user is not null && VerifyPassword(userDto.password, user.Password) is true)
            {
                var token = GenerateToken(user.Username);
                return Ok(new { Token = token });  
            }

            return Unauthorized();
        }

        private bool VerifyPassword(string enteredPassword, string storedPassword)
        {
            return enteredPassword == storedPassword;
        }

        private string GenerateToken(string username)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenOptions = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials);

            var token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

            return token;
        }

    }
}
