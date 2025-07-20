using Kampai.Game.Transaction;

namespace Kampai.Game
{
	public interface ITelemetryUtil
	{
		void DetermineTaxonomy(TransactionUpdateData update, bool concatenateType, out string highLevel, out string specific, out string type, out string other);

		string GetSourceName(TransactionUpdateData update);
	}
}
