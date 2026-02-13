using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace grad.DTO
{
	public class EditLessonDTO
	{
		[JsonIgnore]
		public Guid? Id { get; set; }

		[JsonIgnore]
		public Guid? subjectID { get; set; }

		//[Required(ErrorMessage = "Title Field can't be empty")]
		//[MinLength(3, ErrorMessage = "Title must be at least 3 characters long.")]
		public string ?Title { get; set; }

		//[Required(ErrorMessage = "Description for the video is required")]
		//[MinLength(3, ErrorMessage = "Title must be at least 3 characters long.")]
		public string ?Description { get; set; }
	}
}
