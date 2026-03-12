using Grad.Domain.Model;
namespace Grad.Application.ParentFeatures.Interfaces
{
	public interface IParentRepository
	{
		Task<IEnumerable<Student>> showChildren(Guid id, CancellationToken cancellationToken);
	}
}
