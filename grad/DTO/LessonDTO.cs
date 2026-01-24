using System.Text.Json.Serialization;

namespace grad.DTO
{ 
	public class LessonDTO
	{
		public string? Id { get; set; }

		[JsonIgnore]
		public string? subjectID { get; set; }
		public string? Title { get; set; }

		public string? Description { get; set; }

		public IFormFile? VideoFile {  get; set; }

		public string? videoUrl { get; set; }

		[JsonIgnore]
		public string? VideoPath { get; set; }

		public DateTime? ReleaseDate { get;  set; }
	}
}
