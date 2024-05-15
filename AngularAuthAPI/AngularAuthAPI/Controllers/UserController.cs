using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngularAuthAPI.Context;
using AngularAuthAPI.Helpers;
using AngularAuthAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AngularAuthAPI.Controllers
{
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
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

            if(!PasswordHasher.VerifyPassword(userObj.Password,user.Password))
            {
                return BadRequest(new { Message = "Password is incorrect" });
            }
            user.Token = CreateJwt(user);

            return Ok(new
            {
                Token=user.Token,
                Message="Login Success!"
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User userObj)
        {
            if (userObj == null)
                return BadRequest();

            //Check Username
            if(await CheckUsernameExistAsync(userObj.UserName))
            {
                return BadRequest(new { Message = "Username already exists"});
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
            if(password.Length<8)
                sb.Append("Minimum password length should be 8" + Environment.NewLine);
            if (!(Regex.IsMatch(password, "[a-z]") && Regex.IsMatch(password, "[A-Z]") && Regex.IsMatch(password, "[0-9]")))
                sb.Append("Password should be alphanumeric"+ Environment.NewLine);
            if (!Regex.IsMatch(password, "[~,!,@,#,$,%,^,&,*,(,),_,+,=,{,},|,\\,<,>,.,?,/,\n]"))
                sb.Append("Password should contain special characters"+Environment.NewLine);

            return sb.ToString();
        }

        private string CreateJwt(User user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("secretkeyofmine.....");
            var identity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role,user.Role),
                new Claim(ClaimTypes.Name,$"{user.FirstName} {user.LastName}"),
                 new Claim("username", user.UserName)
            });
            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                Expires=DateTime.Now.AddDays(1),
                SigningCredentials=credentials
            };
            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            return jwtTokenHandler.WriteToken(token);
        }

        [Authorize]   //protect this API endopint from being accessed without authorisation
        [HttpGet]
        public async Task<ActionResult<User>> GetAllUsers()
        {
            return Ok(await _context.Users.ToListAsync());
        }

        //[Authorize]
        //[Route("api/[controller]/details")]
        //[HttpGet]
        //public async Task<ActionResult<User>> getUserDetails()
        //{
        //    var usernameClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "username");
        //    if (usernameClaim == null)
        //    {
        //        return Unauthorized(); 
        //    }

        //    var username = usernameClaim.Value;
        //    // Use the username to fetch the user data
        //    var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
        //    if (user == null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(user);
        //}

    }
}

