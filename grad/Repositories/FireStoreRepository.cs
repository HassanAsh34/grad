using System.Reflection.Metadata;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using grad.Data;

namespace grad.Repositories
{
	public class FireStoreRepository
	{
		private readonly FireStoreContext _DbContext;

		public FireStoreRepository(FireStoreContext dbContext)
		{
			_DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
		}
	}
}
