using Grad.Application.Common.DTOs;
using Grad.Application.ExerciseFeatures.DTOs;


namespace Grad.Application.ExerciseFeatures.Interfaces
{
	public interface IExerciseServices
	{
		public Task<ResultDTO> CreateExercise(CreateExerciseDTO exerciseDTO, CancellationToken cancellationToken = default);
	}
}
