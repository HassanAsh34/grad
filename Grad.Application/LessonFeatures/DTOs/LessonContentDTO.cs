using Grad.Application.ExerciseFeatures.DTOs;
using Grad.Domain.Enums;

namespace Grad.Application.LessonFeatures.DTOs
{
	public class LessonContentDTO
	{

		public Guid Id { get; set; }

		public Guid SubjectId { get; set; }

		public string Title { get; set; }
		public string Description { get; set; } = string.Empty;


		public int VideosCount { get; set; } = 0;

		public bool locked { get; set; }

		public Guid ?Nlid { get; set; }

		public string Next { get; set; } = string.Empty;

		public PerquisiteType NextType { get; set; } = PerquisiteType.None;

		public string Previous { get; set; }

		public PerquisiteType PreviousType { get; set; }


		public List<VideoDTO> Videos { get; set; } = new();

		public LevelDTO? Levels { get; set; }
	}
}
