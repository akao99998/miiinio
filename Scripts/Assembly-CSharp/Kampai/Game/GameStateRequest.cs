using Newtonsoft.Json;

namespace Kampai.Game
{
	public class GameStateRequest
	{
		[JsonProperty("playergamestate")]
		public string PlayerGameState { get; set; }
	}
}
