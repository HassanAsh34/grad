namespace Grad.Application.Common.Interfaces
{
	public interface IUowServices
	{
		Task<int> SaveChangesAsync();
	}
}
