using System;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class QuestScriptService : IQuestScriptService
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("QuestScriptService") as IKampaiLogger;

		private Dictionary<string, IQuestScriptRunner> runners = new Dictionary<string, IQuestScriptRunner>();

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		public bool HasScript(int questInstanceID, bool pre, int stepID = -1, bool isQuestStep = false)
		{
			Quest byInstanceId = playerService.GetByInstanceId<Quest>(questInstanceID);
			if (byInstanceId != null)
			{
				return HasScript(byInstanceId, pre, stepID, isQuestStep);
			}
			logger.Error("Quest Instance Id {0} doesn't exist", questInstanceID);
			return false;
		}

		public bool HasScript(Quest quest, bool pre, int stepID = -1, bool isQuestStep = false)
		{
			string scriptName = string.Empty;
			return HasScript(quest, pre, false, stepID, out scriptName, isQuestStep);
		}

		public bool HasScript(Quest quest, bool pre, bool isReward, int stepID, out string scriptName, bool isQuestStep)
		{
			scriptName = "quest_" + quest.GetActiveDefinition().ID;
			if (stepID > -1)
			{
				scriptName = scriptName + "_step_" + (stepID + 1);
			}
			if (isReward)
			{
				scriptName += "_reward";
			}
			else
			{
				scriptName += ((!pre) ? "_post" : "_pre");
			}
			if (KampaiResources.FileExists(scriptName))
			{
				return true;
			}
			scriptName = string.Empty;
			if (isQuestStep)
			{
				return false;
			}
			return HasIntroVoiceOrOutro(quest, pre);
		}

		private bool HasIntroVoiceOrOutro(Quest quest, bool pre)
		{
			bool flag = quest.GetActiveDefinition().QuestIntro != null && quest.GetActiveDefinition().QuestIntro.Length > 0;
			bool flag2 = quest.GetActiveDefinition().QuestVoice != null && quest.GetActiveDefinition().QuestVoice.Length > 0;
			bool flag3 = quest.GetActiveDefinition().QuestOutro != null && quest.GetActiveDefinition().QuestOutro.Length > 0;
			return (pre && (flag || flag2)) || (!pre && flag3);
		}

		public void StartQuestScript(int questInstanceID, bool pre, bool isReward = false, int stepID = -1, bool isStepQuest = false)
		{
			Quest byInstanceId = playerService.GetByInstanceId<Quest>(questInstanceID);
			if (byInstanceId != null)
			{
				StartQuestScript(byInstanceId, pre, isReward, stepID, isStepQuest);
				return;
			}
			logger.Error("Quest Instance Id {0} doesn't exist", questInstanceID);
		}

		public void StartQuestScript(Quest quest, bool pre, bool isReward = false, int stepID = -1, bool isStepQuest = false)
		{
			string scriptName = string.Empty;
			if (!HasScript(quest, pre, isReward, stepID, out scriptName, isStepQuest))
			{
				if (!isReward)
				{
					gameContext.injectionBinder.GetInstance<GoToNextQuestStateSignal>().Dispatch(quest.GetActiveDefinition().ID);
				}
				return;
			}
			string text = null;
			if (HasIntroVoiceOrOutro(quest, pre))
			{
				text = CreateDialogText(quest, pre);
			}
			if (scriptName.Length > 0)
			{
				TextAsset textAsset = KampaiResources.Load<TextAsset>(scriptName);
				if (textAsset == null)
				{
					logger.Error("Failed to load script {0}", scriptName);
					return;
				}
				text = ((!string.IsNullOrEmpty(text)) ? (text + textAsset.text) : textAsset.text);
			}
			if (!string.IsNullOrEmpty(text))
			{
				if (string.IsNullOrEmpty(scriptName))
				{
					QuestDefinition activeDefinition = quest.GetActiveDefinition();
					scriptName = activeDefinition.ID + "_" + ((!pre) ? "Outro" : "Intro");
				}
				CreateRunningQuest(quest, scriptName, stepID, text);
			}
		}

		private string CreateDialogText(Quest quest, bool pre)
		{
			string text = string.Empty;
			string text2 = string.Empty;
			int num = 0;
			int num2 = 0;
			QuestDefinition activeDefinition = quest.GetActiveDefinition();
			if (pre)
			{
				string questIntro = quest.GetActiveDefinition().QuestIntro;
				string questVoice = quest.GetActiveDefinition().QuestVoice;
				bool flag = questIntro != null && questIntro.Length > 0;
				if (flag)
				{
					text = questIntro;
					num = activeDefinition.SurfaceID;
				}
				if (questVoice != null && questVoice.Length > 0)
				{
					if (flag)
					{
						text2 = questVoice;
						num2 = activeDefinition.SurfaceID;
					}
					else
					{
						text = questVoice;
						num = activeDefinition.SurfaceID;
					}
				}
			}
			else
			{
				string questOutro = quest.GetActiveDefinition().QuestOutro;
				if (questOutro != null && questOutro.Length > 0)
				{
					text = questOutro;
					num = activeDefinition.SurfaceID;
				}
			}
			if (text.Length > 0)
			{
				return "qsutil.showIntroVoiceOrOutroDialogs('" + text + "', " + num + ", '" + text2 + "', " + num2 + ")" + System.Environment.NewLine;
			}
			return null;
		}

		private void CreateRunningQuest(Quest quest, string scriptName, int stepID, string data)
		{
			IQuestScriptRunner orCreateRunner = GetOrCreateRunner(scriptName, QuestRunnerLanguage.Lua);
			orCreateRunner.OnQuestScriptComplete = OnQuestScriptComplete;
			if (quest.questScriptInstances.ContainsKey(scriptName))
			{
				quest.questScriptInstances[scriptName].QuestID = quest.GetActiveDefinition().ID;
				quest.questScriptInstances[scriptName].QuestLocalizedKey = quest.GetActiveDefinition().LocalizedKey;
				quest.questScriptInstances[scriptName].QuestStepID = stepID;
				quest.questScriptInstances[scriptName].Key = scriptName;
			}
			else
			{
				QuestScriptInstance questScriptInstance = new QuestScriptInstance();
				questScriptInstance.QuestID = quest.GetActiveDefinition().ID;
				questScriptInstance.QuestLocalizedKey = quest.GetActiveDefinition().LocalizedKey;
				questScriptInstance.QuestStepID = stepID;
				questScriptInstance.Key = scriptName;
				quest.questScriptInstances.Add(scriptName, questScriptInstance);
			}
			orCreateRunner.Start(filename: (!scriptName.EndsWith("_Intro") && !scriptName.EndsWith("_Outro")) ? ("./" + scriptName + ".txt") : null, questScriptInstance: quest.questScriptInstances[scriptName], scriptText: data, startMethodName: null);
		}

		public void PauseQuestScripts()
		{
			List<string> list = new List<string>();
			foreach (string key in runners.Keys)
			{
				list.Add(key);
			}
			foreach (string item in list)
			{
				runners[item].Pause();
			}
		}

		public void ResumeQuestScripts()
		{
			List<string> list = new List<string>();
			foreach (string key in runners.Keys)
			{
				list.Add(key);
			}
			foreach (string item in list)
			{
				runners[item].Resume();
			}
		}

		public void OnQuestScriptComplete(QuestScriptInstance questScriptInstance)
		{
			gameContext.injectionBinder.GetInstance<QuestScriptCompleteSignal>().Dispatch(questScriptInstance);
			runners.Remove(questScriptInstance.Key);
		}

		public void stop()
		{
			foreach (IQuestScriptRunner value in runners.Values)
			{
				value.Stop();
			}
			runners.Clear();
		}

		private IQuestScriptRunner GetOrCreateRunner(string key, QuestRunnerLanguage lang)
		{
			IQuestScriptRunner value;
			runners.TryGetValue(key, out value);
			if (value == null || value.Lang != lang)
			{
				value = gameContext.injectionBinder.GetInstance<IQuestScriptRunner>(lang);
				runners[key] = value;
			}
			return value;
		}
	}
}
