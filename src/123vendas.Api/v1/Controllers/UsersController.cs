using _123vendas.Application.Commands.Users;
using _123vendas.Application.DTOs.Common;
using _123vendas.Application.DTOs.Users;
using _123vendas.Application.Mappers.Users;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace _123vendas_server.v1.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Tags("Users")]
[Produces("application/json")]
public class UsersController(IMediator mediator) : ControllerBase
{

    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="dto">The user data for creating a new user.</param>
    /// <returns>The created user's data.</returns>
    /// <response code="201">Returns the created user data.</response>
    /// <response code="400">If the request is invalid or missing required fields.</response>
    /// <response code="409">If a user with the same email already exists.</response>
    [HttpPost]
    [ProducesResponseType(typeof(UserPostResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserPostResponseDTO>> PostAsync(
        [FromBody] UserPostRequestDTO dto,
        CancellationToken cancellationToken)
    {
        var command = dto.ToCommand();

        var result = await mediator.Send(command, cancellationToken);

        var response = result?.ToPostResponse();

        return Created(string.Empty, response);
    }

    /// <summary>
    /// Retrieves a user by their ID.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>The user data.</returns>
    /// <response code="200">Returns the user data.</response>
    /// <response code="404">If the user is not found.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserGetResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserGetResponseDTO>> GetByIdAsync(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var command = new GetUserCommand(id);

        var result = await mediator.Send(command, cancellationToken);

        if (result is null)
            return NotFound();

        var response = result.ToGetResponse();


        return Ok(response);
    }

    /// <summary>
    /// Deletes a user by their ID. Only accessible by managers.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>No content if the user is successfully deleted.</returns>
    /// <response code="204">If the user was successfully deleted.</response>
    /// <response code="401">If the request is not authenticated.</response>
    /// <response code="403">If the user does not have the Manager role.</response>
    /// <response code="404">If the user is not found.</response>
    [Authorize(Policy = "ManagerOnly")]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> DeleteByIdAsync([FromRoute] int id, CancellationToken cancellationToken)
    {
        var command = new DeleteUserCommand(id);

        await mediator.Send(command, cancellationToken);

        return NoContent();
    }
}