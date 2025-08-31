using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetCaseStudy.Application.Abstractions;

public interface IIdentityService
{
    Task<(bool Success, IEnumerable<string> Errors)> RegisterAsync(string email, string password, string role);
    Task<(bool Success, string? Token, IEnumerable<string> Errors)> LoginAsync(string email, string password);
}