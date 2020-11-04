using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AspNetCore.WebApi.Constants;
using AspNetCore.WebApi.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AspNetCore.WebApi.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleMananger;
        private readonly IConfiguration _configuration;

        public AccountController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            this._userManager = userManager;
            this._roleMananger = roleManager;
            this._configuration = configuration;
        }

        [Route("login")]
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel viewModel)
        {
            try
            {
                var user = await this._userManager.FindByNameAsync(viewModel.UserName);
                if (user != null && !await this._userManager.IsLockedOutAsync(user))
                {
                    if (await this._userManager.CheckPasswordAsync(user, viewModel.Password))
                    {
                        var userRoles = await this._userManager.GetRolesAsync(user);

                        var authClaims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, user.UserName),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                        };

                        foreach (var role in userRoles)
                        {
                            authClaims.Add(new Claim(ClaimTypes.Role, role));
                        }

                        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

                        var token = new JwtSecurityToken
                        (
                            issuer: this._configuration["JWT:ValidIssuer"],
                            audience: this._configuration["JWT:ValidAudience"],
                            expires: DateTime.Now.AddHours(3),
                            claims: authClaims,
                            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                        );

                        return Ok(new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(token),
                            expiration = token.ValidTo
                        });
                    }
                }

                return StatusCode(
                    StatusCodes.Status400BadRequest,
                    new BaseResponse<object>
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Your account is incorrect!"
                    });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new BaseResponse<object>
                    {
                        Code = StatusCodes.Status500InternalServerError,
                        Message = ex.ToString()
                    });
            }
        }

        [Route("register")]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel viewModel)
        {
            try
            {
                var user = await this._userManager.FindByNameAsync(viewModel.UserName);

                if (user == null)
                {
                    user = new IdentityUser
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserName = viewModel.UserName,
                        Email = viewModel.UserName
                    };

                    var result = await this._userManager.CreateAsync(user, viewModel.Password);

                    if (result.Succeeded)
                    {
                        if (!await this._roleMananger.RoleExistsAsync(UserRoles.Admin))
                        {
                            await this._roleMananger.CreateAsync(new IdentityRole(UserRoles.Admin));
                        }

                        if (!await this._roleMananger.RoleExistsAsync(UserRoles.SuperAdmin))
                        {
                            await this._roleMananger.CreateAsync(new IdentityRole(UserRoles.SuperAdmin));
                        }

                        await this._userManager.AddToRoleAsync(user, UserRoles.Admin);

                        return Ok(new BaseResponse<object>
                        {
                            Code = StatusCodes.Status200OK,
                            Message = "You have created new account!"
                        });
                    }
                    else
                    {
                        return StatusCode(
                            StatusCodes.Status500InternalServerError,
                            new BaseResponse<object>
                            {
                                Code = StatusCodes.Status500InternalServerError,
                                Message = "User creation failed! Please check user details and try again."
                            });
                    }
                }
                else
                {
                    return BadRequest(new BaseResponse<object>
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "User already exists!"
                    });
                }

            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new BaseResponse<object>
                    {
                        Code = StatusCodes.Status500InternalServerError,
                        Message = ex.ToString()
                    });
            }
        }
    }
}