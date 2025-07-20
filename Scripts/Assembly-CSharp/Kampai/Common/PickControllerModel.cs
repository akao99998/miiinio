using System.Collections.Generic;
using Kampai.Game;
using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Common
{
	public class PickControllerModel
	{
		public enum Mode
		{
			None = 0,
			Building = 1,
			DragAndDrop = 2,
			MagnetFinger = 3,
			Minion = 4,
			EnvironmentalMignette = 5,
			LandExpansion = 6,
			TikiBarView = 7,
			StageView = 8,
			VillainIsland = 9,
			VillainLair = 10
		}

		public const float maxTouchDelta = 15f;

		public const float MagnetFingerTheshold = 1f;

		public const float BaseDurationBetweenMinions = 0.2f;

		public const float DurationReductionPerSecond = 0.06f;

		public const float TimeForDoubleClick = 0.2f;

		public float DurationBetweenMinions = 0.2f;

		private List<int> ignoredInstances = new List<int>();

		private int SkrimCounter;

		private bool forceDisabled;

		public Mode CurrentMode { get; set; }

		public bool PreviousPressState { get; set; }

		public Vector3 StartTouchPosition { get; set; }

		public Dictionary<int, SelectedMinionModel> SelectedMinions { get; set; }

		public int? SelectedBuilding { get; set; }

		public bool activitySpinnerExists { get; set; }

		public GameObject StartHitObject { get; set; }

		public GameObject EndHitObject { get; set; }

		public float StartTouchTimeMs { get; set; }

		public bool InvalidateMovement { get; set; }

		public bool DetectedMovement { get; set; }

		public bool ValidLocation { get; set; }

		public bool Blocked { get; set; }

		public float HeldTimer { get; set; }

		public MinionManagerView MMView { get; set; }

		public Queue<int> Minions { get; set; }

		public float CurrentMagnetFingerTimer { get; set; }

		public Queue<Point> Points { get; set; }

		public Point MainPoint { get; set; }

		public Vector3 DragPreviousPosition { get; set; }

		public DragOffsetType OffsetType { get; set; }

		public float LastClickTime { get; set; }

		public Vector2 LastBuildingStorePosition { get; set; }

		public bool WaitingForDouble { get; set; }

		public bool HasPlayedGacha { get; set; }

		public bool HasPlayedSFX { get; set; }

		public Object MinionMoveToIndicator { get; set; }

		public bool Enabled
		{
			get
			{
				return SkrimCounter == 0 && !forceDisabled;
			}
		}

		public bool PanningCameraBlocked { get; set; }

		public bool ZoomingCameraBlocked { get; set; }

		public GameObject minionMoveIndicator { get; set; }

		public bool ForceDisabled
		{
			get
			{
				return forceDisabled;
			}
			set
			{
				forceDisabled = value;
			}
		}

		public PickControllerModel()
		{
			SelectedMinions = new Dictionary<int, SelectedMinionModel>();
			SelectedBuilding = null;
		}

		public void SetIgnoreInstance(int instanceId, bool ignored)
		{
			if (ignored)
			{
				if (!IsInstanceIgnored(instanceId))
				{
					ignoredInstances.Add(instanceId);
				}
			}
			else
			{
				ignoredInstances.Remove(instanceId);
			}
		}

		public bool IsInstanceIgnored(int instanceId)
		{
			return ignoredInstances.Contains(instanceId);
		}

		public void IncreaseSkrimCounter()
		{
			SkrimCounter++;
		}

		public void DecreaseSkrimCounter()
		{
			SkrimCounter--;
			if (SkrimCounter <= 0)
			{
				SkrimCounter = 0;
			}
		}
	}
}
