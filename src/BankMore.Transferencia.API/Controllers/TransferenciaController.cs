using BankMore.Transferencia.Application.Commands.EfetuarTransferencia;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankMore.Transferencia.API.Controllers;

[ApiController]
[Route("api/transferencia")]
[Produces("application/json")]
public class TransferenciaController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransferenciaController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private string NumeroContaLogado =>
        User.Claims.FirstOrDefault(c => c.Type == "numeroConta")?.Value ?? "";

    private string TokenBruto =>
        Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

    /// <summary>Efetua transferência entre contas da mesma instituição</summary>
    /// <response code="204">Transferência realizada com sucesso</response>
    /// <response code="400">Dados inválidos ou falha na operação</response>
    /// <response code="403">Token inválido ou expirado</response>
    [HttpPost("transferir")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Transferir([FromBody] TransferenciaRequest request)
    {
        var command = new EfetuarTransferenciaCommand(
            request.IdRequisicao,
            NumeroContaLogado,
            request.NumeroContaDestino,
            request.Valor,
            TokenBruto);

        await _mediator.Send(command);
        return NoContent();
    }
}

/// <summary>Dados para efetuar a transferência</summary>
public record TransferenciaRequest(
    /// <example>transf-001</example>
    string IdRequisicao,
    /// <example>12345678</example>
    string NumeroContaDestino,
    /// <example>150.00</example>
    decimal Valor);
