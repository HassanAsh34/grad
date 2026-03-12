
namespace Grad.Domain.Model
{
	public class Answer
	{
		public Guid Id { get; private set; } = Guid.NewGuid();
		public string ?answer { get; set; }

		public string ?IMG { get; set; }

		//public bool isCorrect { get; set; }
	}
}
