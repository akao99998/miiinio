using Elevation.Logging;
using Kampai.Game;
using Kampai.Game.View;
using Kampai.Main;
using UnityEngine;

namespace Kampai.Util
{
	public class NamedCharacterBuilder : INamedCharacterBuilder
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("NamedCharacterBuilder") as IKampaiLogger;

		[Inject]
		public PlayLocalAudioSignal audioSignal { get; set; }

		[Inject]
		public StartLoopingAudioSignal startLoopingAudioSignal { get; set; }

		[Inject]
		public StopLocalAudioSignal stopAudioSignal { get; set; }

		[Inject]
		public PlayMinionStateAudioSignal minionStateAudioSignal { get; set; }

		[Inject]
		public IDLCService dlcService { get; set; }

		[Inject]
		public IMinionBuilder minionBuilder { get; set; }

		public NamedCharacterObject Build(NamedCharacter character, GameObject parent)
		{
			string arg = dlcService.GetDownloadQualityLevel().ToUpper();
			string prefab = character.Definition.Prefab;
			string text = string.Format("{0}_{1}", prefab, arg);
			Object @object = KampaiResources.Load(text);
			GameObject gameObject;
			if (@object == null)
			{
				logger.Error("NamedCharacterBuilder: Failed to load {0}.", text);
				gameObject = new GameObject(text + "(FAILED TO LOAD)");
			}
			else
			{
				gameObject = Object.Instantiate(@object) as GameObject;
			}
			NamedCharacterObject namedCharacterObject = character.Setup(gameObject);
			namedCharacterObject.Build(character, parent, logger, minionBuilder);
			namedCharacterObject.Init(character, logger);
			AnimEventHandler animEventHandler = gameObject.AddComponent<AnimEventHandler>();
			animEventHandler.Init(namedCharacterObject, namedCharacterObject.localAudioEmitter, audioSignal, stopAudioSignal, minionStateAudioSignal, startLoopingAudioSignal);
			namedCharacterObject.SetupRandomizer(character.Definition.CharacterAnimations);
			return namedCharacterObject;
		}
	}
}
