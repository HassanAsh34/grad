//v3
using System.Linq.Expressions;
using grad.Infrastructure.Persistence;
using grad.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace grad.Infrastructure.Repository
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

		public async void DeleteEntityAsync<TEntity>(TEntity entity=null, CancellationToken cancellationToken = default) where TEntity : class
		{
			_context.Set<TEntity>().Remove(entity);
		}
		
		public async Task<int> DleteEntitiesAsync<TEntity>(Expression<Func<TEntity, bool>>? filter, CancellationToken cancellationToken = default) where TEntity : class
		{
			return	await _context.Set<TEntity>().Where(filter).ExecuteDeleteAsync();	
		}
	}
}

