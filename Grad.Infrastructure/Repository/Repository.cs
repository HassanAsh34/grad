//v3
using System.Linq.Expressions;
using Grad.Application.Common.Interfaces;
using  Grad.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;




namespace  Grad.Infrastructure.Repository 
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
			CancellationToken cancellationToken = default
		) where TEntity : class
		{ 
			try
			{
				return await _context.Set<TEntity>().Where(filter).ToListAsync(cancellationToken);
			}
			catch (OperationCanceledException)
			{
				//IEnumerable<TEntity> emptyList =;
				return new List<TEntity>();
			}
		}


		public async Task<TEntity?> GetEntityAsync<TEntity>(Expression<Func<TEntity, bool>>? filter = null,
			CancellationToken cancellationToken = default
		) where TEntity : class
		{
			try
			{
				return await _context.Set<TEntity>().Where(filter).FirstOrDefaultAsync(cancellationToken);
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

