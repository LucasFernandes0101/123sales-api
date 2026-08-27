using _123vendas.Application.DTOs.Carts;
using _123vendas.Application.DTOs.Common;
using _123vendas.Application.Mappers.Carts;
using _123vendas.Domain.Base;
using _123vendas.Domain.Interfaces.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace _123vendas_server.v1.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Tags("Carts")]
[Produces("application/json")]
public class CartsController(ICartService cartService) : ControllerBase
{

    /// <summary>
    /// Retrieves a paginated list of carts based on the provided filters.
    /// </summary>
    /// <param name="request">The filter parameters for the cart list.</param>
    /// <returns>A paginated response with the list of carts.</returns>
    /// <response code="200">Returns the paginated list of carts.</response>
    /// <response code="204">If no carts match the filter criteria.</response>
    /// <response code="400">If the request parameters are invalid.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponseDTO<CartGetResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponseDTO<CartGetResponseDTO>>> GetAsync(
        [FromQuery] CartGetRequestDTO request,
        CancellationToken cancellationToken)
    {
        var pagedResult = await cartService.GetAllAsync(
            request.Id,
            request.UserId,
            request.MinDate,
            request.MaxDate,
            request.Page,
            request.Size,
            request.OrderByClause,
            cancellationToken);

        if (pagedResult?.Items is not null && pagedResult.Items.Count > 0)
            return Ok(
                new PagedResponseDTO<CartGetResponseDTO>(
                    pagedResult.Items.ToDTO(),
                    pagedResult.Total,
                    request.Page,
                    request.Size));

        return NoContent();
    }

    /// <summary>
    /// Retrieves detailed information of a specific cart by its ID.
    /// </summary>
    /// <param name="id">The ID of the cart.</param>
    /// <returns>The detailed cart information.</returns>
    /// <response code="200">Returns the cart details.</response>
    /// <response code="404">If the cart is not found.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CartGetDetailResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartGetDetailResponseDTO>> GetAsync(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var cart = await cartService.GetByIdAsync(id, cancellationToken);

        if (cart is null)
            return NotFound();

        var response = cart.ToDetailDTO();

        return Ok(response);
    }

    /// <summary>
    /// Creates a new shopping cart.
    /// </summary>
    /// <param name="request">The data for the new cart.</param>
    /// <returns>The created cart with response data.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CartPostResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CartPostResponseDTO>> PostAsync(
        [FromBody] CartPostRequestDTO request,
        CancellationToken cancellationToken)
    {
        var createdCart = await cartService.CreateAsync(request.ToEntity(), cancellationToken);

        var response = createdCart.ToPostResponseDTO();

        return Created(string.Empty, response);
    }

    /// <summary>
    /// Updates the details of an existing cart by its ID.
    /// </summary>
    /// <param name="id">The ID of the cart to update.</param>
    /// <param name="request">The updated cart data.</param>
    /// <returns>The updated cart with response data.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CartPutResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartPutResponseDTO>> PutAsync(
        [FromRoute] int id,
        [FromBody] CartPutRequestDTO request,
        CancellationToken cancellationToken)
    {
        var cart = await cartService.UpdateAsync(id, request.ToEntity(), cancellationToken);

        return Ok(cart.ToPutResponseDTO());
    }

    /// <summary>
    /// Deletes a cart by its ID.
    /// </summary>
    /// <param name="id">The ID of the cart to delete.</param>
    /// <returns>No content if deletion is successful.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync([FromRoute] int id, CancellationToken cancellationToken)
    {
        await cartService.DeleteAsync(id, cancellationToken);

        return NoContent();
    }
}