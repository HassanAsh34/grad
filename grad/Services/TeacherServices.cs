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

		public async Task<ResultDTO> ShowStudents(Guid Sid, CancellationToken cancellation)
		{
			if (!await _subjectServices.IsSubjectExist(subjectId: Sid))
				return new ResultDTO
				{
					StatusCode = StatusCodes.Status500InternalServerError,
					Message = "Failed to get Students"
				};
			else 
			{
				var students = await _repository
					.GetEntitiesAsync<Enrollement>(
						e => e.SUBFK == Sid,
						q => q.Include(e=>e.Student).Include(e=>e.Student.parent),
						cancellationToken: cancellation
					);
				var result = students.Select(e => new LISTStudentDTO
				{
					Id = e.Student.Id,
					name = $"{e.Student.FName} {e.Student.LName}",
					email = e.Student.EmailorUserName,
					parentInfo = e.Student.parent != null ? new parentInfo
					{
						Email = e.Student.parent.EmailorUserName,
						phoneNumber = e.Student.parent.phoneNumber
					} : null
				}).ToList();


				//IEnumerable<LISTStudentDTO> result = students.Select(e => new LISTStudentDTO
				//{
				//	Id = e.Student.Id,
				//	name = $"{e.Student.FName} {e.Student.LName}",
				//	email = e.Student.EmailorUserName,
				//	parentInfo = e.Student.parent != null ? new parentInfo
				//	{
				//		Email = e.Student.parent.EmailorUserName,
				//		phoneNumber = e.Student.parent.phoneNumber
				//	} : null
				//	//add his progress
				//	// add only what you need
				//});
				int stdCount = result.Count;

				return new ResultDTO
				{
					Message = $"{stdCount} students were found",
					StatusCode = stdCount >0 ? StatusCodes.Status200OK : StatusCodes.Status204NoContent,
					result = result
				};
			}

		}

		public async Task<ResultDTO> AddLesson(LessonDTO lesson,CancellationToken cancellationToken)
		{
			return await _subjectServices.AddLesson(lesson, cancellationToken);
		}

		public async Task<ResultDTO> ViewSubject(Guid sid,CancellationToken cancellationToken)
		{
			return await _subjectServices.ViewSubjectAsync(sid,cancellationToken: cancellationToken);
		}

		public async Task<ResultDTO> ViewLessons(Guid sid, CancellationToken cancellation)
		{
			return await _lessonServices.ViewLessons(sid, cancellation);
		}

		public async Task<ResultDTO> EditLesson(EditLessonDTO lesson, CancellationToken cancellationToken) 
		{
			return await _lessonServices.editLesson(lesson, cancellationToken); 
		}

		public async Task<ResultDTO> DeleteLesson(Guid sid,Guid lid,CancellationToken cancellationToken)
		{
			return await _lessonServices.DeleteLesson(sid, lid, cancellationToken);
		}

		public async Task<ResultDTO> addWords(AddVocabDTO vocabDTO, CancellationToken cancellationToken)
		{
			return await _subjectServices.addwords(vocabDTO, cancellationToken);
		}

	}
}
