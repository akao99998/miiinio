using strange.extensions.command.impl;

namespace Kampai.Game.Mignette
{
	public class ChangeCurrentMignetteScore : Command
	{
		[Inject]
		public int scoreDelta { get; set; }

		[Inject]
		public MignetteGameModel mignetteGameModel { get; set; }

		[Inject]
		public MignetteScoreUpdatedSignal mignetteScoreUpdatedSignal { get; set; }

		public override void Execute()
		{
			mignetteGameModel.CurrentGameScore += scoreDelta;
			mignetteScoreUpdatedSignal.Dispatch(mignetteGameModel.CurrentGameScore);
		}
	}
}
