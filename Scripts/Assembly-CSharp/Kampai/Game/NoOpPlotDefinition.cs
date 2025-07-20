using Kampai.Util;

namespace Kampai.Game
{
	internal sealed class NoOpPlotDefinition : PlotDefinition, IBuilder<Instance>
	{
		public override int TypeCode
		{
			get
			{
				return 1127;
			}
		}

		public Instance Build()
		{
			return new NoOpPlot(this);
		}

		public override Plot Instantiate()
		{
			return new NoOpPlot(this);
		}
	}
}
