using System;
using System.Collections.Generic;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.pool.api;

namespace Kampai.UI.View
{
	public abstract class DooberCommand : IPoolable, IFastPooledCommandBase
	{
		private const int minSquiggle = 50;

		private const int maxSquiggle = 100;

		protected Vector3 iconPosition;

		protected bool fromWorldCanvas;

		internal int itemDefinitionId;

		[Inject(UIElement.CAMERA)]
		public Camera UICamera { get; set; }

		[Inject]
		public SetXPSignal setXPSignal { get; set; }

		[Inject]
		public SetLevelSignal setLevelSignal { get; set; }

		[Inject]
		public SetGrindCurrencySignal setGrindCurrencySignal { get; set; }

		[Inject]
		public SetPremiumCurrencySignal setPremiumCurrencySignal { get; set; }

		[Inject]
		public SetStorageCapacitySignal setStorageSignal { get; set; }

		[Inject]
		public TokenDooberCompleteSignal tokenDooberCompleteSignal { get; set; }

		[Inject]
		public FireXPVFXSignal fireXpSignal { get; set; }

		[Inject]
		public FireGrindVFXSignal fireGrindSignal { get; set; }

		[Inject]
		public FirePremiumVFXSignal firePremiumSignal { get; set; }

		[Inject]
		public SpawnDooberModel dooberModel { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		[Inject]
		public DoobersFlownSignal doobersFlownSignal { get; set; }

		public bool retain { get; private set; }

		public FastCommandPool commandPool { get; set; }

		protected void TweenToDestination(GameObject go, Vector3 destination, float flyTime, DestinationType tweenType, float delayTime = 0f)
		{
			SpawnDooberModel dooberModel = this.dooberModel;
			dooberModel.DooberCounter++;
			bool isMignette;
			Vector3 worldPositionInUI = GetWorldPositionInUI(tweenType, out isMignette);
			Retain();
			if (!isMignette || dooberModel.RewardedAdDooberMode)
			{
				soundFXSignal.Dispatch(DetermineAudioToFire(tweenType));
				worldPositionInUI.z = go.transform.position.z;
				List<Vector3> nodes = CreatePath(go.transform.position, worldPositionInUI) as List<Vector3>;
				GoSpline path = new GoSpline(nodes);
				List<Vector3> nodes2 = CreatePath(worldPositionInUI, destination) as List<Vector3>;
				GoSpline path2 = new GoSpline(nodes2);
				float endValue = 2f;
				if (tweenType == DestinationType.STORE)
				{
					endValue = 3f;
				}
				GoTween tween = new GoTween(go.transform, flyTime, new GoTweenConfig().setDelay(delayTime).setEaseType(GoEaseType.CubicInOut).positionPath(path)
					.scale(endValue));
				GoTween tween2 = new GoTween(go.transform, flyTime, new GoTweenConfig().setDelay(delayTime).setEaseType((!dooberModel.RewardedAdDooberMode) ? GoEaseType.QuartIn : GoEaseType.CubicInOut).positionPath(path2)
					.scale(0.3f)
					.onComplete(delegate(AbstractGoTween thisTween)
					{
						switch (tweenType)
						{
						case DestinationType.GRIND:
							setGrindCurrencySignal.Dispatch();
							fireGrindSignal.Dispatch();
							break;
						case DestinationType.PREMIUM:
							setPremiumCurrencySignal.Dispatch();
							firePremiumSignal.Dispatch();
							break;
						case DestinationType.XP:
							setXPSignal.Dispatch();
							fireXpSignal.Dispatch();
							break;
						case DestinationType.MINIONS:
							setLevelSignal.Dispatch();
							break;
						case DestinationType.STORAGE:
						case DestinationType.STORAGE_POPULATION_GOAL:
							setStorageSignal.Dispatch();
							break;
						case DestinationType.MINION_LEVEL_TOKEN:
							tokenDooberCompleteSignal.Dispatch();
							break;
						case DestinationType.MYSTERY_BOX:
							HandleMysteryBox();
							break;
						}
						if (dooberModel.DooberCounter == 1)
						{
							soundFXSignal.Dispatch("Play_icon_sparkle_01");
						}
						thisTween.destroy();
						UnityEngine.Object.Destroy(go);
						dooberModel.DooberCounter--;
						doobersFlownSignal.Dispatch();
						Release();
					}));
				GoTweenFlow goTweenFlow = new GoTweenFlow();
				if (dooberModel.RewardedAdDooberMode)
				{
					goTweenFlow.insert(0f, tween2);
				}
				else
				{
					goTweenFlow.insert(0f, tween);
					goTweenFlow.insert(flyTime + 0.5f, tween2);
				}
				goTweenFlow.play();
				return;
			}
			List<Vector3> nodes3 = CreatePath(go.transform.position, destination) as List<Vector3>;
			GoSpline path3 = new GoSpline(nodes3);
			Go.to(go.transform, flyTime, new GoTweenConfig().setDelay(delayTime).setEaseType(GoEaseType.CubicInOut).positionPath(path3)
				.scale(0.8f)
				.onComplete(delegate(AbstractGoTween thisTween)
				{
					if (dooberModel.DooberCounter == 1)
					{
						soundFXSignal.Dispatch("Play_icon_sparkle_01");
					}
					thisTween.destroy();
					UnityEngine.Object.Destroy(go);
					dooberModel.DooberCounter--;
					Release();
				}));
		}

		private string DetermineAudioToFire(DestinationType tweenType)
		{
			string result = "Play_loot_pick_up_01";
			if (tweenType == DestinationType.STORAGE_POPULATION_GOAL || tweenType == DestinationType.TIMER_POPULATION_GOAL || tweenType == DestinationType.MINION_LEVEL_TOKEN || tweenType == DestinationType.MYSTERY_BOX)
			{
				result = "Play_mysteryBox_harvest_01";
			}
			return result;
		}

		private void HandleMysteryBox()
		{
			if (itemDefinitionId == 1)
			{
				setPremiumCurrencySignal.Dispatch();
				firePremiumSignal.Dispatch();
			}
			else
			{
				setStorageSignal.Dispatch();
			}
		}

		private Vector3 GetWorldPositionInUI(DestinationType tweenType, out bool isMignette)
		{
			Vector3 result = Vector3.zero;
			isMignette = false;
			if (dooberModel.RewardedAdDooberMode)
			{
				return dooberModel.rewardedAdDooberSpawnLocation;
			}
			switch (tweenType)
			{
			case DestinationType.XP:
				result = UICamera.ViewportToWorldPoint(dooberModel.expScreenPosition);
				break;
			case DestinationType.PREMIUM:
				result = UICamera.ViewportToWorldPoint(dooberModel.premiumScreenPosition);
				break;
			case DestinationType.GRIND:
				result = UICamera.ViewportToWorldPoint(dooberModel.grindScreenPosition);
				break;
			case DestinationType.BUFF:
				result = UICamera.ViewportToWorldPoint(dooberModel.itemScreenPosition);
				break;
			case DestinationType.MINIONS:
				result = UICamera.ViewportToWorldPoint(dooberModel.expScreenPosition);
				break;
			case DestinationType.MIGNETTE:
				isMignette = true;
				break;
			default:
				result = UICamera.ViewportToWorldPoint(dooberModel.itemScreenPosition);
				break;
			}
			return result;
		}

		protected IList<Vector3> CreatePath(Vector3 start, Vector3 destination)
		{
			List<Vector3> list = new List<Vector3>();
			Vector3 vector = default(Vector3);
			Vector3 vector2 = default(Vector3);
			vector = start - 0.3f * (start - destination);
			vector2 = start - 0.6f * (start - destination);
			System.Random random = new System.Random();
			if (random.Next(2) == 0)
			{
				vector.x += (float)random.Next(50, 100) / 25f;
				vector2.x -= (float)random.Next(50, 100) / 25f;
			}
			else
			{
				vector.x -= (float)random.Next(50, 100) / 25f;
				vector2.x += (float)random.Next(50, 100) / 25f;
			}
			list.Add(start);
			list.Add(vector);
			list.Add(vector2);
			list.Add(destination);
			return list;
		}

		protected Vector2 GetScreenStartPosition()
		{
			Vector2 vector = ((iconPosition == Vector3.zero) ? ((Vector2)UICamera.ViewportToWorldPoint(dooberModel.defaultDooberSpawnLocation)) : ((!fromWorldCanvas) ? ((Vector2)iconPosition) : ((Vector2)Camera.main.WorldToScreenPoint(iconPosition))));
			return vector / UIUtils.GetHeightScale();
		}

		public void Restore()
		{
		}

		public void Retain()
		{
			retain = true;
		}

		public void Release()
		{
			retain = false;
			if (commandPool != null)
			{
				commandPool.ReturnToPool(this);
			}
		}
	}
}
