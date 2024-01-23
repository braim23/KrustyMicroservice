﻿using Microsoft.AspNetCore.Identity;

namespace Krusty.Services.AuthAPI.Models;

public class ApplicationUser :IdentityUser
{
    public string Name { get; set; }

}
