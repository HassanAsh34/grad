//v3
using System.Linq.Expressions;
using grad.Interfaces;
using grad.DTO;
using grad.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using StackExchange.Redis;

namespace grad.Repositories
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

			return await query.ToListAsync(cancellationToken);
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

			return await query.FirstOrDefaultAsync(cancellationToken);
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







//using System.Linq.Expressions;
//using grad.Interfaces;
//using grad.DTO;
//using grad.Data;
//using Microsoft.EntityFrameworkCore;
//namespace grad.Repositories
//{
//	public class Repository : IRepository
//	{

//		private readonly Db_Context _context;
//		public Repository(Db_Context context)
//		{
//			_context = context ?? throw new ArgumentNullException(nameof(context));
//		}
//		public Task<object> CreateEntityAsync<TEntity>(TEntity entity) where TEntity : class
//		{
//			if (entity == null)
//				return Task.FromResult<object>(new ResultDTO
//				{
//					ErrCode = 400,
//					ErrMessage = "Entity is null"
//				});

//			else 
//			{
//				_context.Set<TEntity>().Add(entity);
//				_context.SaveChanges();
//				return Task.FromResult<object>(entity);
//			}

//		}

//		public async Task<object> DeleteEntityAsync<TEntity>(TEntity entity) where TEntity : class
//		{
//			if (entity == null)
//				return Task.FromResult<object>(new ResultDTO
//				{
//					ErrCode = 400,
//					ErrMessage = "Entity is null"
//				});

//			else
//			{
//				//TEntity e =await _context.Set<TEntity>().FirstOrDefaultAsync(e => e.ID = entity.ID);
//				_context.Set<TEntity>().Remove(entity);
//				int res = _context.SaveChanges();
//				if (res > 0)
//				{
//					return Task.FromResult<object>(new ResultDTO
//					{
//						SuccessCode = 200,
//						SuccessMessage = "Entity deleted successfully"
//					});
//				}
//				else
//				{
//					return Task.FromResult<object>(new ResultDTO
//					{
//						ErrCode = 500,
//						ErrMessage = "Error deleting entity"
//					});
//				}
//			}
//		}

//	public async Task<IEnumerable<TEntity>> GetEntitiesAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null, string? include = "") where TEntity : class
//	{
//		IQueryable<TEntity> query = _context.Set<TEntity>();
//		if(filter != null)
//			query = query.Where(filter);
//		if (!string.IsNullOrEmpty(include))
//		{
//			// Validate and safely include navigation properties
//			var trimmedInclude = include.Trim();
//			// Only allow alphanumeric characters, dots, and underscores (navigation properties)
//			if (System.Text.RegularExpressions.Regex.IsMatch(trimmedInclude, "^[a-zA-Z0-9_.]+$"))
//			{
//				query = query.Include(trimmedInclude);
//			}
//			else
//			{
//				throw new ArgumentException("Invalid include parameter. Only alphanumeric characters, dots, and underscores are allowed.", nameof(include));
//			}
//		}
//		return await query.ToListAsync();
//	}

//	public Task<TEntity> GetEntityAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null, string? include = "") where TEntity : class
//	{
//		IQueryable<TEntity> query = _context.Set<TEntity>();
//		if (filter != null)
//			query = query.Where(filter);
//		if (!string.IsNullOrEmpty(include))
//		{
//			// Validate and safely include navigation properties
//			var trimmedInclude = include.Trim();
//			// Only allow alphanumeric characters, dots, and underscores (navigation properties)
//			if (System.Text.RegularExpressions.Regex.IsMatch(trimmedInclude, "^[a-zA-Z0-9_.]+$"))
//			{
//				query = query.Include(trimmedInclude);
//			}
//			else
//			{
//				throw new ArgumentException("Invalid include parameter. Only alphanumeric characters, dots, and underscores are allowed.", nameof(include));
//			}
//		}
//		return query.FirstOrDefaultAsync();
//	}

