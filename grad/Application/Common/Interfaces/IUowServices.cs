namespace Grad_Structured.Application.Common.Interfaces
{
	public interface IUowServices
	{
		public Task<int> SaveChangesAsync();
	}
}
