using System.Text.Json.Serialization;
using grad.Model;

namespace grad.DTO
{
	public class ProfileDTO
	{
		public string Id { get; set; }

		public string ?Name { get; set; }

		public string ?Email { get; set; }

		public string ?Address { get; set; }

		public string ?phone { get; set; }

		public string Role { get; set; }

		public DateOnly ?BirthDate { get; set; }

		public string ?Disability { get; set; }

		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
		public Parent ?Parent { get; private set; }

		public void setParent(Parent p)
		{
			this.Parent = p;
		}

		//public Student Student { get; set; }


	}
}
