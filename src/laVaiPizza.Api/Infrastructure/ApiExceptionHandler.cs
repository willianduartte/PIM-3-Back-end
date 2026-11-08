using laVaiPizza.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace laVaiPizza.Api.Infrastructure;

/// <summary>
/// Tratamento global de erros: toda excecao vira uma resposta ProblemDetails (RFC 7807).
/// Erros de negocio viram 400/404 com a mensagem; qualquer outro erro vira 500 generico
/// (o detalhe tecnico so aparece em Desenvolvimento e sempre vai para o log).
/// </summary>
public class ApiExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<ApiExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public ApiExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<ApiExceptionHandler> logger,
        IHostEnvironment environment)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, titulo, detalhe) = exception switch
        {
            NegocioException => (StatusCodes.Status400BadRequest, "Requisição inválida", exception.Message),
            NaoEncontradoException => (StatusCodes.Status404NotFound, "Não encontrado", exception.Message),
            AcessoNegadoException => (StatusCodes.Status403Forbidden, "Acesso negado", exception.Message),
            ConflitoException => (StatusCodes.Status409Conflict, "Conflito", exception.Message),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno do servidor",
                  _environment.IsDevelopment()
                      ? exception.Message
                      : "Ocorreu um erro inesperado. Tente novamente em instantes.")
        };

        if (status == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Erro nao tratado em {Metodo} {Caminho}", httpContext.Request.Method, httpContext.Request.Path);
        else
            _logger.LogInformation("{Titulo}: {Detalhe}", titulo, detalhe);

        httpContext.Response.StatusCode = status;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = titulo,
                Detail = detalhe
            }
        });
    }
}
