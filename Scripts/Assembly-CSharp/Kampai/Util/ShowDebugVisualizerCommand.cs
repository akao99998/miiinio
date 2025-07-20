using Kampai.Game;
using Kampai.Game.View;
using Kampai.UI;
using Kampai.UI.View;
using UnityEngine;
using UnityEngine.UI;
using strange.extensions.command.impl;

namespace Kampai.Util
{
	public class ShowDebugVisualizerCommand : Command
	{
		private static string VISUALIZER = "popup_visualizer";

		[Inject]
		public GameObject hitObject { get; set; }

		[Inject]
		public int ID { get; set; }

		[Inject]
		public float offset { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public IPositionService positionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService defService { get; set; }

		public override void Execute()
		{
			ActionableObject actionableObject = hitObject.GetComponent<ActionableObject>();
			if (actionableObject == null)
			{
				actionableObject = hitObject.GetComponentInParent<ActionableObject>();
			}
			if (!(actionableObject != null))
			{
				return;
			}
			GameObject gameObject = guiService.Execute(GUIOperation.LoadUntrackedInstance, VISUALIZER);
			PositionData positionData = positionService.GetPositionData(hitObject.transform.position);
			gameObject.transform.position = positionData.WorldPositionInUI + new Vector3(offset, 0f, 0f);
			DebugVisualizerView component = gameObject.GetComponent<DebugVisualizerView>();
			component.Init(positionService, hitObject, offset);
			if (ID <= 0)
			{
				Text rightText;
				component.CreateProperty(DebugElement.Title, "Name", actionableObject.gameObject.name, out rightText);
				component.CreateProperty(DebugElement.Title, "Type", actionableObject.GetType().Name, out rightText);
				Text rightText2;
				GameObject go = component.CreateProperty(DebugElement.Value, "CurrentAction", actionableObject.currentAction, out rightText2, 0, -1, false);
				component.AddValueData(go, rightText2, actionableObject, actionableObject.GetType().GetProperty("currentAction"));
			}
			int id = ((ID > 0) ? ID : actionableObject.ID);
			Instance byInstanceId = playerService.GetByInstanceId<Instance>(id);
			if (byInstanceId != null)
			{
				component.CreateNoneValueProperty(DebugElement.Expandable, byInstanceId.GetType().Name, 0, -1, byInstanceId);
				return;
			}
			Definition definition = null;
			defService.TryGet<Definition>(id, out definition);
			if (definition != null)
			{
				component.CreateNoneValueProperty(DebugElement.Expandable, definition.GetType().Name, 0, -1, definition);
			}
		}
	}
}
