namespace grad.Application.Common.Interfaces
{
	public interface IUowServices
	{
		public Task<int> SaveChangesAsync();
	}
}
