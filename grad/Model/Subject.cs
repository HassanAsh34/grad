	namespace grad.Model
	{
		public class Subject
		{

			public Guid Id { get; private set; } = Guid.NewGuid();

			public string Name { get; set; }

			public bool deaf_mute { get; set; }

			public int LessonCount { get; set; } = 0;

			public DateOnly CreatedAt { get; private set; } = DateOnly.FromDateTime(DateTime.Now);

			public DateOnly UpdatedAt { get; set; } = DateOnly.FromDateTime(DateTime.Now);

			//public IEnumerable<Lesson> Lessons { get; set; } //lessons related to the subject

			public ICollection<AssignedSubject> AssignedSubjects { get; set; }	= new List<AssignedSubject>();

			//public ICollection<Teacher> Teachers { get; set; } = new List<Teacher>(); //resposible teachers
			public ICollection<Enrollement> Students { get; set; } = new List<Enrollement>(); //students enrolled in the class

		//private enum Subject
		//{
		//	arabic,
		//	english,
		//	mathematics,
		//	science,
		//}
		}

	}
