namespace grad.DTO
{
	public class AddVocabDTO
	{
		public List<string> word { get; set; }

		public List<IFormFile> files { get; set; }

		public Guid ?sid { get; set; }

	}
}
