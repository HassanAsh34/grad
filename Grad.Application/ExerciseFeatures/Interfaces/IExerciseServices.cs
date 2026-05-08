using Grad.Application.Common.DTOs;
using Grad.Application.ExerciseFeatures.DTOs;


namespace Grad.Application.ExerciseFeatures.Interfaces
{
	public interface IExerciseServices
	{
		public Task<ResultDTO> CreateExercise(CreateLevelDTO exerciseDTO, CancellationToken cancellationToken = default);

		public Task<List<LevelDTO>> GetQuizes(Guid sid, CancellationToken CT = default);

		public Task<ResultDTO> viewLevel(LevelDTO levelDTO, bool teacher, CancellationToken CT = default);

		public Task<ResultDTO> EditLevel(EditLevelDTO editLevel, CancellationToken cancellationToken = default);

		public Task<ResultDTO> DeleteLevel(LevelDTO levelDTO, CancellationToken cancellationToken = default);

	}
}
