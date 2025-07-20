using System;

namespace Kampai.Common
{
	public interface IVideoService
	{
		void playVideo(string urlOrFilename, bool showControls, bool closeOnTouch);

		void playIntro(bool showControls, bool closeOnTouch, Action videoPlayingCallback = null, string videoUriTemplate = null);

		bool IsIntroCached(string videoUriTemplate = null);
	}
}
