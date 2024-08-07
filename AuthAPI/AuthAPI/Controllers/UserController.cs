﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngularAuthAPI.Context;
using AngularAuthAPI.Helpers;
using AngularAuthAPI.Models;
using AngularAuthAPI.Models.Dto;
using AuthAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
//
//Admin User
//username: krishnendhuarun
//password: Krishnendhu@123

namespace AuthAPI.Controllers
{
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IPhotoService _photoService;

        public UserController(AppDbContext context, IWebHostEnvironment env, IPhotoService photoService)
        {
            _context = context;
            _env = env;
            _photoService = photoService;
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] User userObj)
        {
            if (userObj == null)
                return BadRequest();

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.UserName == userObj.UserName);

            if (user == null)
                return NotFound(new { Message = "User not found" });

            if (!PasswordHasher.VerifyPassword(userObj.Password, user.Password))
            {
                return BadRequest(new { Message = "Password is incorrect" });
            }
            user.Token = CreateJwt(user);
            var newAccessToken = user.Token;
            var newRefreshToken = CreateRefreshToken();
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(5);
            await _context.SaveChangesAsync();

            return Ok(new TokenApiDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User userObj)
        {
            if (userObj == null)
                return BadRequest();

            //Check Username
            if (await CheckUsernameExistAsync(userObj.UserName))
            {
                return BadRequest(new { Message = "Username already exists" });
            }

            //Check email
            if (await CheckEmailExistAsync(userObj.Email))
            {
                return BadRequest(new { Message = "Email already exists" });
            }

            //Check password strength
            var pass = CheckPasswordStrength(userObj.Password);
            if (!string.IsNullOrEmpty(pass))
                return BadRequest(new { Message = pass.ToString() });


            userObj.Password = PasswordHasher.HashPassword(userObj.Password);
            userObj.Role = "User";
            userObj.Token = "";

            await _context.Users
                .AddAsync(userObj);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "User Registered!"
            });
        }

        private async Task<bool> CheckUsernameExistAsync(string username)
        {
            return await _context.Users.AnyAsync(x => x.UserName == username);
        }
        //same method can be writen as one line as below
        //  private  Task<bool> CheckUsernameExistAsync(string username)
        //    => _context.Users.AnyAsync(x => x.UserName == username);

        private async Task<bool> CheckEmailExistAsync(string email)
        {
            return await _context.Users.AnyAsync(x => x.Email == email);
        }

        private string CheckPasswordStrength(string password)
        {
            StringBuilder sb = new StringBuilder();
            if (password.Length < 8)
                sb.Append("Minimum password length should be 8" + Environment.NewLine);
            if (!(Regex.IsMatch(password, "[a-z]") && Regex.IsMatch(password, "[A-Z]") && Regex.IsMatch(password, "[0-9]")))
                sb.Append("Password should be alphanumeric" + Environment.NewLine);
            if (!Regex.IsMatch(password, "[~,!,@,#,$,%,^,&,*,(,),_,+,=,{,},|,\\,<,>,.,?,/,\n]"))
                sb.Append("Password should contain special characters" + Environment.NewLine);

            return sb.ToString();
        }

        private string CreateJwt(User user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("secretkeyofmine.....");
            var identity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role,user.Role),
                new Claim(ClaimTypes.Name,$"{user.UserName}")
            });
            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = credentials
            };
            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            return jwtTokenHandler.WriteToken(token);
        }

        private string CreateRefreshToken()
        {
            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var refreshToken = Convert.ToBase64String(tokenBytes);
            var tokenInUser = _context.Users.Any(x => x.RefreshToken == refreshToken);
            if (tokenInUser)
            {
                return CreateRefreshToken();
            }
            return refreshToken;
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var key = Encoding.ASCII.GetBytes("secretkeyofmine.....");
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = false
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("This is invalid token");
            return principal;
        }

        [Authorize]   //protect this API endopint from being accessed without authorisation
        [HttpGet]
        public async Task<ActionResult<User>> GetAllUsers()
        {
            return Ok(await _context.Users.ToListAsync());
        }

        [Authorize]
        [HttpGet("user")]
        public async Task<ActionResult<User>> GetUser(string username)
        {
            return Ok(await _context.Users.FirstOrDefaultAsync(u => u.UserName == username));
        }


        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(TokenApiDto tokenApiDto)
        {
            if (tokenApiDto == null)
                return BadRequest("Invalid client request");
            string accessToken = tokenApiDto.AccessToken;
            string refreshToken = tokenApiDto.RefreshToken;
            var principal = GetPrincipalFromExpiredToken(accessToken);
            var username = principal.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
            if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
                return BadRequest("Invalid request");
            var newAccessToken = CreateJwt(user);
            var newRefreshToken = CreateRefreshToken();
            user.RefreshToken = newRefreshToken;
            await _context.SaveChangesAsync();
            return Ok(new TokenApiDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }

        [HttpGet("userById")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            return Ok(await _context.Users.FirstOrDefaultAsync(u => u.Id == id));
        }

        [HttpPut("UpdateUser/{id}")]
        public async Task<ActionResult> UpdateUser(int id, [FromBody] User userObj)
        {
            if (userObj == null || id <= 0)
                return BadRequest(new { Message = "Invalid user data" });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return NotFound(new { Message = "User not found" });

            // Update user fields
            user.UserName = userObj.UserName;
            user.Email = userObj.Email;

            // If updating password, hash it
            if (!string.IsNullOrEmpty(userObj.Password))
            {
                user.Password = PasswordHasher.HashPassword(userObj.Password);
            }

            // Additional fields to update
            user.FirstName = userObj.FirstName;
            user.LastName = userObj.LastName;

            try
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return Ok(new { Message = "User updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while updating the user", Details = ex.Message });
            }
        }

        [HttpDelete("DeleteUser/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (id <= 0)
                return BadRequest(new { Message = "Invalid user ID" });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return NotFound(new { Message = "User not found" });

            try
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return Ok(new { Message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while deleting the user", Details = ex.Message });
            }
        }

        [HttpDelete("uploadphoto/{id}")]
        public async Task<IActionResult> UploadPhoto(IFormFile file,int id)
        {
            var result = await _photoService.UploadPhotoAsync(file);
            if(result.Error!=null)
            {
                return BadRequest(result.Error.Message);
            }
            return Ok(201);
        }

    }
}

