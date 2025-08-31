using Microsoft.AspNetCore.Authorization;
using NetCaseStudy.Application.DTOs;
using NetCaseStudy.Application.Abstractions;

namespace NetCaseStudy.Api.Authorization;

public class OrderAuthorizationHandler
    : AuthorizationHandler<ViewOrderRequirement, OrderDto>
{
    private readonly ICurrentUserService _currentUser;
    public OrderAuthorizationHandler(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ViewOrderRequirement requirement,
        OrderDto resource)
    {
        var isAdmin = context.User.IsInRole("Admin");
        if (isAdmin || resource.UserId == _currentUser.UserId)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}