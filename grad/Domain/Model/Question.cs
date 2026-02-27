using System.Collections.Generic;
using MongoDB.Driver;

namespace grad.Domain.Model
{
	public class Question
	{
		public string question_type { get; set; }

		public string ?prompt_text { get; set; }

		public string ?prompt_image { get; set; }

		public Answer CorrectAnswer { get; set; }

		public IEnumerable<Answer> Answers { get; set; }

		public int score { get; set; } = 10;
	}
}
