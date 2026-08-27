using _123vendas.Application.DTOs.Branches;
using _123vendas.Application.DTOs.Common;
using _123vendas.Application.Mappers.Branches;
using _123vendas.Domain.Base;
using _123vendas.Domain.Interfaces.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace _123vendas_server.v1.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Tags("Branches")]
[Produces("application/json")]
public class BranchesController(IBranchService branchService) : ControllerBase
{

    /// <summary>
    /// Retrieves a paginated list of branches based on the provided filter criteria.
    /// </summary>
    /// <param name="request">The filter and pagination parameters for retrieving branches.</param>
    /// <returns>A paged list of branches matching the filter criteria.</returns>
    /// <response code="200">Returns the paged list of branches.</response>
    /// <response code="204">If no branches match the filter criteria.</response>
    /// <response code="400">If the request parameters are invalid.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponseDTO<BranchGetResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponseDTO<BranchGetResponseDTO>>> GetAsync(
        [FromQuery] BranchGetRequestDTO request,
        CancellationToken cancellationToken)
    {
        var pagedResult = await branchService.GetAllAsync(
            request.Id,
            request.IsActive,
            request.Name,
            request.StartDate,
            request.EndDate,
            request.Page,
            request.Size,
            request.OrderByClause,
            cancellationToken);

        if (pagedResult?.Items is not null && pagedResult.Items.Count > 0)
            return Ok(
                new PagedResponseDTO<BranchGetResponseDTO>(
                    pagedResult.Items.ToDTO(),
                    pagedResult.Total,
                    request.Page,
                    request.Size));

        return NoContent();
    }

    /// <summary>
    /// Retrieves the details of a specific branch by its ID.
    /// </summary>
    /// <param name="id">The ID of the branch to retrieve.</param>
    /// <returns>The details of the branch.</returns>
    /// <response code="200">Returns the branch details.</response>
    /// <response code="404">If the branch is not found.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BranchGetDetailResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BranchGetDetailResponseDTO>> GetAsync(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var branch = await branchService.GetByIdAsync(id, cancellationToken);

        if (branch is null)
            return NotFound();

        var response = branch.ToDetailDTO();

        return Ok(response);
    }

    /// <summary>
    /// Creates a new branch.
    /// </summary>
    /// <param name="request">The details of the branch to create.</param>
    /// <returns>The created branch's response data.</returns>
    [Authorize(Policy = "ManagerOnly")]
    [HttpPost]
    [ProducesResponseType(typeof(BranchPostResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<BranchPostResponseDTO>> PostAsync(
        [FromBody] BranchPostRequestDTO request,
        CancellationToken cancellationToken)
    {
        var createdBranch = await branchService.CreateAsync(request.ToEntity(), cancellationToken);

        var response = createdBranch.ToPostResponseDTO();

        return Created(string.Empty, response);
    }

    /// <summary>
    /// Updates the details of an existing branch.
    /// </summary>
    /// <param name="id">The ID of the branch to update.</param>
    /// <param name="request">The updated branch details.</param>
    /// <returns>The updated branch's response data.</returns>
    [Authorize(Policy = "ManagerOnly")]
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(BranchPutResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> PutAsync(
        [FromRoute] int id,
        [FromBody] BranchPutRequestDTO request,
        CancellationToken cancellationToken)
    {
        var branch = await branchService.UpdateAsync(id, request.ToEntity(), cancellationToken);

        return Ok(branch.ToPutResponseDTO());
    }

    /// <summary>
    /// Deletes a branch by its ID.
    /// </summary>
    /// <param name="id">The ID of the branch to delete.</param>
    /// <returns>No content if the branch was successfully deleted.</returns>
    [Authorize(Policy = "ManagerOnly")]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteAsync([FromRoute] int id, CancellationToken cancellationToken)
    {
        await branchService.DeleteAsync(id, cancellationToken);

        return NoContent();
    }
}