using System.Collections.Generic;

namespace Kampai.Game
{
	public class LandExpansionConfigService : ILandExpansionConfigService
	{
		private Dictionary<int, LandExpansionConfig> expansionConfigLookup = new Dictionary<int, LandExpansionConfig>();

		[Inject]
		public IDefinitionService definitionService { get; set; }

		public IList<int> GetExpansionIds()
		{
			TryInitialize();
			return new List<int>(expansionConfigLookup.Keys);
		}

		public LandExpansionConfig GetExpansionConfig(int expansion)
		{
			TryInitialize();
			LandExpansionConfig value = null;
			expansionConfigLookup.TryGetValue(expansion, out value);
			return value;
		}

		private void TryInitialize()
		{
			if (expansionConfigLookup.Count != 0)
			{
				return;
			}
			foreach (LandExpansionConfig item in definitionService.GetAll<LandExpansionConfig>())
			{
				expansionConfigLookup.Add(item.expansionId, item);
			}
		}
	}
}
