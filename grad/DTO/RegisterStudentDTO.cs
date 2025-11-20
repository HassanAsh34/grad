using System.Text.Json.Serialization;

namespace grad.DTO
{
	public class RegisterStudentDTO
	{
		[JsonIgnore]
		public string ?p_Id { get; set; }

		public string FName { get; set; }

		[JsonIgnore]
		public string ?Pname { get; set; }

		public DateOnly BirthDate { get; set; }

		public int Disability { get; set; } // 0: None, 1: Hearing, 2: Speech

		//public string Address { get; set; }

		[JsonIgnore]
		
		public string ?PEmail { get; set; }

	}
}
