using System.Text.Json.Serialization;
using grad.Model;

namespace grad.DTO
{
	public class RegisterStudentDTO
	{
		[JsonIgnore]
		public Guid ?p_Id { get; set; }

		public IFormFile ?image { get; set; }

		[JsonIgnore]
		public string ?Image_Path { get; set; }


		public string FName { get; set; }

		[JsonIgnore]
		public string ?Pname { get; set; }

		public DateOnly BirthDate { get; set; }

		public int ?gender { get; set; }

		public int Disability { get; set; } // 0: None, 1: Hearing, 2: Speech

		//public string Address { get; set; }

		[JsonIgnore]
		
		public string ?PEmail { get; set; }

		[JsonIgnore]

		public string ?Username { get; set; }

	}
}
