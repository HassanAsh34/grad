namespace grad.Model
{
	public class Student : User
	{
		//public String FName { get; set; }

		//public String LName { get; set; }

		public DateOnly BirthDate { get; set; }
		public int age { get; set; }

		public DisablityType Disability { get; set; }

		public enum DisablityType
		{
			None = 1,
			Hearing = 2,
			Speech = 3
		}

		//public string Address { get; set; }

		//public string NationalID { get; set; }

		public Guid ?PID { get; set; } //pfk

		//public string classroomId { get; set; } //cfk

		public IEnumerable<Enrollement> EnrolledSubjects { get; set; }

		public Parent parent { get; set; }
	}
}
