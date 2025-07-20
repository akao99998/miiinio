using System.Collections.Generic;

namespace Kampai.Game
{
	public class QuestChainStepDefinition
	{
		public string Intro { get; set; }

		public string Voice { get; set; }

		public string Outro { get; set; }

		public int XP { get; set; }

		public int Grind { get; set; }

		public int Premium { get; set; }

		public IList<QuestChainTask> Tasks { get; set; }
	}
}
