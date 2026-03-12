using Microsoft.AspNetCore.Http;

namespace Grad.Application.SubjectFeatures.DTOs
{	
	public class AddVocabDTO
	{
		public List<string> word { get; set; } = new();

		public List<IFormFile> files { get; set; } = new();
		public List<string> fileNames { get; set; } = new();

		public Guid? sid { get; set; }

		public Guid? Tid { get; set; }
	}
}
