using grad.Application.Common.DTOs;
using grad.Application.exercise.DTOs;

namespace grad.Application.exercise.Interfaces
{
	public interface IExerciseServices
	{
		public Task<ResultDTO> CreateExercise(CreateExerciseDTO exerciseDTO, CancellationToken cancellationToken = default);
	}
}
