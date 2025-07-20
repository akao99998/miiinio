using Kampai.Common.Service.Audio;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class LoadAudioCommand : Command
	{
		[Inject]
		public IFMODService fmodService { get; set; }

		[Inject]
		public ICoroutineProgressMonitor couroutineProgressMonitor { get; set; }

		public override void Execute()
		{
			couroutineProgressMonitor.StartTask(fmodService.InitializeSystem(), "fmod");
		}
	}
}
