namespace Grad.API.Middleware
{
	/// <summary>
	/// Global exception-handling middleware.
	/// Catches any unhandled exception in the pipeline, logs full details
	/// via Serilog, and returns a clean 500 JSON response to the client.
	/// </summary>
	public class ExceptionHandlingMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<ExceptionHandlingMiddleware> _logger;

		public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
			catch (OperationCanceledException)
			{
				// Client cancelled the request — not a server error
				_logger.LogWarning(
					"Request cancelled by client: {Method} {Path}",
					context.Request.Method,
					context.Request.Path);

				context.Response.StatusCode = 499; // Client Closed Request
				await context.Response.WriteAsync("Request was cancelled.");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex,
					"Unhandled exception on {Method} {Path} | TraceId: {TraceId}",
					context.Request.Method,
					context.Request.Path,
					context.TraceIdentifier);

				if (!context.Response.HasStarted)
				{
					context.Response.StatusCode = StatusCodes.Status500InternalServerError;
					context.Response.ContentType = "application/json";

					var response = new
					{
						message = "An internal server error occurred.",
						traceId = context.TraceIdentifier
					};

					await context.Response.WriteAsJsonAsync(response);
				}
			}
		}
	}
}
