using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetCaseStudy.Api.Filters;
using NetCaseStudy.Application.DTOs;
using NetCaseStudy.Application.Features.Products.Commands;
using NetCaseStudy.Application.Features.Products.Queries;

namespace NetCaseStudy.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize]
    [ETagFilter] 
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ProductDto>>> Get(
        int page = 1,
        int pageSize = 10,
        string? search = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? sortBy = null,
        bool desc = false)
    {
        var query = new ListProductsQuery(page, pageSize, search, minPrice, maxPrice, sortBy, desc);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
    
    [HttpGet("cursor")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWithCursor(
        int pageSize = 20,
        string? cursor = null,
        string? search = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? sortBy = "id",
        bool desc = false)
    {
        var query = new ListProductsCursorQuery(pageSize, cursor, search, minPrice, maxPrice, sortBy, desc);
        var result = await _mediator.Send(query);
        return Ok(result);
    }


    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var product = await _mediator.Send(new GetProductByIdQuery(id));
        if (product is null)
        {
            return NotFound();
        }
        return Ok(product);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<int>> Create([FromBody] CreateProductRequest request)
    {
        var id = await _mediator.Send(new CreateProductCommand(request));
        return CreatedAtAction(nameof(GetById), new { id, version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0" }, id);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] CreateProductRequest request)
    {
        var success = await _mediator.Send(new UpdateProductCommand(id, request));
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _mediator.Send(new DeleteProductCommand(id));
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }
}