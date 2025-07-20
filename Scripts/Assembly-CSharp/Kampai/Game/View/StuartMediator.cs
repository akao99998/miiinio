using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.Game.View
{
	public class StuartMediator : FrolicCharacterMediator
	{
		[Inject]
		public StuartView stuartView { get; set; }

		[Inject]
		public StuartAddToStageSignal addToStageSignal { get; set; }

		[Inject]
		public StuartStartPerformingSignal startPerformingSignal { get; set; }

		[Inject]
		public StuartGetOnStageImmediateSignal getOnStageImmediateSignal { get; set; }

		[Inject]
		public StuartTicketFilledSignal stuartTicketFilledSignal { get; set; }

		[Inject]
		public StuartTunesGuitarSignal stuartTunesGuitarSignal { get; set; }

		[Inject]
		public SocialEventAvailableSignal socialEventAvailableSignal { get; set; }

		[Inject]
		public RestoreStuartSignal restoreStuartSignal { get; set; }

		[Inject]
		public StuartShowCompleteSignal stuartShowCompleteSignal { get; set; }

		[Inject]
		public StopLocalAudioSignal stopLocalAudioSignal { get; set; }

		[Inject]
		public StopStuartPerformanceAudioSignal stopPerformanceAudioSignal { get; set; }

		[Inject]
		public StuartSpinMicSignal spinMicSignal { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			addToStageSignal.AddListener(AddToStage);
			startPerformingSignal.AddListener(StartPerforming);
			getOnStageImmediateSignal.AddListener(GetOnStageImmediate);
			stuartTicketFilledSignal.AddListener(TicketFilled);
			stuartTunesGuitarSignal.AddListener(StuartTunesGuitar);
			stuartShowCompleteSignal.AddListener(RestoreStuart);
			stopPerformanceAudioSignal.AddListener(StopPerformanceAudio);
			stuartView.OnSpinMic = OnSpinMic;
		}

		public override void OnRemove()
		{
			base.OnRemove();
			addToStageSignal.RemoveListener(AddToStage);
			startPerformingSignal.RemoveListener(StartPerforming);
			getOnStageImmediateSignal.RemoveListener(GetOnStageImmediate);
			stuartTicketFilledSignal.RemoveListener(TicketFilled);
			stuartTunesGuitarSignal.RemoveListener(StuartTunesGuitar);
			stuartShowCompleteSignal.RemoveListener(RestoreStuart);
			stopPerformanceAudioSignal.RemoveListener(StopPerformanceAudio);
		}

		private void StuartTunesGuitar()
		{
			stuartView.GetOnStage(true);
			stuartView.TuneGuitar(delegate
			{
				socialEventAvailableSignal.Dispatch();
			});
		}

		private void OnSpinMic()
		{
			spinMicSignal.Dispatch();
		}

		private void RestoreStuart()
		{
			restoreStuartSignal.Dispatch();
		}

		private void AddToStage(Vector3 position, Quaternion rotation, StuartStageAnimationType animType)
		{
			stuartView.AddToStage(position, rotation, animType);
		}

		private void StartPerforming(SignalCallback<Signal> finishedCallback)
		{
			stuartView.GetOnStage(true);
			stuartView.Perform(finishedCallback);
		}

		private void StopPerformanceAudio()
		{
			stopLocalAudioSignal.Dispatch(base.gameObject.GetComponent<CustomFMOD_StudioEventEmitter>());
		}

		private void GetOnStageImmediate(bool enable)
		{
			stuartView.GetOnStageImmediate(enable);
		}

		private void TicketFilled()
		{
			stuartView.StartingState(StuartStageAnimationType.CELEBRATE, true);
		}
	}
}
