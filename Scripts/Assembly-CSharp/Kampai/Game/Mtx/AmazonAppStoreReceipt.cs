namespace Kampai.Game.Mtx
{
	public class AmazonAppStoreReceipt : IMtxReceipt
	{
		public string amazonUserId { get; set; }

		public string receipt { get; set; }

		public AmazonAppStoreReceipt()
		{
			base.platformStore = PlatformStore.AmazonAppStore;
		}

		public override string ToString()
		{
			return string.Format("AmazonAppStoreReceipt: amazonUserId: {0}, receipt: {1}", amazonUserId ?? "null", receipt);
		}
	}
}
