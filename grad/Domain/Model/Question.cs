using System.Collections.Generic;
using MongoDB.Driver;

namespace grad.Domain.Model
{
	public class Question
	{
		public string question_type { get; set; }

		public Guid Qid { get; set; } = Guid.NewGuid();

		public string ?prompt_text { get; set; }

		public string ?prompt_image { get; set; }

		public Answer ?CorrectAnswer { get; set; }

		public List<Answer> Answers { get; set; } = new List<Answer>();

		public int score { get; set; } = 10;
	}
}
