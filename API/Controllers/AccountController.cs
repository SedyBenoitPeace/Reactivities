using System.Text;
using System;
using System.Net.Http;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.DTOs;
using API.Services;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Infrastructure.Email;
using Microsoft.AspNetCore.WebUtilities;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly TokenService _tokenService;
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly EmailSender _emailSender;
        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, TokenService tokenService, IConfiguration config, EmailSender emailSender)
        {
            this._emailSender = emailSender;
            this._config = config;
            this._tokenService = tokenService;
            this._signInManager = signInManager;
            this._userManager = userManager;
            _httpClient = new HttpClient
            {
                BaseAddress = new System.Uri("https://graph.facebook.com")
            };
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LogintDto logintDto)
        {
            var user = await _userManager.Users.Include(p => p.Photos).FirstOrDefaultAsync(u => u.Email == logintDto.Email);

            if (user == null)
                return Unauthorized();

            if (!user.EmailConfirmed)
                return Unauthorized("Email not verified");

            var result = await _signInManager.CheckPasswordSignInAsync(user, logintDto.Password, false);

            if (!result.Succeeded)
                return Unauthorized("Wrong password");

            await SetRefreshToken(user);

            return CreateUserObject(user);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await _userManager.FindByEmailAsync(registerDto.Email) != null)
            {
                ModelState.AddModelError("email", "Email already in use");
                return ValidationProblem();
            }


            if (await _userManager.Users.AnyAsync(u => u.UserName == registerDto.Username))
            {
                ModelState.AddModelError("username", "username already in use");
                return ValidationProblem();
            }


            var user = new AppUser
            {
                DisplayName = registerDto.DisplayName,
                Email = registerDto.Email,
                UserName = registerDto.Email
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            var origin = Request.Headers["origin"];
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var verifyUrl = $"{origin}/account/verifyEmail?token={token}&email={user.Email}";

            var message = $"<p>Please click the below link to verify your email address:</p><p><a href='{verifyUrl}'>Click to verify email<a/></p>";

            await _emailSender.SendEmailAsync(user.Email, "Please verify email", message);

            return Ok("Regstration succeded");

        }

        [AllowAnonymous]
        [HttpPost("verifyEmail")]
        public async Task<ActionResult> VerifyEmail(string token, string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null) return Unauthorized();

            var decodedTokenBytes = WebEncoders.Base64UrlDecode(token);

            var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if(!result.Succeeded) return BadRequest("Could not verify the email");

            return Ok("Email confirmed");
        }

                [AllowAnonymous]
        [HttpGet("resendEmailConfirmationLink")]
        public async Task<ActionResult> ResendEmailConfirmationLink(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null) return Unauthorized();

            var origin = Request.Headers["origin"];
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var verifyUrl = $"{origin}/account/verifyEmail?token={token}&email={user.Email}";

            var message = $"<p>Please click the below link to verify your email address:</p><p><a href='{verifyUrl}'>Click to verify email<a/></p>";

            await _emailSender.SendEmailAsync(user.Email, "Please verify email", message);

            return Ok("Email link resent");
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var user = await _userManager.Users.Include(p => p.Photos).FirstOrDefaultAsync(u => u.Email == User.FindFirstValue(ClaimTypes.Email));

            await SetRefreshToken(user);

            return CreateUserObject(user);

        }

        [AllowAnonymous]
        [HttpPost("fbLogin")]
        public async Task<ActionResult<UserDto>> FacebookLogin(string accessToken)
        {
            var fbVerifyKeys = _config["Facebook:AppId"] + "|" + _config["Facebook:AppSecret"];

            var verifyToken = await _httpClient.GetAsync($"debug_token?input_token={accessToken}&access_token={fbVerifyKeys}");

            if (!verifyToken.IsSuccessStatusCode) return Unauthorized();

            var fbUrl = $"me?access_token={accessToken}&fields=name,email,picture.width(100).height(100)";

            var response = await _httpClient.GetAsync(fbUrl);

            if (!response.IsSuccessStatusCode) return Unauthorized();

            var fbInfo = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            var username = (string)fbInfo.id;

            var user = await _userManager.Users.Include(p => p.Photos).FirstOrDefaultAsync(x => x.UserName == username);

            if (user != null) return CreateUserObject(user);

            user = new AppUser
            {
                DisplayName = (string)fbInfo.name,
                Email = (string)fbInfo.email,
                UserName = (string)fbInfo.id,
                Photos = new List<Photo> { new Photo { Id = "fb_" + (string)fbInfo.Id, Url = (string)fbInfo.picture.data.url, IsMain = true } },
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user);

            if (!result.Succeeded) return BadRequest("problem when creating account");

            await SetRefreshToken(user);

            return CreateUserObject(user);
        }

        [Authorize]
        [HttpPost("refreshToken")]
        public async Task<ActionResult<UserDto>> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var user = await _userManager.Users.Include(r => r.RefreshTokens).FirstOrDefaultAsync(x => x.UserName == User.FindFirstValue(ClaimTypes.Name));

            if (user == null) return Unauthorized();

            var oldToken = user.RefreshTokens.FirstOrDefault(x => x.Token == refreshToken);

            if (oldToken != null && !oldToken.IsActive) return Unauthorized();

            if (oldToken != null) oldToken.Revoked = DateTime.UtcNow;

            return CreateUserObject(user);
        }

        private async Task SetRefreshToken(AppUser user)
        {
            var refreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshTokens.Add(refreshToken);

            await _userManager.UpdateAsync(user);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);
        }

        private UserDto CreateUserObject(AppUser user)
        {
            return new UserDto
            {
                DisplayName = user.DisplayName,
                Token = _tokenService.CreateToken(user),
                Username = user.UserName,
                Image = user?.Photos?.FirstOrDefault(x => x.IsMain)?.Url
            };
        }
    }

}
