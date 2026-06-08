namespace Grad.Domain.Model
{
	public class Teacher : User  //further improvements can be done like adding  resume, degrees, experience etc.
	{

		//public String FName { get; set; }

		//public String LName { get; set; }

		//public string phoneNumber { get; set; }

		//public string Address { get; set; }
		public DateOnly BirthDate { get; set; }
		public int age { get; set; }

		public string cvPath { get; set; }

		//public string ?SubjectName { get; set; }

		public ICollection<AssignedSubject> AssignedSubjects { get; set; } = new List<AssignedSubject>();

		//public Subject ?Subject { get; set; }
		//public IEnumerable<Class> Classes { get; set; }
	}
}
