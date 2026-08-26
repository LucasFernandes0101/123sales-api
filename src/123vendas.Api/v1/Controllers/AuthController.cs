using _123vendas.Application.DTOs.Auth;
using _123vendas.Application.DTOs.Common;
using _123vendas.Application.Mappers.Auth;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace _123vendas_server.v1.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class AuthController(IMediator mediator) : ControllerBase
{

    /// <summary>
    /// Authenticates a user with their credentials.
    /// </summary>
    /// <param name="request">The authentication request.</param>
    /// <returns>Authentication token if successful.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(AuthenticateUserResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticateUserResponseDTO>> AuthenticateUser([FromBody] AuthenticateUserRequestDTO request)
    {
        var command = request.ToCommand();

        var result = await mediator.Send(command);

        var response = result.ToResponseDTO();

        return Ok(response);
    }
}