//		public Task<object> UpdateEntityAsync<TEntity>(TEntity entity) where TEntity : class
//		{
//			if (entity == null)
//				return Task.FromResult<object>(new ResultDTO
//				{
//					ErrCode = 400,
//					ErrMessage = "Entity is null"
//				});
//			else
//			{
//				_context.Set<TEntity>().Update(entity);
//				int res = _context.SaveChanges();
//				if (res > 0)
//				{
//					return Task.FromResult<object>(new ResultDTO
//					{
//						SuccessCode = 200,
//						SuccessMessage = "Entity updated successfully"
//					});
//				}
//				else
//				{
//					return Task.FromResult<object>(new ResultDTO
//					{
//						ErrCode = 500,
//						ErrMessage = "Error updating entity"
//					});
//				}
//			}
//		}
//	}
//}










////using System.Linq.Expressions;
////using grad.Interfaces;
////using grad.DTO;
////using grad.Data;
////using Microsoft.EntityFrameworkCore;
////namespace grad.Repositories
////{
////	public class Repository : IRepository
////	{

////		private readonly Db_Context _context;
////		public Repository(Db_Context context)
////		{
////			_context = context ?? throw new ArgumentNullException(nameof(context));
////		}
////		public Task<object> CreateEntityAsync<TEntity>(TEntity entity) where TEntity : class
////		{
////			if (entity == null)
////				return Task.FromResult<object>(new ResultDTO
////				{
////					ErrCode = 400,
////					ErrMessage = "Entity is null"
////				});

////			else 
////			{
////				_context.Set<TEntity>().Add(entity);
////				_context.SaveChanges();
////				return Task.FromResult<object>(entity);
////			}

////		}

////		public async Task<object> DeleteEntityAsync<TEntity>(TEntity entity) where TEntity : class
////		{
////			if (entity == null)
////				return Task.FromResult<object>(new ResultDTO
////				{
////					ErrCode = 400,
////					ErrMessage = "Entity is null"
////				});

////			else
////			{
////				//TEntity e =await _context.Set<TEntity>().FirstOrDefaultAsync(e => e.ID = entity.ID);
////				_context.Set<TEntity>().Remove(entity);
////				int res = _context.SaveChanges();
////				if (res > 0)
////				{
////					return Task.FromResult<object>(new ResultDTO
////					{
////						SuccessCode = 200,
////						SuccessMessage = "Entity deleted successfully"
////					});
////				}
////				else
////				{
////					return Task.FromResult<object>(new ResultDTO
////					{
////						ErrCode = 500,
////						ErrMessage = "Error deleting entity"
////					});
////				}
////			}
////		}

////	public async Task<IEnumerable<TEntity>> GetEntitiesAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null, string? include = "") where TEntity : class
////	{
////		IQueryable<TEntity> query = _context.Set<TEntity>();
////		if(filter != null)
////			query = query.Where(filter);
////		if (!string.IsNullOrEmpty(include))
////		{
////			// Validate and safely include navigation properties
////			var trimmedInclude = include.Trim();
////			// Only allow alphanumeric characters, dots, and underscores (navigation properties)
////			if (System.Text.RegularExpressions.Regex.IsMatch(trimmedInclude, "^[a-zA-Z0-9_.]+$"))
////			{
////				query = query.Include(trimmedInclude);
////			}
////			else
////			{
////				throw new ArgumentException("Invalid include parameter. Only alphanumeric characters, dots, and underscores are allowed.", nameof(include));
////			}
////		}
////		return await query.ToListAsync();
////	}

