using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MultiLLibray.API.Context;
using MultiLLibray.API.DTOs;
using MultiLLibray.API.MapperProfiles;
using MultiLLibray.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using MultiLLibray.API.Extensions;

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
        [AllowAnonymous]
        public async Task<IActionResult> Login(UserDto userDto)
        {
            User user= _userMapper.Map(userDto);
            user = await _context.Users.FirstOrDefaultAsync(x=>x.Username==userDto.username);

            if (user is not null && Helper.VerifyPassword(userDto.password, user.Password) is true)
            {
                var token = Helper.GenerateToken(user.Username,_configuration);
                return Ok(new { Token = token });  
            }

            return Unauthorized();
        }

    }
}
