using grad.DTO;
using grad.Interfaces;
using grad.Model;
using Microsoft.EntityFrameworkCore;

namespace grad.Services
{
	public class TeacherServices : ITeacherServices
	{
		private readonly ISubjectServices _subjectServices;

		private readonly ILessonServices _lessonServices;

		private readonly IRepository _repository;

		public TeacherServices(ISubjectServices subjectServices,IRepository repository,ILessonServices lessonServices)
		{
			_subjectServices = subjectServices ?? throw new ArgumentNullException(nameof(subjectServices));
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
			_lessonServices = lessonServices ?? throw new ArgumentNullException(nameof(lessonServices));
		}
		public async Task<ResultDTO> ShowStudents(Guid Sid,CancellationToken cancellation)
		{
			if (await _subjectServices.IsSubjectExist(subjectId: Sid))
			{
				IEnumerable<Enrollement> enrollement = await _repository.GetEntitiesAsync<Enrollement>(e => e.SUBFK == Sid,cancellationToken: cancellation);
				return new ResultDTO
				{
					StatusCode = StatusCodes.Status200OK,
					result = enrollement
				};
			}
			else
				throw new NotImplementedException();

		}

		public async Task<ResultDTO> AddLesson(LessonDTO lesson,CancellationToken cancellationToken)
		{
			return await _subjectServices.AddLesson(lesson, cancellationToken);
		}

		public async Task<ResultDTO> ViewLessons(Guid sid, CancellationToken cancellation)
		{
			return await _lessonServices.ViewLessons(sid, cancellation);
		}
	}
}
