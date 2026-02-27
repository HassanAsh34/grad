using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace grad.Application.lesson.DTOs
{ 
	public class VideoDTO
	{
		public Guid LId { get; set; }

		public Guid subjectID { get; set; }

		public Guid? VId { get; set; }

		[Required(ErrorMessage ="Title Field can't be empty")]
		[MinLength(3, ErrorMessage = "Title must be at least 3 characters long.")]
		public string Title { get; set; }

		[Required(ErrorMessage = "Description for the video is required")]
		[MinLength(3, ErrorMessage = "Title must be at least 3 characters long.")]
		public string Description { get; set; }

		public string ?Uploaded_by { get; set; }

		//[Required(ErrorMessage = "the video is required")]
		public IFormFile? VideoFile {  get; set; }

		public string? videoUrl { get; set; }

		//[JsonIgnore]
		//public string? VideoPath { get; set; }

		public DateTime? ReleaseDate { get;  set; }
	}
}
