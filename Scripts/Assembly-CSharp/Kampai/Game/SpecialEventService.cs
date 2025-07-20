namespace Kampai.Game
{
	public class SpecialEventService : ISpecialEventService
	{
		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		public bool IsSpecialEventActive()
		{
			foreach (SpecialEventItemDefinition item in definitionService.GetAll<SpecialEventItemDefinition>())
			{
				SpecialEventItem firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<SpecialEventItem>(item.ID);
				if (firstInstanceByDefinitionId != null && !firstInstanceByDefinitionId.HasEnded && item.IsActive)
				{
					return true;
				}
			}
			return false;
		}
	}
}
