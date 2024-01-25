using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Krusty.Services.ProductAPI.Extensions;

public static class WebApplicaitonBuilderExtensions
{
    public static WebApplicationBuilder AddAppAuthentication(this WebApplicationBuilder builder)
    {
        // Another way of VVV
        //var settingsSection = builder.Configuration.GetSection("ApiSettings");
        //var secrett = builder.Configuration.GetValue<string>("Secret");


        var secret = builder.Configuration.GetValue<string>("ApiSettings:Secret");
        var issuer = builder.Configuration.GetValue<string>("ApiSettings:Issuer");
        var audience = builder.Configuration.GetValue<string>("ApiSettings:Audience");

        var key = Encoding.ASCII.GetBytes(secret);

        builder.Services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(x =>
        {
            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                ValidateAudience = true
            };
        });

        return builder;
    }
}
