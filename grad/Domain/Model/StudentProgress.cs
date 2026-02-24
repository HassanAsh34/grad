//using MongoDB.Bson.Serialization.Attributes;

namespace Grad_Structured.Domain.Model
{
	public class StudentProgress
	{
		//[BsonId]
		public Guid Id { get; private set; } = Guid.NewGuid();

		public Guid Eid_fk { get; set; }

		public Guid lid { get; set; }

		public DateTime Completed_At { get; set; }

		public Enrollement Enrollement { get; set; }
	}
}