using System;
using System.Collections.Generic;
using Kampai.Game.View;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	public class OrderBoard : RepairableBuilding<BlackMarketBoardDefinition>, Building, ZoomableBuilding, Instance, Locatable, IFastJSONDeserializable, IFastJSONSerializable, Identifiable
	{
		public IList<OrderBoardTicket> tickets { get; set; }

		public IList<int> PriorityPrestigeDefinitionIDs { get; set; }

		public int HarvestableCharacterDefinitionId { get; set; }

		[JsonIgnore]
		public ZoomableBuildingDefinition ZoomableDefinition
		{
			get
			{
				return base.Definition;
			}
		}

		[JsonIgnore]
		public bool menuEnabled { get; set; }

		[JsonIgnore]
		public bool MenuOpened { get; set; }

		public OrderBoard(BlackMarketBoardDefinition def)
			: base(def)
		{
			menuEnabled = true;
			MenuOpened = false;
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "TICKETS":
				reader.Read();
				tickets = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadOrderBoardTicket, tickets);
				break;
			case "PRIORITYPRESTIGEDEFINITIONIDS":
				reader.Read();
				PriorityPrestigeDefinitionIDs = ReaderUtil.PopulateListInt32(reader, PriorityPrestigeDefinitionIDs);
				break;
			case "HARVESTABLECHARACTERDEFINITIONID":
				reader.Read();
				HarvestableCharacterDefinitionId = Convert.ToInt32(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public override void Serialize(JsonWriter writer)
		{
			writer.WriteStartObject();
			SerializeProperties(writer);
			writer.WriteEndObject();
		}

		protected override void SerializeProperties(JsonWriter writer)
		{
			base.SerializeProperties(writer);
			if (tickets != null)
			{
				writer.WritePropertyName("tickets");
				writer.WriteStartArray();
				IEnumerator<OrderBoardTicket> enumerator = tickets.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						OrderBoardTicket current = enumerator.Current;
						writer.WriteStartObject();
						if (current.TransactionInst != null)
						{
							writer.WritePropertyName("TransactionInst");
							writer.WriteStartObject();
							writer.WritePropertyName("ID");
							writer.WriteValue(current.TransactionInst.ID);
							if (current.TransactionInst.Inputs != null)
							{
								writer.WritePropertyName("Inputs");
								writer.WriteStartArray();
								IEnumerator<QuantityItem> enumerator2 = current.TransactionInst.Inputs.GetEnumerator();
								try
								{
									while (enumerator2.MoveNext())
									{
										QuantityItem current2 = enumerator2.Current;
										writer.WriteStartObject();
										writer.WritePropertyName("ID");
										writer.WriteValue(current2.ID);
										writer.WritePropertyName("Quantity");
										writer.WriteValue(current2.Quantity);
										writer.WriteEndObject();
									}
								}
								finally
								{
									enumerator2.Dispose();
								}
								writer.WriteEndArray();
							}
							if (current.TransactionInst.Outputs != null)
							{
								writer.WritePropertyName("Outputs");
								writer.WriteStartArray();
								IEnumerator<QuantityItem> enumerator3 = current.TransactionInst.Outputs.GetEnumerator();
								try
								{
									while (enumerator3.MoveNext())
									{
										QuantityItem current3 = enumerator3.Current;
										writer.WriteStartObject();
										writer.WritePropertyName("ID");
										writer.WriteValue(current3.ID);
										writer.WritePropertyName("Quantity");
										writer.WriteValue(current3.Quantity);
										writer.WriteEndObject();
									}
								}
								finally
								{
									enumerator3.Dispose();
								}
								writer.WriteEndArray();
							}
							writer.WriteEndObject();
						}
						writer.WritePropertyName("StartGameTime");
						writer.WriteValue(current.StartGameTime);
						writer.WritePropertyName("BoardIndex");
						writer.WriteValue(current.BoardIndex);
						writer.WritePropertyName("OrderNameTableIndex");
						writer.WriteValue(current.OrderNameTableIndex);
						writer.WritePropertyName("StartTime");
						writer.WriteValue(current.StartTime);
						writer.WritePropertyName("CharacterDefinitionId");
						writer.WriteValue(current.CharacterDefinitionId);
						writer.WriteEndObject();
					}
				}
				finally
				{
					enumerator.Dispose();
				}
				writer.WriteEndArray();
			}
			if (PriorityPrestigeDefinitionIDs != null)
			{
				writer.WritePropertyName("PriorityPrestigeDefinitionIDs");
				writer.WriteStartArray();
				IEnumerator<int> enumerator4 = PriorityPrestigeDefinitionIDs.GetEnumerator();
				try
				{
					while (enumerator4.MoveNext())
					{
						int current4 = enumerator4.Current;
						writer.WriteValue(current4);
					}
				}
				finally
				{
					enumerator4.Dispose();
				}
				writer.WriteEndArray();
			}
			writer.WritePropertyName("HarvestableCharacterDefinitionId");
			writer.WriteValue(HarvestableCharacterDefinitionId);
		}

		public override BuildingObject AddBuildingObject(GameObject gameObject)
		{
			if (tickets == null || tickets.Count == 0)
			{
				tickets = new List<OrderBoardTicket>();
				PriorityPrestigeDefinitionIDs = new List<int>();
				PriorityPrestigeDefinitionIDs.Add(40003);
				PriorityPrestigeDefinitionIDs.Add(40003);
				HarvestableCharacterDefinitionId = 0;
			}
			return gameObject.AddComponent<OrderBoardBuildingObjectView>();
		}
	}
}
