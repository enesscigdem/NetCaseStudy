using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NetCaseStudy.Application.Abstractions;
using NetCaseStudy.Application.DTOs;
using NetCaseStudy.Application.Features.Orders.Commands;
using NetCaseStudy.Application.Features.Orders.Queries;

namespace NetCaseStudy.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthorizationService _authorization;

    public OrdersController(IMediator mediator, ICurrentUserService currentUser, IAuthorizationService authorization)
    {
        _mediator = mediator;
        _currentUser = currentUser;
        _authorization = authorization;
    }
    
    [HttpGet]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<OrderDto>>> Get(
        int page = 1,
        int pageSize = 10)
    {
        var isAdmin = User.IsInRole("Admin");
        var userId = _currentUser.UserId;
        var query = new ListOrdersQuery(page, pageSize, userId, isAdmin);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _mediator.Send(new GetOrderByIdQuery(id));
        if (order is null) return NotFound();

        var authz = await _authorization.AuthorizeAsync(User, order, "ViewOrder");
        if (!authz.Succeeded) return Forbid();

        return Ok(order);
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [EnableRateLimiting("WritePolicy")]
    public async Task<ActionResult<int>> Create([FromBody] CreateOrderRequest request)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }
        var id = await _mediator.Send(new CreateOrderCommand(request, userId));
        return CreatedAtAction(nameof(GetById), new { id, version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0" }, id);
    }

    [HttpPut("{id:int}/cancel")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [EnableRateLimiting("WritePolicy")]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = _currentUser.UserId;
        var isAdmin = User.IsInRole("Admin");
        try
        {
            var success = await _mediator.Send(new CancelOrderCommand(id, userId ?? string.Empty, isAdmin));
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}