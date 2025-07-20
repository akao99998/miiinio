using Elevation.Logging;
using Kampai.Game;
using Kampai.Util;
using strange.extensions.command.impl;

public class UserRegisteredCommand : Command
{
	public IKampaiLogger logger = LogManager.GetClassLogger("UserRegisteredCommand") as IKampaiLogger;

	[Inject]
	public UserIdentity Identity { get; set; }

	[Inject]
	public ILocalPersistanceService LocalPersistService { get; set; }

	[Inject]
	public IEncryptionService encryptionService { get; set; }

	[Inject]
	public LoginUserSignal LoginUserSignal { get; set; }

	public override void Execute()
	{
		LocalPersistService.PutData("UserID", Identity.UserID);
		string text = encryptionService.Encrypt(Identity.ExternalID, "Kampai!");
		string text2 = encryptionService.Encrypt(Identity.ID, "Kampai!");
		LocalPersistService.PutData("AnonymousSecret", text);
		LocalPersistService.PutData("AnonymousID", text2);
		logger.Log(KampaiLogLevel.Info, "Saved user's credentials");
		LoginUserSignal.Dispatch();
	}
}
