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
public class UsersController(IMediator mediator) : ControllerBase
{

    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="dto">The user data for creating a new user.</param>
    /// <returns>The created user's data.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(UserPostResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserPostResponseDTO>> PostAsync([FromBody] UserPostRequestDTO dto)
    {
        var command = dto.ToCommand();

        var result = await mediator.Send(command);

        var response = result?.ToPostResponse();

        return Created(string.Empty, response);
    }

    /// <summary>
    /// Retrieves a user by their ID.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>The user data.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserGetResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserGetResponseDTO>> GetByIdAsync(int id)
    {
        var command = new GetUserCommand(id);

        var result = await mediator.Send(command);

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
    [Authorize(Policy = "ManagerOnly")]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteByIdAsync(int id)
    {
        var command = new DeleteUserCommand(id);

        await mediator.Send(command);

        return NoContent();
    }
}