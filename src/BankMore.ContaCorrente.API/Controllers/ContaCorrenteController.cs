using BankMore.ContaCorrente.Application.Commands.CadastrarConta;
using BankMore.ContaCorrente.Application.Commands.InativarConta;
using BankMore.ContaCorrente.Application.Commands.MovimentarConta;
using BankMore.ContaCorrente.Application.Queries.ConsultarSaldo;
using BankMore.ContaCorrente.Application.Queries.Login;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankMore.ContaCorrente.API.Controllers;

[ApiController]
[Route("api/conta-corrente")]
[Produces("application/json")]
public class ContaCorrenteController : ControllerBase
{
    private readonly IMediator _mediator;

    public ContaCorrenteController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private string NumeroContaLogado =>
        User.Claims.FirstOrDefault(c => c.Type == "numeroConta")?.Value ?? "";

    /// <summary>Cadastra uma nova conta corrente</summary>
    /// <response code="200">Conta criada com sucesso, retorna o número da conta</response>
    /// <response code="400">CPF inválido — tipo: INVALID_DOCUMENT</response>
    [HttpPost("cadastrar")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CadastrarContaResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Cadastrar([FromBody] CadastrarContaRequest request)
    {
        var command = new CadastrarContaCommand(request.Cpf, request.Nome, request.Senha);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>Efetua login retornando token JWT</summary>
    /// <response code="200">Login efetuado com sucesso</response>
    /// <response code="401">Credenciais inválidas — tipo: USER_UNAUTHORIZED</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var query = new LoginQuery(request.Identificador, request.Senha);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>Inativa a conta corrente do usuário logado</summary>
    /// <response code="204">Conta inativada com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="403">Token inválido ou expirado</response>
    [HttpPatch("inativar")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Inativar([FromBody] InativarContaRequest request)
    {
        var command = new InativarContaCommand(NumeroContaLogado, request.Senha);
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>Registra uma movimentação (crédito ou débito) na conta corrente</summary>
    /// <response code="204">Movimentação registrada com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="403">Token inválido ou expirado</response>
    [HttpPost("movimentar")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Movimentar([FromBody] MovimentarContaRequest request)
    {
        var command = new MovimentarContaCommand(
            request.IdRequisicao,
            NumeroContaLogado,
            request.NumeroConta,
            request.Valor,
            request.Tipo);
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>Consulta o saldo da conta corrente do usuário logado</summary>
    /// <response code="200">Saldo retornado com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="403">Token inválido ou expirado</response>
    [HttpGet("saldo")]
    [Authorize]
    [ProducesResponseType(typeof(ConsultarSaldoResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Saldo()
    {
        var query = new ConsultarSaldoQuery(NumeroContaLogado);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}


/// <summary>Dados para cadastro de conta corrente</summary>
public record CadastrarContaRequest(
    /// <example>12345678909</example>
    string Cpf,
    /// <example>Ana Silva</example>
    string Nome,
    /// <example>Senha@123</example>
    string Senha);

/// <summary>Dados para login</summary>
public record LoginRequest(
    /// <example>12345678 ou 12345678909</example>
    string Identificador,
    /// <example>Senha@123</example>
    string Senha);

/// <summary>Dados para inativar a conta</summary>
public record InativarContaRequest(
    /// <example>Senha@123</example>
    string Senha);

/// <summary>Dados para movimentação</summary>
public record MovimentarContaRequest(
    /// <example>req-001</example>
    string IdRequisicao,
    /// <example>null</example>
    string? NumeroConta,
    /// <example>100.00</example>
    decimal Valor,
    /// <example>C</example>
    string Tipo);