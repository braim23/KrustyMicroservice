﻿using Krusty.Services.EmailAPI.Models.Dto;

namespace Krusty.Services.EmailAPI.Services;

public interface IEmailService
{
    Task EmailCartAndLog(CartDto cartDto);
    Task RegisterUserEmailAndLog(string email);
}
