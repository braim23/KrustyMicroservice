using Krusty.Services.AuthAPI.Models.Dto;
using Krusty.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Mvc;

namespace Krusty.Services.AuthAPI.Controllers;
[Route("api/auth")]
[ApiController]
public class AuthAPIController : Controller
{
    private readonly IAuthService _authService;
    protected ResponseDTO _resposeDto;

    public AuthAPIController(IAuthService authService)
    {
        _authService = authService;
        _resposeDto = new();
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegistrationRequestDTO registReqDto)
    {
        var response = await _authService.Register(registReqDto);
        if(!string.IsNullOrEmpty(response))
        {
            _resposeDto.IsSuccess = false;
            _resposeDto.Message = response;
            return BadRequest(response);
        }

        return Ok(_resposeDto);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginRequestDTO)
    {
        var loginResponse = await _authService.Login(loginRequestDTO);
        if(loginResponse.UserDTO == null)
        {
            _resposeDto.IsSuccess=false;
            _resposeDto.Message = "Username of password is incorrect!";
            return BadRequest(_resposeDto);
        }
        _resposeDto.Result = loginResponse;
        return Ok(_resposeDto);
    }

    [HttpPost("AssignRole")]
    public async Task<IActionResult> AssignRole([FromBody] RegistrationRequestDTO registReqDto)
    {
        var assignRoleSuccessful= await _authService.AssignRole(registReqDto.Email, registReqDto.Role.ToUpper());
        if (!assignRoleSuccessful)
        {
            _resposeDto.IsSuccess = false;
            _resposeDto.Message = "Error encounted when assigning role.";
            return BadRequest(_resposeDto);
        }
        return Ok(_resposeDto);
    }
}
