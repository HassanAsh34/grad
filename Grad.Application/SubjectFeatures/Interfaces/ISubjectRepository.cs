using Grad.Application.Common.Interfaces;
using Grad.Domain.Model;


namespace Grad.Application.SubjectFeatures.Interfaces
{
    public interface ISubjectRepository : IRepository
    {
        Task<List<Subject>> GetSubjectsByTeacherAsync(Guid teacherId, CancellationToken ct = default);
        Task<List<Subject>> GetSubjectsByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
        Task<List<Subject>> GetSubjectsFilteredAsync(bool? deafMute, IEnumerable<Guid>? excludeIds = null, CancellationToken ct = default);
        Task<List<Subject>> GetAllSubjectsAsync(CancellationToken ct = default);
        Task<Subject?> GetSubjectWithRelationsAsync(Guid subjectId, CancellationToken ct = default);
        Task<SubjectContent?> GetSubjectContentAsync(Guid subjectId, CancellationToken ct = default);

        Task<int> AddSubject(Subject subject, CancellationToken CT = default);


		Task<bool> IsSubjectExistAsync(string? subjectName = "", bool? deaf_mute = false, Guid? subjectId = null, CancellationToken cancellationToken = default);
        
        Task<int> AddVocabularyAsync(Guid subjectId, Vocabulary vocabulary, CancellationToken ct = default);

        Task<bool> updateLessonCountAsync(Guid subjectId, CancellationToken ct = default);

	}
}
