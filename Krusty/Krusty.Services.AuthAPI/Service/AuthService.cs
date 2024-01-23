using Krusty.Services.AuthAPI.Data;
using Krusty.Services.AuthAPI.Models;
using Krusty.Services.AuthAPI.Models.Dto;
using Krusty.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Identity;

namespace Krusty.Services.AuthAPI.Service;

public class AuthService : IAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(AppDbContext dbContext, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IJwtTokenGenerator jwtTokenGenerator)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<bool> AssignRole(string email, string roleName)
    {
        var user = _dbContext.ApplicationUsers.FirstOrDefault(u => u.UserName.ToLower() == email.ToLower());
        if(user != null)
        {
            if (!_roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult())
            {
                // create role if it does not exist
                _roleManager.CreateAsync(new IdentityRole(roleName)).GetAwaiter().GetResult();
            }
            await _userManager.AddToRoleAsync(user, roleName);
            return true;
        }
        return false;

    }

    public async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO)
    {
        var user = _dbContext.ApplicationUsers.FirstOrDefault(u=>u.UserName.ToLower() == loginRequestDTO.Username.ToLower());

        bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDTO.Password);
        if(user==null || isValid == false)
        {
            return new LoginResponseDTO() { UserDTO = null, Token = "" };
        }
        // if user was found, generate JWT Token
        var token = _jwtTokenGenerator.GenerateToken(user);
        UserDTO userDTO = new()
        {
            Email = user.Email,
            ID = user.Id,
            Name = user.Name,
            PhoneNumber = user.PhoneNumber
        };

        LoginResponseDTO loginResponseDTO = new LoginResponseDTO()
        {
            UserDTO = userDTO,
            Token = token
        };

        return loginResponseDTO;

    }

    public async Task<string> Register(RegistrationRequestDTO registrationRequestDTO)
    {
        ApplicationUser user = new()
        {
            UserName = registrationRequestDTO.Email,
            Email = registrationRequestDTO.Email,
            NormalizedEmail = registrationRequestDTO.Email.ToUpper(),
            Name = registrationRequestDTO.Name,
            PhoneNumber = registrationRequestDTO.PhoneNumber
        };

        try
        {
            var result = await _userManager.CreateAsync(user, registrationRequestDTO.Password);
            if (result.Succeeded)
            {
                var userToReturn = _dbContext.ApplicationUsers.First(u => u.UserName == registrationRequestDTO.Email);

                //map to user dto
                UserDTO userDto = new()
                {
                    Email = userToReturn.Email,
                    ID = userToReturn.Id,
                    Name = userToReturn.Name,
                    PhoneNumber = userToReturn.PhoneNumber
                };
                return "";
            }
            else
            {
                return result.Errors.FirstOrDefault().Description;
            }
        }
        catch (Exception ex)
        {

        }
        return "Error encountered";
    }
}
