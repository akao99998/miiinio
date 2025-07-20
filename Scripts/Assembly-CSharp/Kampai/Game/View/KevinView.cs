using Kampai.Util;
using Kampai.Util.AI;
using UnityEngine;

namespace Kampai.Game.View
{
	public class KevinView : FrolicCharacterView
	{
		private RuntimeAnimatorController defaultAnimationController;

		private string WelcomeHutStateMachine;

		public override NamedCharacterObject Build(NamedCharacter character, GameObject parent, IKampaiLogger logger, IMinionBuilder minionBuilder)
		{
			base.Build(character, parent, logger, minionBuilder);
			MinionObject.SetEyes(base.transform, 2u);
			MinionObject.SetBody(base.transform, MinionBody.TALL);
			MinionObject.SetHair(base.transform, MinionHair.SPROUT);
			base.gameObject.AddComponent<SteerToAvoidCollisions>();
			SteerToAvoidEnvironment steerToAvoidEnvironment = base.gameObject.AddComponent<SteerToAvoidEnvironment>();
			steerToAvoidEnvironment.Modifier = 8;
			SteerCharacterToSeek steerCharacterToSeek = base.gameObject.AddComponent<SteerCharacterToSeek>();
			steerCharacterToSeek.enabled = false;
			steerCharacterToSeek.Threshold = 0.1f;
			defaultAnimationController = base.gameObject.GetComponent<Animator>().runtimeAnimatorController;
			if (defaultAnimationController == null)
			{
				logger.Error("No default animation controller found for {0}", base.name);
			}
			KevinCharacter kevinCharacter = character as KevinCharacter;
			WelcomeHutStateMachine = kevinCharacter.Definition.WelcomeHutStateMachine;
			return this;
		}

		internal void Walk(bool enable)
		{
			SetAnimBool("walk", enable);
		}

		public void GreetVillain(bool shouldGreet)
		{
			SetAnimBool("GreetVillain", shouldGreet);
		}

		public void WaveFarewell(bool shouldWave)
		{
			SetAnimBool("WaveFarewell", shouldWave);
		}

		internal void GotoWelcomeHut(GameObject WelcomeHut, bool pop)
		{
			if (WelcomeHut == null)
			{
				return;
			}
			Transform transform = WelcomeHut.transform.Find("route0");
			if (!(transform == null))
			{
				if (pop)
				{
					base.transform.position = transform.position;
					base.transform.rotation = transform.rotation;
				}
				ClearActionQueue();
				agent.MaxSpeed = 0f;
				RuntimeAnimatorController controller = KampaiResources.Load<RuntimeAnimatorController>(WelcomeHutStateMachine);
				EnqueueAction(new SetAnimatorAction(this, controller, logger));
				base.IsIdle = true;
			}
		}
	}
}
