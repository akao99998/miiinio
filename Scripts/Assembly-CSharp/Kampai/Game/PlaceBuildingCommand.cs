using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	internal sealed class PlaceBuildingCommand : Command
	{
		[Inject]
		public int buildingId { get; set; }

		[Inject]
		public Location location { get; set; }

		[Inject]
		public RerouteMinionPathsSignal rerouteSignal { get; set; }

		[Inject]
		public ShowBuildingFootprintSignal showFootprintSignal { get; set; }

		[Inject]
		public RemoveBuildingSignal removeSignal { get; set; }

		[Inject]
		public AddFootprintSignal addFootprintSignal { get; set; }

		[Inject]
		public IBuildingUtilities buildingUtil { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public DebugUpdateGridSignal debugUpdateGridSignal { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public DecoGridModel decoGridModel { get; set; }

		[Inject]
		public UpdateConnectablesSignal updateConnectablesSignal { get; set; }

		public override void Execute()
		{
			Building byInstanceId = playerService.GetByInstanceId<Building>(buildingId);
			if (buildingUtil.ValidateLocation(byInstanceId, this.location))
			{
				ConnectableBuilding connectableBuilding = byInstanceId as ConnectableBuilding;
				ConnectableBuildingDefinition connectableBuildingDefinition = null;
				if (connectableBuilding != null)
				{
					connectableBuildingDefinition = connectableBuilding.Definition;
					Location location = byInstanceId.Location;
					decoGridModel.RemoveDeco(location.x, location.y);
					updateConnectablesSignal.Dispatch(location, connectableBuildingDefinition.connectableType);
				}
				showFootprintSignal.Dispatch(null, null, Tuple.Create(1, 1), false);
				string buildingFootprint = definitionService.GetBuildingFootprint(byInstanceId.Definition.FootprintID);
				removeSignal.Dispatch(byInstanceId.Location, buildingFootprint);
				if (byInstanceId.IsFootprintable)
				{
					addFootprintSignal.Dispatch(byInstanceId, this.location);
				}
				byInstanceId.Location = this.location;
				if (connectableBuilding != null && connectableBuildingDefinition != null)
				{
					decoGridModel.AddDeco(this.location.x, this.location.y, connectableBuildingDefinition.connectableType);
					updateConnectablesSignal.Dispatch(this.location, connectableBuildingDefinition.connectableType);
				}
				Point ll = default(Point);
				Point ur = default(Point);
				GetBounds(this.location, buildingFootprint, out ll, out ur);
				rerouteSignal.Dispatch(new Tuple<Point, Point>(ll, ur));
				debugUpdateGridSignal.Dispatch();
			}
		}

		private void GetBounds(Location loc, string footprint, out Point ll, out Point ur)
		{
			int num = loc.x;
			int num2 = loc.y;
			ll.x = (ll.y = int.MaxValue);
			ur.x = (ur.y = int.MinValue);
			for (int i = 0; i < footprint.Length; i++)
			{
				switch (footprint[i])
				{
				case '|':
					num = loc.x;
					num2--;
					continue;
				case '.':
					num++;
					continue;
				}
				if (num < ll.x)
				{
					ll.x = num;
				}
				if (num2 < ll.y)
				{
					ll.y = num2;
				}
				if (num > ur.x)
				{
					ur.x = num;
				}
				if (num2 > ur.y)
				{
					ur.y = num2;
				}
				num++;
			}
		}
	}
}
