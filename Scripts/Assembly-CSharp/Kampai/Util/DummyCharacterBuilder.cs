using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Game.View;
using Kampai.Main;
using UnityEngine;

namespace Kampai.Util
{
	public class DummyCharacterBuilder : IDummyCharacterBuilder
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("DummyCharacterBuilder") as IKampaiLogger;

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

		[Inject]
		public IRandomService randomService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		public DummyCharacterObject BuildMinion(Minion minion, CostumeItemDefinition costume, Transform parent, bool isHigh, Vector3 villainScale, Vector3 villainPositionOffset)
		{
			string targetLOD = dlcService.GetDownloadQualityLevel().ToUpper();
			string name = string.Format("DUMMY_{0}", minion.Name);
			GameObject gameObject = SkinnedMeshAggregator.CreateAggregateObject(name, costume.Skeleton, costume.MeshList, targetLOD);
			DummyCharacterObject dummyCharacterObject = gameObject.AddComponent<DummyCharacterObject>();
			dummyCharacterObject.Init(minion, logger, randomService, definitionService, GetWeightedInstanceList(costume.characterUIAnimationDefinition));
			dummyCharacterObject.Build(minion, costume.characterUIAnimationDefinition, parent, logger, isHigh, villainScale, villainPositionOffset, minionBuilder);
			AnimEventHandler animEventHandler = gameObject.AddComponent<AnimEventHandler>();
			animEventHandler.Init(dummyCharacterObject, dummyCharacterObject.localAudioEmitter, audioSignal, stopAudioSignal, minionStateAudioSignal, startLoopingAudioSignal);
			return dummyCharacterObject;
		}

		public DummyCharacterObject BuildNamedChacter(NamedCharacter namedCharacter, Transform parent, bool isHigh, Vector3 villainScale, Vector3 villainPositionOffset)
		{
			string arg = dlcService.GetDownloadQualityLevel().ToUpper();
			NamedCharacterDefinition definition = namedCharacter.Definition;
			string prefab = definition.Prefab;
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
			DummyCharacterObject dummyCharacterObject = gameObject.AddComponent<DummyCharacterObject>();
			CharacterUIAnimationDefinition characterUIAnimationDefinition = definition.CharacterAnimations.characterUIAnimationDefinition;
			dummyCharacterObject.Init(namedCharacter, logger, randomService, definitionService, GetWeightedInstanceList(characterUIAnimationDefinition));
			dummyCharacterObject.Build(namedCharacter, characterUIAnimationDefinition, parent, logger, isHigh, villainScale, villainPositionOffset, minionBuilder);
			AnimEventHandler animEventHandler = gameObject.AddComponent<AnimEventHandler>();
			animEventHandler.Init(dummyCharacterObject, dummyCharacterObject.localAudioEmitter, audioSignal, stopAudioSignal, minionStateAudioSignal, startLoopingAudioSignal);
			return dummyCharacterObject;
		}

		private List<WeightedInstance> GetWeightedInstanceList(CharacterUIAnimationDefinition characterUIAnimationDefinition)
		{
			List<WeightedInstance> list = new List<WeightedInstance>();
			if (!characterUIAnimationDefinition.UseLegacy)
			{
				WeightedDefinition def = definitionService.Get<WeightedDefinition>(characterUIAnimationDefinition.IdleWeightedAnimationID);
				WeightedDefinition def2 = definitionService.Get<WeightedDefinition>(characterUIAnimationDefinition.HappyWeightedAnimationID);
				WeightedDefinition def3 = definitionService.Get<WeightedDefinition>(characterUIAnimationDefinition.SelectedWeightedAnimationID);
				list.Add(new WeightedInstance(def));
				list.Add(new WeightedInstance(def2));
				list.Add(new WeightedInstance(def3));
			}
			return list;
		}
	}
}
