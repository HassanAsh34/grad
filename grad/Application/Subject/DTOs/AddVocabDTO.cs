namespace Grad_Structured.Application.subject.DTOs
{	
	public class AddVocabDTO
	{
		public List<string> word { get; set; }

		public List<IFormFile> files { get; set; }

		public Guid ?sid { get; set; }

		public Guid ?Tid { get; set; }

	}
}
