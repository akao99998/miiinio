using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class InterpolateSaleTimeCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("InterpolateSaleTimeCommand") as IKampaiLogger;

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IMarketplaceService marketplaceService { get; set; }

		[Inject]
		public MarketplaceSaleItem marketplaceItem { get; set; }

		public override void Execute()
		{
			MarketplaceDefinition marketplaceDefinition = definitionService.Get<MarketplaceDefinition>();
			List<Vector3> list = (List<Vector3>)marketplaceDefinition.buyTimeSpline;
			if (list == null)
			{
				logger.Error("Definition for the marketplace sale time spline is null");
				return;
			}
			AbstractGoSplineSolver abstractGoSplineSolver = CreateSplineSolver(list);
			abstractGoSplineSolver.buildPath();
			float normalizedPrice = GetNormalizedPrice();
			Vector3 point = abstractGoSplineSolver.getPoint(normalizedPrice);
			marketplaceItem.LengthOfSale = GetTimeFromParameter(point.y);
		}

		private float GetNormalizedPrice()
		{
			int num = marketplaceItem.Definition.MinStrikePrice * marketplaceItem.QuantitySold;
			int num2 = marketplaceItem.Definition.MaxStrikePrice * marketplaceItem.QuantitySold;
			return (float)(marketplaceItem.SalePrice - num) / (float)(num2 - num);
		}

		private int GetTimeFromParameter(float t)
		{
			t = Mathf.Clamp01(t);
			int lowPriceBuyTimeSeconds = marketplaceItem.Definition.LowPriceBuyTimeSeconds;
			int highPriceBuyTimeSeconds = marketplaceItem.Definition.HighPriceBuyTimeSeconds;
			return (int)((float)Mathf.FloorToInt((float)lowPriceBuyTimeSeconds + (float)(highPriceBuyTimeSeconds - lowPriceBuyTimeSeconds) * t) * marketplaceService.DebugMultiplier);
		}

		private AbstractGoSplineSolver CreateSplineSolver(List<Vector3> nodes)
		{
			if (nodes.Count == 2)
			{
				return new GoSplineStraightLineSolver(nodes);
			}
			if (nodes.Count == 3)
			{
				return new GoSplineQuadraticBezierSolver(nodes);
			}
			if (nodes.Count == 4)
			{
				return new GoSplineCubicBezierSolver(nodes);
			}
			return new GoSplineCatmullRomSolver(nodes);
		}
	}
}
