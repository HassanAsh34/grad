//v3
using System.Linq.Expressions;


namespace Grad.Application.Common.Interfaces
{
	public interface IRepository
	{
		Task<IEnumerable<TEntity>> GetEntitiesAsync<TEntity>(
			Expression<Func<TEntity, bool>>? filter = null,
			CancellationToken cancellationToken = default
		) where TEntity : class;

		Task<TEntity?> GetEntityAsync<TEntity>(Expression<Func<TEntity, bool>>? filter = null,
			CancellationToken cancellationToken = default
		) where TEntity : class;

		void CreateEntityAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class;

		void UpdateEntityAsync<TEntity>(
			TEntity entity,
			CancellationToken cancellationToken = default
		) where TEntity : class;

		void DeleteEntityAsync<TEntity>(
			TEntity entity = null,
			CancellationToken cancellationToken = default) where TEntity : class;

		Task<int> DeleteEntitiesAsync<TEntity>(
			Expression<Func<TEntity, bool>>? filter,
			CancellationToken cancellationToken = default) where TEntity : class;
	}
}

//v2
//using System.Linq.Expressions;
//using System.Threading;
//using grad.DTO;

//namespace grad.Interfaces
//{
//	public interface IRepository
//	{
//		Task<IEnumerable<TEntity>> GetEntitiesAsync<TEntity>(
//			Expression<Func<TEntity, bool>>? filter = null,
//			Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
//			CancellationToken cancellationToken = default
//		) where TEntity : class;

//		public Task<TEntity?> GetEntityAsync<TEntity>(Expression<Func<TEntity, bool>>? filter = null, Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
//			CancellationToken cancellationToken = default
//		) where TEntity : class;

//		public Task<object> CreateEntityAsync<TEntity>(
//			TEntity entity,
//			CancellationToken cancellationToken = default
//		) where TEntity : class;

//		public Task<bool> UpdateEntityAsync<TEntity>(
//			TEntity entity,
//			CancellationToken cancellationToken = default
//		) where TEntity : class;

//		public Task<bool> DeleteEntityAsync<TEntity>(
//			TEntity entity,
//			CancellationToken cancellationToken = default
//		) where TEntity : class;
//	}
//}
