using System;
using System.Collections.Generic;
using Kampai.Common;
using Kampai.Game;
using Kampai.Main;

namespace Kampai.Splash
{
	public class LoadInService : ILoadInService
	{
		private const int NB_TIPS_TO_SAVE = 10;

		public IRandomService randomService;

		private float defaultTipCycleTime = 3f;

		private IList<string> defaultLoadTips;

		private IList<TipToShow> savedTips;

		private SavedTipsModel model = new SavedTipsModel();

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		public LoadInService()
		{
			InitDefaultTips();
			InitSavedTips();
			long seed = DateTime.Now.Second;
			randomService = new RandomService(seed);
		}

		private void InitDefaultTips()
		{
			defaultLoadTips = new List<string>();
			defaultLoadTips.Add("defaultTip001");
			defaultLoadTips.Add("defaultTip002");
			defaultLoadTips.Add("defaultTip003");
			defaultLoadTips.Add("defaultTip004");
			defaultLoadTips.Add("defaultTip005");
			defaultLoadTips.Add("defaultTip006");
			defaultLoadTips.Add("defaultTip007");
			defaultLoadTips.Add("defaultTip008");
			defaultLoadTips.Add("defaultTip009");
			defaultLoadTips.Add("defaultTip010");
			defaultLoadTips.Add("defaultTip011");
		}

		private void InitSavedTips()
		{
			savedTips = new List<TipToShow>(model.Tips);
		}

		public TipToShow GetNextTip()
		{
			if (model.Tips.Count > 0 && model.TipsLocale.Equals(localizationService.GetLanguage()))
			{
				if (savedTips.Count == 0)
				{
					InitSavedTips();
				}
				int index = randomService.NextInt(savedTips.Count);
				TipToShow result = savedTips[index];
				savedTips.RemoveAt(index);
				return result;
			}
			if (defaultLoadTips.Count == 0)
			{
				InitDefaultTips();
			}
			int index2 = randomService.NextInt(defaultLoadTips.Count);
			TipToShow result2 = new TipToShow(localizationService.GetString(defaultLoadTips[index2]), defaultTipCycleTime);
			defaultLoadTips.RemoveAt(index2);
			return result2;
		}

		public void SaveTipsForNextLaunch(int level)
		{
			List<TipToShow> randomTipsFromCurrentBucket = GetRandomTipsFromCurrentBucket(level, 10);
			model.SaveTipsToDevice(randomTipsFromCurrentBucket, localizationService.GetLanguage());
		}

		private List<TipToShow> GetRandomTipsFromCurrentBucket(int level, int nbTips)
		{
			LoadinTipBucketDefinition bucket = GetBucket(level);
			List<LoadInTipDefinition> all = definitionService.GetAll<LoadInTipDefinition>();
			List<TipToShow> list = new List<TipToShow>();
			foreach (LoadInTipDefinition item in all)
			{
				foreach (BucketAssignment bucket2 in item.Buckets)
				{
					if (bucket2.BucketId == bucket.ID)
					{
						list.Add(new TipToShow(localizationService.GetString(item.Text), bucket2.Time));
						break;
					}
				}
			}
			if (list.Count <= nbTips)
			{
				return list;
			}
			List<TipToShow> list2 = new List<TipToShow>();
			for (int i = 0; i < nbTips; i++)
			{
				int index = randomService.NextInt(list.Count);
				list2.Add(list[index]);
				list.RemoveAt(index);
			}
			return list2;
		}

		private LoadinTipBucketDefinition GetBucket(int level)
		{
			IList<LoadinTipBucketDefinition> all = definitionService.GetAll<LoadinTipBucketDefinition>();
			LoadinTipBucketDefinition loadinTipBucketDefinition = null;
			foreach (LoadinTipBucketDefinition item in all)
			{
				if (loadinTipBucketDefinition == null || loadinTipBucketDefinition.Max < item.Max)
				{
					loadinTipBucketDefinition = item;
				}
				if (level >= item.Min && level <= item.Max)
				{
					return item;
				}
			}
			return loadinTipBucketDefinition;
		}
	}
}
