	namespace Grad.Domain.Model
	{
		public class Subject
		{

			public Guid Id { get; private set; } = Guid.NewGuid();

			public string Name { get; set; }

			public bool deaf_mute { get; set; }

			public bool AI_supported { get; set; } = false;

			public int LessonCount { get; set; } = 0;

			public DateOnly CreatedAt { get; private set; } = DateOnly.FromDateTime(DateTime.Now);

			public DateOnly UpdatedAt { get; set; } = DateOnly.FromDateTime(DateTime.Now);

		//public IEnumerable<Lesson> Lessons { get; set; } //lessons related to the subject

		public List<AssignedSubject> AssignedSubjects { get; set; } = new();

			//public ICollection<Teacher> Teachers { get; set; } = new List<Teacher>(); //resposible teachers
			public List<Enrollment> Students { get; set; } = new();
		
			public List<Submission> Submissions { get; set; } = new();//students enrolled in the class

		//private enum Subject
		//{
		//	arabic,
		//	english,
		//	mathematics,
		//	science,
		//}
		}

	}
