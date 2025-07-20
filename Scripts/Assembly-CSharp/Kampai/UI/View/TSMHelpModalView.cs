using Kampai.Game.View;
using Kampai.Main;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class TSMHelpModalView : PopupMenuView
	{
		public Text Title;

		public Text Message;

		public KampaiImage Image;

		public ButtonView Button;

		public Text ButtonText;

		public MinionSlotModal MinionSlot;

		private DummyCharacterObject dummyCharacterObject;

		public void InitializeView(IFancyUIService fancyUIService, TSMHelpModalArguments args, MoveAudioListenerSignal moveAudioListenerSignal)
		{
			base.Init();
			base.Open();
			Title.gameObject.SetActive(true);
			Title.text = args.Title;
			Message.gameObject.SetActive(true);
			Message.text = args.Message;
			Image.sprite = UIUtils.LoadSpriteFromPath(args.Image);
			Image.maskSprite = UIUtils.LoadSpriteFromPath(args.Image);
			ButtonText.text = args.ButtonText;
			DummyCharacterType type = DummyCharacterType.NamedCharacter;
			dummyCharacterObject = fancyUIService.CreateCharacter(type, DummyCharacterAnimationState.SelectedHappy, MinionSlot.transform, MinionSlot.VillainScale, MinionSlot.VillainPositionOffset, 40014);
			moveAudioListenerSignal.Dispatch(false, dummyCharacterObject.transform);
		}
	}
}