////	public Task<TEntity> GetEntityAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null, string? include = "") where TEntity : class
////	{
////		IQueryable<TEntity> query = _context.Set<TEntity>();
////		if (filter != null)
////			query = query.Where(filter);
////		if (!string.IsNullOrEmpty(include))
////		{
////			// Validate and safely include navigation properties
////			var trimmedInclude = include.Trim();
////			// Only allow alphanumeric characters, dots, and underscores (navigation properties)
////			if (System.Text.RegularExpressions.Regex.IsMatch(trimmedInclude, "^[a-zA-Z0-9_.]+$"))
////			{
////				query = query.Include(trimmedInclude);
////			}
////			else
////			{
////				throw new ArgumentException("Invalid include parameter. Only alphanumeric characters, dots, and underscores are allowed.", nameof(include));
////			}
////		}
////		return query.FirstOrDefaultAsync();
////	}

////		public Task<object> UpdateEntityAsync<TEntity>(TEntity entity) where TEntity : class
////		{
////			if (entity == null)
////				return Task.FromResult<object>(new ResultDTO
////				{
////					ErrCode = 400,
////					ErrMessage = "Entity is null"
////				});
////			else
////			{
////				_context.Set<TEntity>().Update(entity);
////				int res = _context.SaveChanges();
////				if (res > 0)
////				{
////					return Task.FromResult<object>(new ResultDTO
////					{
////						SuccessCode = 200,
////						SuccessMessage = "Entity updated successfully"
////					});
////				}
////				else
////				{
////					return Task.FromResult<object>(new ResultDTO
////					{
////						ErrCode = 500,
////						ErrMessage = "Error updating entity"
////					});
////				}
////			}
////		}
////	}
////}














//v2

//using System.Linq.Expressions;
//using grad.Interfaces;
//using grad.DTO;
//using grad.Data;
//using Microsoft.EntityFrameworkCore;
//using System.Threading;
//using StackExchange.Redis;

//namespace grad.Repositories
//{
//	public class Repository : IRepository
//	{
//		private readonly Db_Context _context;

//		public Repository(Db_Context context)
//		{
//			_context = context ?? throw new ArgumentNullException(nameof(context));
//		}


//		public async Task<object> CreateEntityAsync<TEntity>(
//			TEntity entity,
//			CancellationToken cancellationToken = default
//		) where TEntity : class
//		{
//			if (entity == null)
//				return null;

//			await _context.Set<TEntity>().AddAsync(entity, cancellationToken);
//			int result = await _context.SaveChangesAsync(cancellationToken);

//			return result > 0 ? entity : null;
//		}

//		public async Task<bool> DeleteEntityAsync<TEntity>(TEntity entity,CancellationToken cancellationToken = default
//		) where TEntity : class
//		{
//			if (entity == null)
//				return false;

//			_context.Set<TEntity>().Remove(entity);
//			int result = await _context.SaveChangesAsync(cancellationToken);

//			return result > 0;
//		}

//		public async Task<IEnumerable<TEntity>> GetEntitiesAsync<TEntity>(
//			Expression<Func<TEntity, bool>>? filter = null,
//			Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
//			CancellationToken cancellationToken = default
//		) where TEntity : class
//		{
//			IQueryable<TEntity> query = _context.Set<TEntity>();

//			if (filter != null)
//				query = query.Where(filter);

//			// Apply include expression if provided
//			if (include != null)
//				query = include(query);

//			return await query.ToListAsync(cancellationToken);
//		}

//		public async Task<TEntity?> GetEntityAsync<TEntity>(Expression<Func<TEntity, bool>>? filter = null, Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
//			CancellationToken cancellationToken = default
//		) where TEntity : class
//		{
//			IQueryable<TEntity> query = _context.Set<TEntity>();

//			if (filter != null)
//				query = query.Where(filter);

//			if (include != null)
//				query = include(query);

//			return await query.FirstOrDefaultAsync(cancellationToken);
//		}


//		public async Task<bool> UpdateEntityAsync<TEntity>(
//			TEntity entity,
//			CancellationToken cancellationToken = default
//		) where TEntity : class
//		{
//			if (entity == null)
//				return false;

//			_context.Set<TEntity>().Update(entity);
//			int result = await _context.SaveChangesAsync(cancellationToken);

//			return result > 0;
//		}
//	}
//}
