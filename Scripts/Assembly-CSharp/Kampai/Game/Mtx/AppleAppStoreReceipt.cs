namespace Kampai.Game.Mtx
{
	public class AppleAppStoreReceipt : IMtxReceipt
	{
		public string receipt { get; set; }

		public AppleAppStoreReceipt()
		{
			base.platformStore = PlatformStore.AppleAppStore;
		}

		public override string ToString()
		{
			return string.Format("AppleAppStoreReceipt: receipt: {0}", receipt ?? "null");
		}
	}
}
