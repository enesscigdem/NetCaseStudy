using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using NetCaseStudy.Application.Localization;

namespace NetCaseStudy.Infrastructure.Identity;

public class LocalizedIdentityErrorDescriber : IdentityErrorDescriber
{
    private readonly IStringLocalizer<SharedResource> _L;

    public LocalizedIdentityErrorDescriber(IStringLocalizer<SharedResource> localizer)
    {
        _L = localizer;
    }

    public override IdentityError DefaultError()
        => new() { Code = nameof(DefaultError), Description = _L["Identity_DefaultError"] };

    public override IdentityError PasswordTooShort(int length)
        => new() { Code = nameof(PasswordTooShort), Description = string.Format(_L["Identity_PasswordTooShort"], length) };

    public override IdentityError DuplicateEmail(string email)
        => new() { Code = nameof(DuplicateEmail), Description = string.Format(_L["Identity_DuplicateEmail"], email) };

    public override IdentityError DuplicateUserName(string userName)
        => new() { Code = nameof(DuplicateUserName), Description = string.Format(_L["Identity_DuplicateUserName"], userName) };
}