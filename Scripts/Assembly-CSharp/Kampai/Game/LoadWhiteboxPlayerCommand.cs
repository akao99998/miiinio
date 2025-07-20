using Elevation.Logging;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class LoadWhiteboxPlayerCommand : Command
	{
		public const string PLAYER_DATA_ENDPOINT = "/rest/gamestate/{0}";

		public IKampaiLogger logger = LogManager.GetClassLogger("LoadWhiteboxPlayerCommand") as IKampaiLogger;

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject("devPlayerPath")]
		public string devPlayerFilename { get; set; }

		[Inject]
		public IResourceService resourceService { get; set; }

		[Inject]
		public ILocalPersistanceService localPersistService { get; set; }

		public override void Execute()
		{
			string empty = string.Empty;
			empty = resourceService.LoadText(devPlayerFilename);
			localPersistService.PutData("LoadMode", "default");
			localPersistService.PutData("LocalFileName", string.Empty);
			localPersistService.PutData("LocalID", string.Empty);
			if (empty.Length != 0)
			{
				playerService.Deserialize(empty);
			}
			else
			{
				logger.Fatal(FatalCode.CMD_LOAD_PLAYER);
			}
		}
	}
}
