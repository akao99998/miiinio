using Kampai.Game.View;

public class MinionStateAudioArgs
{
	public ActionableObject source { get; set; }

	public string audioEvent { get; set; }

	public string emitterKey { get; set; }

	public float cueId { get; set; }
}
