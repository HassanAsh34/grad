using Grad.Domain.Model;

namespace Grad.Application.ExerciseFeatures.Interfaces
{
	public interface IExerciseRepository
	{
		Task<int> addExerciseToLesson(Guid sid, LessonContent lesson, CancellationToken CT = default);

		Task<int> addQuizToSubject(Guid sid, Level exercise, CancellationToken CT = default);

		Task<Level> GetLevel(Guid Sid, Guid? Lid, Guid LVLid, CancellationToken CT = default);

		Task<List<Level>> viewLevels(Guid sid, CancellationToken CT);

		Task<int> editLevel(Guid Sid, Guid? Lid, Level exercise, CancellationToken CT = default);
	}
}
