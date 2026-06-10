using Grad.Application.Common.DTOs;
using Grad.Application.StudentFeatures.DTOs;
using Grad.Application.SubjectFeatures.DTOs;
using Grad.Domain.Model;

namespace Grad.Application.SubjectFeatures.Interfaces
{
	public interface ISubjectServices
	{
		public Task<ResultDTO> AddSubject(SubjectDTO subject, CancellationToken cancellationToken = default);
		public Task<ResultDTO> RemoveSubject(Guid subjectid, CancellationToken cancellationToken = default);
		public Task<ResultDTO> UpdateSubject(Guid subjectid,string SubjectName, CancellationToken cancellationToken = default);
		public Task<ResultDTO> ViewSubjectsAsync(bool Teacher = false, List<Guid>? guids = null, int disability = -1, Dictionary<Guid, EnrollmentDTO>? enrollements = null, CancellationToken cancellationToken = default);
		 
		//public Task<ResultDTO> ViewSubjectsAsync(Guid? Tid = null, List<Guid> ?guids = null, int disability = -1, IEnumerable<Enrollement>? enrollements = null, CancellationToken cancellationToken = default);
		public Task<bool> updateCountAsync(Guid sid, CancellationToken cancellationToken = default);
		public Task<ResultDTO> ViewSubjectAsync(Guid sid,bool all = true, CancellationToken cancellationToken = default);
		public Task<ResultDTO> addwords(AddVocabDTO vocabDTO, CancellationToken cancellationToken = default);
		public Task<bool> IsSubjectExist(string? subjectName = "", bool? deaf_mute = false, Guid? subjectId = null, CancellationToken cancellationToken = default);

		public Task<ResultDTO> ViewDictionary(Guid sid, CancellationToken cancellationToken = default);
	}
}
