using Kampai.Game;
using Kampai.Main;
using strange.extensions.command.impl;

public class DLCLevelChangedCommand : Command
{
	[Inject]
	public IDLCService dlcService { get; set; }

	[Inject]
	public ILocalContentService localContentService { get; set; }

	public override void Execute()
	{
		string downloadQualityLevel = dlcService.GetDownloadQualityLevel();
		localContentService.SetDLCQuality(downloadQualityLevel);
	}
}
