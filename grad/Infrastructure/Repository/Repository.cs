//v3
using System.Linq.Expressions;
using Grad_Structured.Infrastructure.Persistence;
using Grad_Structured.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace Grad_Structured.Infrastructure.Repository
{
	public class Repository : IRepository
	{
		private readonly Db_Context _context;

		public Repository(Db_Context context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
		}

		//public async Task AddAsync<TEntity>(
		//TEntity entity,
		//CancellationToken cancellationToken = default
		//) where TEntity : class
		//{
		//	await _context.Set<TEntity>().AddAsync(entity, cancellationToken);
		//}

		public async Task<IEnumerable<TEntity>> GetEntitiesAsync<TEntity>(
			Expression<Func<TEntity, bool>>? filter = null,
			Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
			CancellationToken cancellationToken = default
		) where TEntity : class
		{
			IQueryable<TEntity> query = _context.Set<TEntity>();

			if (filter != null)
				query = query.Where(filter);

			// Apply include expression if provided
			if (include != null)
				query = include(query);

			
			try
			{
				return await query.ToListAsync(cancellationToken);
			}
			catch (OperationCanceledException)
			{
				//IEnumerable<TEntity> emptyList =;
				return new List<TEntity>();
			}
		}


		public async Task<TEntity?> GetEntityAsync<TEntity>(Expression<Func<TEntity, bool>>? filter = null, Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
			CancellationToken cancellationToken = default
		) where TEntity : class
		{
			IQueryable<TEntity> query = _context.Set<TEntity>();

			if (filter != null)
				query = query.Where(filter);

			if (include != null)
				query = include(query);
			try
			{
				return await query.FirstOrDefaultAsync(cancellationToken);
			}
			catch (OperationCanceledException)
			{
				return null;
			}
		}

		//public async Task CreateEntityAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class
		//{
		//	//await _context.Set<TEntity>().AddAsync(entity, cancellationToken);
		//	_context.Set<TEntity>().Add(entity);
		//}
		public void CreateEntityAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class 
		{
			_context.Set<TEntity>().Add(entity);
		}
		public void UpdateEntityAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class
		{
			_context.Set<TEntity>().Update(entity);
		}

		public void DeleteEntityAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class
		{
			_context.Set<TEntity>().Remove(entity);
		}
	}
}

