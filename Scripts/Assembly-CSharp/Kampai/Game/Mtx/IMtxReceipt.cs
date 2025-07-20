namespace Kampai.Game.Mtx
{
	public class IMtxReceipt
	{
		public enum PlatformStore
		{
			AppleAppStore = 0,
			GooglePlay = 1,
			AmazonAppStore = 2
		}

		public PlatformStore platformStore { get; set; }
	}
}
