using EventHorizon.DataAccess.Persistence;
using EventHorizon.Models.DTOs.Account;
using EventHorizon.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace EventHorizon.Controllers
{
    [ApiController]
    [Route("api/account")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> UserManager;
        private readonly IConfiguration Configuration; // allows you to access appsettings
        private readonly GeneralResponse _response;

        public AccountController(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _response = new GeneralResponse();
            UserManager = userManager;
            Configuration = configuration;
        }

        [HttpPost("register")]
        //[ProducesResponseType(StatusCodes.Status201Created)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GeneralResponse>> Register(RegisterAccountDTO accountFromRequest)
        {
            if(accountFromRequest == null)
            {
                _response.isSuccess = false;
                _response.statusCode = HttpStatusCode.BadRequest;
                return _response;
            }
            try
            {
                // Register
                ApplicationUser applicationUser = new ApplicationUser()
                {
                    UserName = accountFromRequest.UserName,
                    Email = accountFromRequest.Email
                };

                var result = await UserManager.CreateAsync(applicationUser, accountFromRequest.Password);

                if (result.Succeeded)
                {
                    // created successfully
                    await UserManager.AddToRoleAsync(applicationUser,RolesConstant.User);

                    _response.statusCode = HttpStatusCode.Created;
                    _response.isSuccess = true;
                    _response.Result = applicationUser;
                }
                else
                {
                    // failed to create
                    _response.statusCode = HttpStatusCode.BadRequest;
                    _response.isSuccess = false;
                    _response.Errors = new List<string>();

                    foreach (var error in result.Errors)
                        _response.Errors.Add(error.Description);          
                }
            }
            catch (Exception ex) 
            {
                _response.isSuccess = false;
                _response.statusCode = HttpStatusCode.InternalServerError;
                _response.Errors = new List<string>() { ex.ToString() };
            }

            return _response;
        }

        [HttpPost("login")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GeneralResponse>> Login(LoginDTO userFromRequest)
        {
            if (userFromRequest == null)
            {
                _response.isSuccess = false;
                _response.statusCode = HttpStatusCode.BadRequest;
                return _response;
            }

            try
            {
                // check
                ApplicationUser? userFromDb = await UserManager.FindByNameAsync(userFromRequest.UserName);
                if (userFromDb == null)
                {
                    _response.isSuccess = false;
                    _response.statusCode = HttpStatusCode.NotFound;
                    _response.Errors = new List<string>() { "User Not Found" };
                    return _response;
                }

                // validate password
                bool validPass = await UserManager.CheckPasswordAsync(userFromDb, userFromRequest.Password);
                if (validPass)
                {
                    // create JWT
                    List<Claim> claims = new List<Claim>();

                    // Token Generated ID change (JWT Predefined Claims)
                    claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, userFromDb.Id));
                    claims.Add(new Claim(ClaimTypes.Name, userFromDb.UserName));

                    var UserRoles = await UserManager.GetRolesAsync(userFromDb);

                    foreach (var role in UserRoles)
                        claims.Add(new Claim(ClaimTypes.Role, role));

                    var Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:SecretKey"]));

                    SigningCredentials signingCredentials = new SigningCredentials(Key, SecurityAlgorithms.HmacSha256);

                    JwtSecurityToken token = new JwtSecurityToken(
                            //issuer: Configuration["JWT:IssuerURL"],
                            //audience: Configuration["JWT:AudienceURL"],
                            expires: DateTime.Now.AddHours(2),
                            claims: claims,
                            signingCredentials: signingCredentials

                        );

                    _response.isSuccess = true;
                    _response.statusCode = HttpStatusCode.OK;
                    // Generate Token
                    _response.Result = new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                        expiration = DateTime.Now.AddHours(2)
                    };
                }
                else
                {
                    _response.isSuccess = false;
                    _response.statusCode = HttpStatusCode.BadRequest;
                    _response.Errors = new List<string>() { "Invalid UserName Or Password" };
                }
            }
            catch(Exception ex)
            {
                _response.isSuccess = false;
                _response.statusCode = HttpStatusCode.InternalServerError;
                _response.Errors = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [HttpPost("logout")]
        [Authorize]
        public ActionResult<GeneralResponse> Logout()
        {
            try
            {
                _response.isSuccess = true;
                _response.statusCode = HttpStatusCode.OK;
                _response.Result = new { message = "Logged out successfully" };
            }
            catch (Exception ex)
            {
                _response.isSuccess = false;
                _response.statusCode = HttpStatusCode.InternalServerError;
                _response.Errors = new List<string>() { ex.ToString() };
            }
            return _response;
        }
    }
}
