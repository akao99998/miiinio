using Elevation.Logging;
using Kampai.Game;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.signal.impl;

public class SocialPartyFillOrderProfileButtonMediator : KampaiMediator
{
	public class SocialPartyFillOrderProfileButtonMediatorData
	{
		public UserIdentity identity;

		public GameObject parent;

		public int index;
	}

	public IKampaiLogger logger = LogManager.GetClassLogger("SocialPartyFillOrderProfileButtonMediator") as IKampaiLogger;

	private int index;

	private SocialPartyFillOrderProfileButtonMediatorData data;

	[Inject]
	public SocialPartyFillOrderProfileButtonView view { get; set; }

	[Inject]
	public ShowSocialPartyFBConnectSignal showPartyFBConnectSignal { get; set; }

	[Inject]
	public ShowSocialPartyInviteSignal sendSocialPartyInviteSignal { get; set; }

	[Inject(SocialServices.FACEBOOK)]
	public ISocialService facebookService { get; set; }

	[Inject]
	public ILocalizationService localService { get; set; }

	[Inject]
	public SocialPartyFillOrderProfileButtonMediatorUpdateSignal socialPartyFillOrderProfileButtonMediatorUpdateSignal { get; set; }

	public override void Initialize(GUIArguments args)
	{
		data = args.Get<SocialPartyFillOrderProfileButtonMediatorData>();
		if (data.parent == null)
		{
			Object.Destroy(view.gameObject);
			return;
		}
		index = data.index;
		RectTransform rectTransform = view.transform as RectTransform;
		rectTransform.SetParent(data.parent.transform);
		rectTransform.localPosition = Vector3.zero;
		rectTransform.localScale = Vector3.one;
		rectTransform.offsetMin = new Vector2(0f, 0f);
		rectTransform.offsetMax = new Vector2(0f, 0f);
		if (index == 0)
		{
			rectTransform.anchorMin = new Vector2(0.02f, 0.52f);
			rectTransform.anchorMax = new Vector2(0.48f, 0.98f);
		}
		else if (index == 1)
		{
			rectTransform.anchorMin = new Vector2(0.52f, 0.52f);
			rectTransform.anchorMax = new Vector2(0.98f, 0.98f);
		}
		else if (index == 2)
		{
			rectTransform.anchorMin = new Vector2(0.02f, 0.02f);
			rectTransform.anchorMax = new Vector2(0.48f, 0.48f);
		}
		else
		{
			rectTransform.anchorMin = new Vector2(0.52f, 0.02f);
			rectTransform.anchorMax = new Vector2(0.98f, 0.48f);
		}
		SetupProfile();
	}

	public override void OnRegister()
	{
		socialPartyFillOrderProfileButtonMediatorUpdateSignal.AddListener(UpdateDetails);
	}

	public override void OnRemove()
	{
		socialPartyFillOrderProfileButtonMediatorUpdateSignal.RemoveListener(UpdateDetails);
		view.addButton.ClickedSignal.RemoveListener(PlusButton);
	}

	public void UpdateDetails(SocialPartyFillOrderProfileButtonMediatorData details)
	{
		if (data != null && details.index == data.index)
		{
			data = details;
			SetupProfile();
		}
	}

	public void SetupProfile()
	{
		FacebookService facebookService = this.facebookService as FacebookService;
		if (facebookService.isLoggedIn && data.identity != null)
		{
			view.profileOpenPanel.SetActive(true);
			view.profileClosedPanel.SetActive(false);
			string externalID = data.identity.ExternalID;
			Signal<string> signal = new Signal<string>();
			signal.AddListener(OnFacebookPictureComplete);
			StartCoroutine(facebookService.DownloadUserPicture(externalID, signal));
			FBUser friend = facebookService.GetFriend(externalID);
			if (friend != null)
			{
				view.profileOpenTextFBName.text = friend.name;
				view.profileOpenTextFBName.gameObject.SetActive(true);
			}
			else
			{
				view.profileOpenTextFBName.gameObject.SetActive(false);
			}
		}
		else
		{
			if (facebookService.isLoggedIn)
			{
				view.AddPlayerIcon.SetActive(true);
				view.loginText.SetActive(false);
			}
			else
			{
				view.AddPlayerIcon.SetActive(false);
				view.loginText.SetActive(true);
			}
			view.profileOpenPanel.SetActive(false);
			view.profileClosedPanel.SetActive(true);
			view.profileOpenTextFBName.gameObject.SetActive(false);
			view.profileCloseTextAvailableName.text = localService.GetString("socialpartyprofileclosetextavailablename");
			view.addButton.ClickedSignal.AddListener(PlusButton);
		}
	}

	public void PlusButton()
	{
		FacebookService facebookService = this.facebookService as FacebookService;
		if (facebookService.isLoggedIn)
		{
			sendSocialPartyInviteSignal.Dispatch();
			return;
		}
		this.facebookService.LoginSource = "Social Event";
		showPartyFBConnectSignal.Dispatch(delegate(bool successful)
		{
			if (successful)
			{
				view.AddPlayerIcon.SetActive(true);
				view.loginText.SetActive(false);
				sendSocialPartyInviteSignal.Dispatch();
			}
		});
	}

	private void OnFacebookPictureComplete(string id)
	{
		FacebookService facebookService = this.facebookService as FacebookService;
		Texture userPicture = facebookService.GetUserPicture(id);
		if (userPicture != null)
		{
			Sprite sprite = Sprite.Create(userPicture as Texture2D, new Rect(0f, 0f, userPicture.width, userPicture.height), new Vector2(0f, 0f));
			view.profileOpenImageFB.sprite = sprite;
		}
		else
		{
			logger.Warning("OnFacebookPictureComplete null texture for Facebook ID: {0}", id);
		}
	}
}
