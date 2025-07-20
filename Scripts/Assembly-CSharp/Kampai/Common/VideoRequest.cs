using System;
using Ea.Sharkbite.HttpPlugin.Http.Api;

namespace Kampai.Common
{
	internal sealed class VideoRequest
	{
		public const int PROGRESS_GATE = 30;

		public bool showControls;

		public bool closeOnTouch;

		public int retries = 3;

		public int progressBarStart;

		public int progressBarNow = 30;

		public IRequest networkRequest;

		public Action callback;

		public string videoUriTemplate;
	}
}
