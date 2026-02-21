using System.Text.Json.Serialization;
using grad.Model;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

using JsonIgnoreAttribute = System.Text.Json.Serialization.JsonIgnoreAttribute;

namespace grad.DTO
{
	public class LessonContentDTO
	{

		public Guid Id { get; set; }

		public Guid SubjectId { get; set; }

		public string Title { get; set; }


		public int VideosCount { get; set; } = 0;


		public List<VideoDTO> Videos { get; set; } = new();

		public Exercise? Exercises { get; set; }
	}
}
