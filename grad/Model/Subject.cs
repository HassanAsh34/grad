namespace grad.Model
{
	public class Subject
	{

		public string Id { get; private set; } = Guid.NewGuid().ToString();

		public string Name { get; set; }

		public IEnumerable<Teacher> Teachers { get; set; } //resposible teachers

		public IEnumerable<Student> Students { get; set; } //students enrolled in the class

		//private enum Subject
		//{
		//	arabic,
		//	english,
		//	mathematics,
		//	science,
		//}
	}

}
