using Grad.Application.Common.Interfaces;
using Grad.Domain.Model;

namespace Grad.Application.Users.Interfaces
{
    public interface IUserRepository : IRepository
    {
        Task<Parent?> GetParentWithStudentsAsync(Guid id, CancellationToken ct = default);
        Task<Student?> GetStudentWithParentAndSubjectsAsync(Guid id, Guid? pid = null, CancellationToken ct = default);
        Task<Teacher?> GetTeacherWithAssignedSubjectsAsync(Guid id, CancellationToken ct = default);
        Task<Admin?> GetAdminByIdAsync(Guid id, CancellationToken ct = default);
    }
}
