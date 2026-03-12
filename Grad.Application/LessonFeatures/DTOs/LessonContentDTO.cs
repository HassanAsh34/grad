using Grad.Application.ExerciseFeatures.DTOs;

namespace Grad.Application.LessonFeatures.DTOs
{
	public class LessonContentDTO
	{

		public Guid Id { get; set; }

		public Guid SubjectId { get; set; }

		public string Title { get; set; }


		public int VideosCount { get; set; } = 0;


		public List<VideoDTO> Videos { get; set; } = new();

		public ExerciseDTO? Exercises { get; set; }
	}
}
