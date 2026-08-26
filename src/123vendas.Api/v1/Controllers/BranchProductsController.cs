using _123vendas.Application.DTOs.BranchProducts;
using _123vendas.Application.DTOs.Common;
using _123vendas.Application.Mappers.BranchProducts;
using _123vendas.Domain.Base;
using _123vendas.Domain.Interfaces.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace _123vendas_server.v1.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class BranchProductsController : ControllerBase
{
    private readonly IBranchProductService _branchProductService;

    public BranchProductsController(IBranchProductService branchProductService)
    {
        _branchProductService = branchProductService;
    }

    /// <summary>
    /// Retrieves a list of branch products based on the provided filter criteria and pagination parameters.
    /// </summary>
    /// <param name="request">The filter and pagination parameters for retrieving branch products.</param>
    /// <returns>A paged list of branch products matching the filter criteria.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponseDTO<BranchProductGetResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<PagedResponseDTO<BranchProductGetResponseDTO>>> GetAsync([FromQuery] BranchProductGetRequestDTO request)
    {
        var pagedResult = await _branchProductService.GetAllAsync(request.Id,
                                                                  request.BranchId,
                                                                  request.ProductId,
                                                                  request.IsActive,
                                                                  request.StartDate,
                                                                  request.EndDate,
                                                                  request.Page,
                                                                  request.Size,
                                                                  request.OrderByClause);

        if (pagedResult?.Items is not null && pagedResult.Items.Any())
            return Ok(new PagedResponseDTO<BranchProductGetResponseDTO>(pagedResult.Items.ToDTO(), pagedResult.Total, request.Page, request.Size));

        return NoContent();
    }

    /// <summary>
    /// Retrieves the details of a specific branch product by its ID.
    /// </summary>
    /// <param name="id">The ID of the branch product to retrieve.</param>
    /// <returns>The details of the branch product.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BranchProductGetDetailResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BranchProductGetDetailResponseDTO>> GetAsync([FromRoute] int id)
    {
        var branchProduct = await _branchProductService.GetByIdAsync(id);

        if (branchProduct is null)
            return NotFound();

        var response = branchProduct.ToDetailDTO();

        return Ok(response);
    }

    /// <summary>
    /// Creates a new branch product.
    /// </summary>
    /// <param name="request">The details of the branch product to create.</param>
    /// <returns>The created branch product's response data.</returns>
    [Authorize(Policy = "ManagerOnly")]
    [HttpPost]
    [ProducesResponseType(typeof(BranchProductPostResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BranchProductPostResponseDTO>> PostAsync([FromBody] BranchProductPostRequestDTO request)
    {
        var createdBranchProduct = await _branchProductService.CreateAsync(request.ToEntity());

        var response = createdBranchProduct.ToPostResponseDTO();

        return Created(string.Empty, response);
    }

    /// <summary>
    /// Updates the details of an existing branch product.
    /// </summary>
    /// <param name="id">The ID of the branch product to update.</param>
    /// <param name="request">The updated branch product details.</param>
    /// <returns>The updated branch product's response data.</returns>
    [Authorize(Policy = "ManagerOnly")]
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(BranchProductPutResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BranchProductPutResponseDTO>> PutAsync([FromRoute] int id, [FromBody] BranchProductPutRequestDTO request)
    {
        var branchProduct = await _branchProductService.UpdateAsync(id, request.ToEntity());

        return Ok(branchProduct.ToPutResponseDTO());
    }

    /// <summary>
    /// Deletes a branch product by its ID.
    /// </summary>
    /// <param name="id">The ID of the branch product to delete.</param>
    /// <returns>No content if the branch product was successfully deleted.</returns>
    [Authorize(Policy = "ManagerOnly")]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        await _branchProductService.DeleteAsync(id);

        return NoContent();
    }
}