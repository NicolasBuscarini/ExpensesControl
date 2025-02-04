using CodeFlow.Data.Context.Package.Tracking;
using CodeFlow.Start.Package.WebTransfer.Base;
using CodeFlow.Start.Package.WebTransfer.Base.Response;
using ExpensesControl.Infrastructure.SqlServer.Persistence;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ExpensesControl.Application.Behavior;

public class CommandFailureBehavior<TRequest, TResponse>(ILogger<CommandFailureBehavior<TRequest, TResponse>> logger, SqlContext dbContext, IConfiguration config) : IPipelineBehavior<TRequest, TResponse>
	where TRequest : notnull
	where TResponse : BaseResponse, new()
{
	/// <summary>
	/// Indicates whether request and response logging is enabled.
	/// </summary>
	private readonly bool _isRequestResponseLoggingEnabled = config.GetValue("EnableRequestResponseLogging", false);

	/// <summary>
	/// Handles the request and logs/persists any failures that occur during its execution.
	/// </summary>
	/// <param name="request">The incoming request to handle.</param>
	/// <param name="next">The delegate to call the next behavior in the pipeline.</param>
	/// <param name="cancellationToken">A token to observe for cancellation.</param>
	/// <returns>The response from the next behavior or handler.</returns>
	public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		// Log HTTP request information if logging is enabled.
		if (_isRequestResponseLoggingEnabled)
		{
			logger.LogDebug("Handling request:\n" +
							"\tRequest: {@request}", request);
		}

		try
		{
			// Proceed to the next handler
			var response = await next();

			// Log HTTP response information if logging is enabled.
			if (_isRequestResponseLoggingEnabled)
			{
				logger.LogDebug("Request completed successfully:\n" +
								"\tResponse: {@response}", response);
			}

			return response;
		}
		catch (Exception ex)
		{
			var referenceId = Guid.NewGuid().ToString();
			logger.LogError(ex, "UnhandledException: {ExceptionType} - {Message}. {Log}", ex.GetType(), ex.Message, $"ReferenceId: {referenceId}");

			var commandName = typeof(TRequest).Name;
			var timestamp = DateTime.UtcNow;
			var errorDetails = ex.ToString();

			// Get the TraceId from the current activity or context
			var traceId = Activity.Current?.TraceId.ToString() ?? "NoTraceId";

			var commandFailure = new CommandFailure
			{
				Id = Guid.NewGuid(),
				CommandName = commandName,
				ErrorDetails = errorDetails,
				Timestamp = timestamp,
				RequestContent = request?.ToString(),
				TraceId = traceId
			};

			dbContext.Set<CommandFailure>().Add(commandFailure);
			await dbContext.SaveChangesAsync(cancellationToken);
			logger.LogInformation("CommandFailure saved to the database. ID: {Id}, TraceId: {TraceId}", commandFailure.Id, traceId);

			// Return a response with the error message
			var response = new TResponse();
			response.AddErrorMessage<TResponse>("Ocorreu um erro inesperado. Verifique os dados ou tente novamente mais tarde.", ErrorType.InternalError);
			response.TraceId = traceId;

			return response;
		}
	}
}