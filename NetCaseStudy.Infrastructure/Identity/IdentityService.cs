using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using NetCaseStudy.Application.Abstractions;
using NetCaseStudy.Application.Localization;

namespace NetCaseStudy.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly TokenService _tokenService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public IdentityService(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        TokenService tokenService,
        IStringLocalizer<SharedResource> localizer)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _localizer = localizer;
    }

    public async Task<(bool Success, IEnumerable<string> Errors)> RegisterAsync(string email, string password, string role)
    {
        var user = new IdentityUser
        {
            UserName = email,
            Email = email
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            return (false, result.Errors.Select(e => e.Description));
        }

        if (!await _roleManager.RoleExistsAsync(role))
        {
            var roleResult = await _roleManager.CreateAsync(new IdentityRole(role));
            if (!roleResult.Succeeded)
            {
                return (false, roleResult.Errors.Select(e => e.Description));
            }
        }

        var addRoleResult = await _userManager.AddToRoleAsync(user, role);
        if (!addRoleResult.Succeeded)
        {
            return (false, addRoleResult.Errors.Select(e => e.Description));
        }

        return (true, Enumerable.Empty<string>());
    }

    public async Task<(bool Success, string? Token, IEnumerable<string> Errors)> LoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return (false, null, new[] { _localizer["UserNotFound"].Value });
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
        if (!result.Succeeded)
        {
            return (false, null, new[] { _localizer["InvalidCredentials"].Value });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.GenerateToken(user, roles);
        return (true, token, Enumerable.Empty<string>());
    }

}
