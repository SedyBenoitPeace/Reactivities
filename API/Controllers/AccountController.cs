using System.Threading.Tasks;
using API.DTOs;
using API.Services;
using Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly TokenService _tokenService;
        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, TokenService tokenService)
        {
            this._tokenService = tokenService;
            this._signInManager = signInManager;
            this._userManager = userManager;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LogintDto logintDto)
        {
            var user = await _userManager.FindByEmailAsync(logintDto.Email);
            if (user == null)
                return Unauthorized();
            var result = await _signInManager.CheckPasswordSignInAsync(user, logintDto.Password, false);

            if (!result.Succeeded)
                return Unauthorized();

            return new UserDto
            {
                DisplayName = user.DisplayName,
                //Token = _signInManager.UserManager.GenerateUserToken(user, "token", user.Email),
                Token = _tokenService.CreateToken(user),
                Username = user.UserName,
                Image = null
                //Image = user.Photos.FirstOrDefault(x => x.IsMain)?.Url
            };
        }
    }

}
