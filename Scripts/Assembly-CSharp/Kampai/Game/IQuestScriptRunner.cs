using System;

namespace Kampai.Game
{
	public interface IQuestScriptRunner
	{
		Action<QuestScriptInstance> OnQuestScriptComplete { get; set; }

		QuestRunnerLanguage Lang { get; }

		ReturnValueContainer InvokationValues { get; }

		void Start(QuestScriptInstance questScriptInstance, string scriptText, string filename, string startMethodName);

		void Stop();

		void Pause();

		void Resume();
	}
}
