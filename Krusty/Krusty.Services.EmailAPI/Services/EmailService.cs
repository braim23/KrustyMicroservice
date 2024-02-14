using Krusty.Services.EmailAPI.Data;
using Krusty.Services.EmailAPI.Models;
using Krusty.Services.EmailAPI.Models.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using System.Text;

namespace Krusty.Services.EmailAPI.Services;

public class EmailService : IEmailService
{
    private DbContextOptions<AppDbContext> _dbOptions;

    public EmailService(DbContextOptions<AppDbContext> options)
    {
        _dbOptions = options;
    }

    public async Task EmailCartAndLog(CartDto cartDto)
    {
        StringBuilder message = new StringBuilder();

        message.AppendLine("<br/>Cart Email Requested");
        message.AppendLine("<br/>Total " + cartDto.CartHeaderDto.CartTotal);
        message.Append("<br/>");
        message.Append("<ul>");
        foreach(var item in cartDto.CartDetailsDto)
        {
            message.Append("<li>");
            message.Append(item.ProductDto.Name + " x " + item.Count);
            message.Append("</li>");
        }
        message.Append("</ul>");

        await LogAndEmail(message.ToString(), cartDto.CartHeaderDto.Email);
    }

    public async Task RegisterUserEmailAndLog(string email)
    {
        string message = "User Registreation Successful. <br> Email: " + email;
        await LogAndEmail(message, "admin@mail.com");
    }

    private async Task<bool> LogAndEmail(string message, string email)
    {
        try
        {
            EmailLogger emailLogger = new()
            {
                Email = email,
                EmailSent = DateTime.Now,
                Message = message
            };
            await using var _db = new AppDbContext(_dbOptions);
            await _db.EmailLoggers.AddAsync(emailLogger);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

}
