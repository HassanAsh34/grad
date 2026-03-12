using Grad.Domain.Model;

namespace Grad.Application.ExerciseFeatures.Interfaces
{
	public interface IExerciseRepository
	{
		Task<int> addExerciseToLesson(Guid sid, LessonContent lesson, CancellationToken CT = default);

		Task<int> addQuizToSubject(Guid sid, Exercise exercise, CancellationToken CT = default);
	}
}
