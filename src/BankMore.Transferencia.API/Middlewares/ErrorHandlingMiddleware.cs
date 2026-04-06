using System.Net;
using System.Text.Json;
using BankMore.Transferencia.Application.Common;

namespace BankMore.Transferencia.API.Middlewares;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Erro de domínio: {Mensagem}", ex.Message);

            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";

            var body = JsonSerializer.Serialize(new
            {
                mensagem = ex.Message,
                tipo = ex.TipoFalha.ToString()
            });

            await context.Response.WriteAsync(body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno inesperado");

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var body = JsonSerializer.Serialize(new { mensagem = "Erro interno do servidor." });
            await context.Response.WriteAsync(body);
        }
    }
}
