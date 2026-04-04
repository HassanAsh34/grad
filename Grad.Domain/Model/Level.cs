using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grad.Domain.Enums;

namespace Grad.Domain.Model
{
	public class Level
	{
		public Guid ID { get; set; } = Guid.NewGuid();

		public int Duration { get; set; } = -1;
		public string Name { get; set; }
		public List<Exercise> Exercise { get; set; } = new List<Exercise>();
		public int PassingPercentage { get; set; }
		public Difficulty levelDifficulty { get; set; }
		public int total_score => Exercise?.Sum(e => e.total_score) ?? 0;

		public int PassingScore => total_score * PassingPercentage / 100;
	}
}
