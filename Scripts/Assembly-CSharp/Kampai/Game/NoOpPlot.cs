namespace Kampai.Game
{
	internal sealed class NoOpPlot : Plot<NoOpPlotDefinition>
	{
		public NoOpPlot(NoOpPlotDefinition definition)
			: base(definition)
		{
		}
	}
}
