using System.Text.Json;
using BankMore.Transferencia.Application.Common;
using BankMore.Transferencia.Domain.Entities;
using BankMore.Transferencia.Domain.Enums;
using BankMore.Transferencia.Domain.Interfaces;
using MediatR;
using TransferenciaEntity = BankMore.Transferencia.Domain.Entities.Transferencia;
namespace BankMore.Transferencia.Application.Commands.EfetuarTransferencia;

public record EfetuarTransferenciaCommand(
    string IdRequisicao,
    string NumeroContaOrigem,
    string NumeroContaDestino,
    decimal Valor,
    string Token) : IRequest;

public class EfetuarTransferenciaHandler : IRequestHandler<EfetuarTransferenciaCommand>
{
    private readonly ITransferenciaRepository _transferenciaRepository;
    private readonly IIdempotenciaRepository _idempotenciaRepository;
    private readonly IContaCorrenteClient _contaCorrenteClient;

    public EfetuarTransferenciaHandler(
        ITransferenciaRepository transferenciaRepository,
        IIdempotenciaRepository idempotenciaRepository,
        IContaCorrenteClient contaCorrenteClient)
    {
        _transferenciaRepository = transferenciaRepository;
        _idempotenciaRepository = idempotenciaRepository;
        _contaCorrenteClient = contaCorrenteClient;
    }

    public async Task Handle(EfetuarTransferenciaCommand request, CancellationToken cancellationToken)
    {
        var idempotencia = await _idempotenciaRepository.ObterPorChaveAsync(request.IdRequisicao);
        if (idempotencia is not null)
            return;

        if (request.Valor <= 0)
            throw new DomainException("Apenas valores positivos são aceitos.", TipoFalha.INVALID_VALUE);

        if (request.NumeroContaOrigem == request.NumeroContaDestino)
            throw new DomainException("Conta de origem e destino não podem ser iguais.", TipoFalha.INVALID_ACCOUNT);

        var requisicaoJson = JsonSerializer.Serialize(new
        {
            request.IdRequisicao,
            request.NumeroContaOrigem,
            request.NumeroContaDestino,
            request.Valor
        });

        var idDebito = $"{request.IdRequisicao}-debito";
        try
        {
            await _contaCorrenteClient.MovimentarAsync(
                idDebito,
                request.NumeroContaOrigem,
                request.Valor,
                "D",
                request.Token);
        }
        catch (Exception ex)
        {
            throw new DomainException(
                $"Falha ao debitar conta de origem: {ex.Message}",
                TipoFalha.TRANSFER_ERROR);
        }

        var idCredito = $"{request.IdRequisicao}-credito";
        try
        {
            await _contaCorrenteClient.MovimentarAsync(
                idCredito,
                request.NumeroContaDestino,
                request.Valor,
                "C",
                request.Token);
        }
        catch (Exception ex)
        {
            var idEstorno = $"{request.IdRequisicao}-estorno";
            try
            {
                await _contaCorrenteClient.MovimentarAsync(
                    idEstorno,
                    request.NumeroContaOrigem,
                    request.Valor,
                    "C",
                    request.Token);
            }
            catch
            {
            }

            throw new DomainException(
                $"Falha ao creditar conta de destino. Estorno realizado: {ex.Message}",
                TipoFalha.TRANSFER_ERROR);
        }

        var transferencia = new TransferenciaEntity(
        request.IdRequisicao,
        request.NumeroContaOrigem,
        request.NumeroContaDestino,
        request.Valor);

        await _transferenciaRepository.InserirAsync(transferencia);
        await _idempotenciaRepository.InserirAsync(new Idempotencia(
            request.IdRequisicao,
            requisicaoJson,
            JsonSerializer.Serialize(new { sucesso = true })));
    }
}
