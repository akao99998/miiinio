using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Game.Trigger;
using Kampai.Util;

public class JsonConverters
{
	public FastJsonConverter<TransactionDefinition> transactionDefinitionConverter;

	public FastJsonConverter<AdPlacementDefinition> adPlacementDefinitionConverter;

	public FastJsonConverter<TriggerConditionDefinition> triggerConditionDefinitionConverter;

	public FastJsonConverter<BuildingDefinition> buildingDefinitionConverter;

	public FastJsonConverter<NamedCharacterDefinition> namedCharacterDefinitionConverter;

	public FastJsonConverter<FrolicCharacterDefinition> frolicCharacterDefinitionConverter;

	public FastJsonConverter<ItemDefinition> itemDefinitionConverter;

	public FastJsonConverter<PlotDefinition> plotDefinitionConverter;

	public FastJsonConverter<QuestDefinition> questDefinitionConverter;

	public FastJsonConverter<SalePackDefinition> salePackDefinitionConverter;

	public FastJsonConverter<CurrencyItemDefinition> currencyItemDefinitionConverter;

	public FastJsonConverter<CurrencyStorePackDefinition> currencyStorePackDefinitionConverter;

	public FastJsonConverter<TriggerDefinition> triggerDefinitionConverter;

	public FastJsonConverter<TriggerRewardDefinition> triggerRewardDefinitionConverter;

	public FastJsonConverter<Instance> instanceConverter;

	public FastJsonConverter<PlayerVersion> playerVersionConverter;

	public FastJsonConverter<TriggerInstance> triggerInstanceConverter;

	public FastJsonConverter<SocialTeam> socialTeamConverter;
}
