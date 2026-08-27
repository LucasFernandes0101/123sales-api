using _123vendas.Application.DTOs.Common;
using _123vendas.Application.DTOs.Sales;
using _123vendas.Application.Mappers.Sales;
using _123vendas.Domain.Base;
using _123vendas.Domain.Interfaces.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace _123vendas_server.v1.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Tags("Sales")]
[Produces("application/json")]
public class SalesController(ISaleService saleService) : ControllerBase
{

    /// <summary>
    /// Retrieves a paginated list of sales based on query parameters.
    /// </summary>
    /// <param name="request">The request containing query parameters for filtering and pagination.</param>
    /// <returns>A paginated response containing the list of sales.</returns>
    /// <response code="200">Returns the paginated list of sales.</response>
    /// <response code="204">If no sales match the filter criteria.</response>
    /// <response code="400">If the request parameters are invalid.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponseDTO<SaleGetResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponseDTO<SaleGetResponseDTO>>> GetAsync(
        [FromQuery] SaleGetRequestDTO request,
        CancellationToken cancellationToken)
    {
        var pagedResult = await saleService.GetAllAsync(
            request.Id,
            request.BranchId,
            request.UserId,
            request.Status,
            request.StartDate,
            request.EndDate,
            request.Page,
            request.Size,
            request.OrderByClause,
            cancellationToken);

        if (pagedResult?.Items is not null && pagedResult.Items.Count > 0)
            return Ok(
                new PagedResponseDTO<SaleGetResponseDTO>(
                    pagedResult.Items.ToDTO(),
                    pagedResult.Total,
                    request.Page,
                    request.Size));

        return NoContent();
    }

    /// <summary>
    /// Retrieves the details of a specific sale by its ID.
    /// </summary>
    /// <param name="id">The ID of the sale to retrieve.</param>
    /// <returns>The details of the specified sale.</returns>
    /// <response code="200">Returns the sale details.</response>
    /// <response code="404">If the sale is not found.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SaleGetDetailResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SaleGetDetailResponseDTO>> GetAsync(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var sale = await saleService.GetByIdAsync(id, cancellationToken);

        if (sale is null)
            return NotFound();

        var response = sale.ToDetailDTO();

        return Ok(response);
    }

    /// <summary>
    /// Creates a new sale. Only accessible to users with "Manager" policy.
    /// </summary>
    /// <param name="request">The request containing the sale data to be created.</param>
    /// <returns>The response containing the details of the created sale.</returns>
    [Authorize(Policy = "ManagerOnly")]
    [HttpPost]
    [ProducesResponseType(typeof(SalePostResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SalePostResponseDTO>> PostAsync(
        [FromBody] SalePostRequestDTO request,
        CancellationToken cancellationToken)
    {
        var createdSale = await saleService.CreateAsync(request.ToEntity(), cancellationToken);

        var response = createdSale.ToPostResponseDTO();

        return Created(string.Empty, response);
    }

    /// <summary>
    /// Updates an existing sale. Only accessible to users with "Manager" policy.
    /// </summary>
    /// <param name="id">The ID of the sale to update.</param>
    /// <param name="request">The request containing the updated sale data.</param>
    /// <returns>The response containing the updated sale details.</returns>
    [Authorize(Policy = "ManagerOnly")]
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(SalePutResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SalePutResponseDTO>> PutAsync(
        [FromRoute] int id,
        [FromBody] SalePutRequestDTO request,
        CancellationToken cancellationToken)
    {
        var sale = await saleService.UpdateAsync(id, request.ToEntity(), cancellationToken);

        return Ok(sale.ToPutResponseDTO());
    }

    /// <summary>
    /// Deletes a sale by its ID. Only accessible to users with "Manager" policy.
    /// </summary>
    /// <param name="id">The ID of the sale to delete.</param>
    /// <returns>No content response if the deletion is successful.</returns>
    [Authorize(Policy = "ManagerOnly")]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteAsync([FromRoute] int id, CancellationToken cancellationToken)
    {
        await saleService.DeleteAsync(id, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Cancels an entire sale by its ID.
    /// </summary>
    /// <param name="id">The ID of the sale to cancel.</param>
    /// <returns>No content response if the cancellation is successful.</returns>
    /// <response code="204">If the sale was successfully cancelled.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="404">If the sale is not found.</response>
    [HttpPut("{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelAsync(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        await saleService.CancelAsync(id, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Cancels a specific item within a sale by its sequence number.
    /// </summary>
    /// <param name="id">The ID of the sale to which the item belongs.</param>
    /// <param name="sequence">The sequence number of the item to cancel.</param>
    /// <returns>The details of the sale after the item cancellation.</returns>
    /// <response code="200">Returns the sale details after item cancellation.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="404">If the sale or item is not found.</response>
    [HttpPut("{id}/Items/{sequence}/cancel")]
    [ProducesResponseType(typeof(SaleGetDetailResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SaleGetDetailResponseDTO>> CancelItemAsync(
        [FromRoute] int id,
        [FromRoute] int sequence,
        CancellationToken cancellationToken)
    {
        var sale = await saleService.CancelItemAsync(id, sequence, cancellationToken);

        return Ok(sale.ToDetailDTO());
    }

    /// <summary>
    /// Retrieves the details of a specific item within a sale by its sequence number.
    /// </summary>
    /// <param name="id">The ID of the sale to which the item belongs.</param>
    /// <param name="sequence">The sequence number of the item to retrieve.</param>
    /// <returns>The details of the specified item.</returns>
    /// <response code="200">Returns the sale item details.</response>
    /// <response code="404">If the sale or item is not found.</response>
    [HttpGet("{id}/Items/{sequence}")]
    [ProducesResponseType(typeof(SaleItemGetDetailDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SaleItemGetDetailDTO>> GetItemAsync(
        [FromRoute] int id,
        [FromRoute] int sequence,
        CancellationToken cancellationToken)
    {
        var saleItem = await saleService.GetItemAsync(id, sequence, cancellationToken);

        return Ok(saleItem.ToDetailDTO());
    }
}