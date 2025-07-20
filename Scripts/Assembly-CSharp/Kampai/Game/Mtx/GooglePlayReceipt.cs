namespace Kampai.Game.Mtx
{
	public class GooglePlayReceipt : IMtxReceipt
	{
		public string signedData { get; set; }

		public string signature { get; set; }

		public GooglePlayReceipt()
		{
			base.platformStore = PlatformStore.GooglePlay;
		}

		public override string ToString()
		{
			return string.Format("GooglePlayReceipt: signedData: {0}, signature: {1}", signedData ?? "null", signature);
		}
	}
}
