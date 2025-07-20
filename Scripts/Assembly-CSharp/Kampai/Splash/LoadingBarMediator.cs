using System.Collections.Generic;
using Ea.Sharkbite.HttpPlugin.Http.Api;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.Splash
{
	public class LoadingBarMediator : Mediator
	{
		private float start;

		private float target;

		private float current;

		private float timeTarget;

		private float timeRemaining;

		private bool dlcMode;

		private IDictionary<string, DownloadProgress> dlcProgess = new Dictionary<string, DownloadProgress>();

		private bool isDlcStale;

		[Inject]
		public LoadingBarView view { get; set; }

		[Inject]
		public SplashProgressUpdateSignal splashProgressUpdateSignal { get; set; }

		[Inject]
		public SetSplashProgressSignal setSplashProgressSignal { get; set; }

		[Inject]
		public DownloadInitializeSignal downloadInitializeSignal { get; set; }

		[Inject]
		public DownloadProgressSignal downloadProgressSignal { get; set; }

		[Inject]
		public DownloadResponseSignal downloadResponseSignal { get; set; }

		[Inject]
		public DLCDownloadFinishedSignal downloadFinishedSignal { get; set; }

		[Inject]
		public DLCModel dlcModel { get; set; }

		[Inject]
		public DLCLoadScreenModel model { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		public override void OnRegister()
		{
			splashProgressUpdateSignal.AddListener(OnSplashProgressUpdate);
			setSplashProgressSignal.AddListener(OnSetSplashProgress);
			downloadInitializeSignal.AddListener(InitializeDLC);
			downloadProgressSignal.AddListener(OnDownloadProgress);
			downloadResponseSignal.AddListener(OnDownloadResponse);
			downloadFinishedSignal.AddListener(OnDownloadFinish);
			view.Init();
			if (!dlcMode)
			{
				current = model.CurrentLoadProgress;
				UpdateView();
			}
		}

		public override void OnRemove()
		{
			splashProgressUpdateSignal.RemoveListener(OnSplashProgressUpdate);
			setSplashProgressSignal.RemoveListener(OnSetSplashProgress);
			downloadInitializeSignal.RemoveListener(InitializeDLC);
			downloadProgressSignal.RemoveListener(OnDownloadProgress);
			downloadResponseSignal.RemoveListener(OnDownloadResponse);
			downloadFinishedSignal.RemoveListener(OnDownloadFinish);
			model.CurrentLoadProgress = current;
		}

		private void OnSplashProgressUpdate(int target, float time)
		{
			this.target += target;
			start = current;
			if (this.target > 100f)
			{
				this.target = 100f;
			}
			timeRemaining = time;
			timeTarget = time;
		}

		private void OnSetSplashProgress(float progress)
		{
			start = (current = (target = Mathf.Min(progress, 100f)));
			timeRemaining = (timeTarget = 0f);
		}

		private void Update()
		{
			float currentProgress = GetCurrentProgress();
			if (!dlcMode && timeRemaining > 0f && timeTarget > 0f)
			{
				float deltaTime = Time.deltaTime;
				timeRemaining -= deltaTime;
				float num = (target - start) * (deltaTime / timeTarget);
				if (num > 0f)
				{
					current = Mathf.Min(current + num, 100f);
				}
			}
			else if (dlcMode && isDlcStale)
			{
				isDlcStale = false;
				long num2 = 0L;
				foreach (DownloadProgress value in dlcProgess.Values)
				{
					num2 += value.CompletedBytes;
				}
				model.CurrentProgress = Mathf.Min((float)num2 / 1048576f, model.TotalSize);
			}
			if (currentProgress != GetCurrentProgress())
			{
				UpdateView();
			}
		}

		private float GetCurrentProgress()
		{
			return dlcMode ? model.CurrentProgress : current;
		}

		private void UpdateView()
		{
			float num = (dlcMode ? (model.CurrentProgress / model.TotalSize * 100f) : current);
			view.SetText(dlcMode ? localizationService.GetString("DLCIndicatorProgress", Mathf.RoundToInt(model.CurrentProgress), Mathf.Max(1, Mathf.RoundToInt(model.TotalSize))) : string.Format("{0:0}%", num));
			view.SetMeterFill(num);
		}

		private void ToggleDlcMode(bool isEnabled)
		{
			if (dlcMode != isEnabled)
			{
				dlcMode = isEnabled;
				UpdateView();
			}
		}

		private void InitializeDLC(ulong size)
		{
			model.TotalSize = (float)size / 1048576f;
			model.CurrentProgress = 0f;
			dlcProgess.Clear();
			foreach (BundleInfo neededBundle in dlcModel.NeededBundles)
			{
				dlcProgess[neededBundle.name] = new DownloadProgress(neededBundle.name)
				{
					TotalBytes = (long)neededBundle.size,
					CompressionRatio = (float)neededBundle.size / (float)neededBundle.zipsize
				};
			}
			isDlcStale = true;
			ToggleDlcMode(true);
		}

		private void OnDownloadProgress(DownloadProgress progress, IRequest request)
		{
			DownloadProgress value;
			if (dlcProgess.TryGetValue(DownloadUtil.GetBundleNameFromUrl(request.Uri), out value))
			{
				value.CompletedBytes = ((!progress.IsGZipped) ? progress.CompletedBytes : ((long)((float)progress.CompletedBytes * value.CompressionRatio)));
				isDlcStale = true;
			}
		}

		private void OnDownloadResponse(IResponse response)
		{
			DownloadProgress value;
			if (!response.Success && dlcProgess.TryGetValue(DownloadUtil.GetBundleNameFromUrl(response.Request.Uri), out value))
			{
				value.CompletedBytes = 0L;
				isDlcStale = true;
			}
		}

		private void OnDownloadFinish()
		{
			ToggleDlcMode(false);
			dlcProgess.Clear();
			isDlcStale = false;
		}
	}
}
