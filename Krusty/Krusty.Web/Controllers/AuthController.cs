using Krusty.Web.Models;
using Krusty.Web.Service.IService;
using Krusty.Web.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Krusty.Web.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _authService;
    private readonly ITokenProvider _tokenProvider;

    public AuthController(IAuthService authService, ITokenProvider tokenProvider)
    {
        _authService = authService;
        _tokenProvider = tokenProvider;
    }
    [HttpGet]
    public IActionResult Login()
    {
        LoginRequestDTO loginRequestDTO = new();
        return View(loginRequestDTO);
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginRequestDTO loginReqDTO)
    {
        ResponseDto responseDto = await _authService.LoginAsync(loginReqDTO);

        if (responseDto != null && responseDto.IsSuccess)
        {
            LoginResponseDTO loginResponseDTO = 
                JsonConvert.DeserializeObject<LoginResponseDTO>(Convert.ToString(responseDto.Result));

            await SignInUserAsync(loginResponseDTO);
            _tokenProvider.SetToken(loginResponseDTO.Token);
            return RedirectToAction("Index", "Home");
        }
        else
        {
            TempData["error"] = responseDto.Message;
            return View(loginReqDTO);
        }

    }


    [HttpGet]
    public IActionResult Register()
    {
        var roleList = new List<SelectListItem>()
        {
            new SelectListItem{Text= SD.RoleAdmin, Value = SD.RoleAdmin},
            new SelectListItem{Text= SD.RoleCustomer, Value = SD.RoleCustomer}
        };
        ViewBag.RoleList = roleList;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegistrationRequestDTO registReqDTO)
    {
        ResponseDto result = await _authService.RegisterAsync(registReqDTO);
        ResponseDto assignRole;

        if(result != null && result.IsSuccess)
        {
            if (string.IsNullOrEmpty(registReqDTO.Role))
            {
                registReqDTO.Role = SD.RoleCustomer;
            }
            assignRole = await _authService.AssignRoleAsync(registReqDTO);
            if (assignRole != null && assignRole.IsSuccess)
            {
                TempData["success"] = "Registration Successful!";
                return RedirectToAction(nameof(Login));
            }
        }
        else
        {
            TempData["error"] = result.Message;
        }
        var roleList = new List<SelectListItem>()
        {
            new SelectListItem{Text= SD.RoleAdmin, Value = SD.RoleAdmin},
            new SelectListItem{Text= SD.RoleCustomer, Value = SD.RoleCustomer}
        };
        ViewBag.RoleList = roleList;
        return View(registReqDTO);
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        _tokenProvider.ClearToken();
        return RedirectToAction("Index","Home");
    }

    private async Task SignInUserAsync(LoginResponseDTO loginResponseDTO)
    {
        // extracting the claims

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(loginResponseDTO.Token);
        var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
        identity.AddClaim(new Claim(JwtRegisteredClaimNames.Email,
            jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Email).Value));

        identity.AddClaim(new Claim(JwtRegisteredClaimNames.Sub,
            jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Sub).Value));

        identity.AddClaim(new Claim(JwtRegisteredClaimNames.Name,
            jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Name).Value));

        identity.AddClaim(new Claim(ClaimTypes.Name,
            jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Email).Value));

        identity.AddClaim(new Claim(ClaimTypes.Role,
            jwt.Claims.FirstOrDefault(u => u.Type == "role").Value));

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    }

}
