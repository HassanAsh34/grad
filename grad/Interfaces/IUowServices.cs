namespace grad.Interfaces
{
	public interface IUowServices
	{
		public Task<int> SaveChangesAsync();
	}
}
