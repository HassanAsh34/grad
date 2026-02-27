using grad.Application.Common.Interfaces;
using grad.Infrastructure.Persistence;
using Microsoft.Extensions.Hosting;

public class SeedHostedService : IHostedService
{
	private readonly IServiceProvider _serviceProvider;

	public SeedHostedService(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		using var scope = _serviceProvider.CreateScope();

		var uow = scope.ServiceProvider.GetRequiredService<IUowServices>();
		var repository = scope.ServiceProvider.GetRequiredService<IRepository>();

		await DataSeeder.SeedAdminAsync(repository, uow);
	}

	public Task StopAsync(CancellationToken cancellationToken)
		=> Task.CompletedTask;
}
