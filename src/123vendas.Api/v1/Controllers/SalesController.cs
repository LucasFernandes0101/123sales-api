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
public class SalesController(ISaleService saleService) : ControllerBase
{

    /// <summary>
    /// Retrieves a paginated list of sales based on query parameters.
    /// </summary>
    /// <param name="request">The request containing query parameters for filtering and pagination.</param>
    /// <returns>A paginated response containing the list of sales.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponseDTO<SaleGetResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<PagedResponseDTO<SaleGetResponseDTO>>> GetAsync([FromQuery] SaleGetRequestDTO request)
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
            request.OrderByClause);

        if (pagedResult?.Items is not null && pagedResult.Items.Count > 0)
            return Ok(new PagedResponseDTO<SaleGetResponseDTO>(pagedResult.Items.ToDTO(), pagedResult.Total, request.Page, request.Size));

        return NoContent();
    }

    /// <summary>
    /// Retrieves the details of a specific sale by its ID.
    /// </summary>
    /// <param name="id">The ID of the sale to retrieve.</param>
    /// <returns>The details of the specified sale.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SaleGetDetailResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SaleGetDetailResponseDTO>> GetAsync([FromRoute] int id)
    {
        var sale = await saleService.GetByIdAsync(id);

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
    public async Task<ActionResult<SalePostResponseDTO>> PostAsync([FromBody] SalePostRequestDTO request)
    {
        var createdSale = await saleService.CreateAsync(request.ToEntity());

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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SalePutResponseDTO>> PutAsync([FromRoute] int id, [FromBody] SalePutRequestDTO request)
    {
        var sale = await saleService.UpdateAsync(id, request.ToEntity());

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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        await saleService.DeleteAsync(id);

        return NoContent();
    }

    /// <summary>
    /// Cancels an entire sale by its ID.
    /// </summary>
    /// <param name="id">The ID of the sale to cancel.</param>
    /// <returns>No content response if the cancellation is successful.</returns>
    [HttpPut("{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CancelAsync([FromRoute] int id)
    {
        await saleService.CancelAsync(id);

        return NoContent();
    }

    /// <summary>
    /// Cancels a specific item within a sale by its sequence number.
    /// </summary>
    /// <param name="id">The ID of the sale to which the item belongs.</param>
    /// <param name="sequence">The sequence number of the item to cancel.</param>
    /// <returns>The details of the sale after the item cancellation.</returns>
    [HttpPut("{id}/Items/{sequence}/cancel")]
    [ProducesResponseType(typeof(SaleGetDetailResponseDTO), StatusCodes.Status200OK)]
    public async Task<ActionResult<SaleGetDetailResponseDTO>> CancelItemAsync([FromRoute] int id, [FromRoute] int sequence)
    {
        var sale = await saleService.CancelItemAsync(id, sequence);

        return Ok(sale.ToDetailDTO());
    }

    /// <summary>
    /// Retrieves the details of a specific item within a sale by its sequence number.
    /// </summary>
    /// <param name="id">The ID of the sale to which the item belongs.</param>
    /// <param name="sequence">The sequence number of the item to retrieve.</param>
    /// <returns>The details of the specified item.</returns>
    [HttpGet("{id}/Items/{sequence}")]
    [ProducesResponseType(typeof(SaleItemGetDetailDTO), StatusCodes.Status200OK)]
    public async Task<ActionResult<SaleItemGetDetailDTO>> GetItemAsync([FromRoute] int id, [FromRoute] int sequence)
    {
        var saleItem = await saleService.GetItemAsync(id, sequence);

        return Ok(saleItem.ToDetailDTO());
    }
}