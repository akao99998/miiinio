using Kampai.Game.Mignette.AlligatorSkiing.View;
using UnityEngine;

public class AlligatorSkiingRotateTrigger : MonoBehaviour
{
	public enum TriggerType
	{
		Alligator = 0,
		Minion = 1
	}

	public Vector3 Rotation = Vector3.zero;

	public TriggerType TriggerCause = TriggerType.Minion;

	private void OnTriggerEnter(Collider other)
	{
		if (TriggerCause == TriggerType.Minion)
		{
			AlligatorSkiingMinionHardpointViewObject component = other.GetComponent<AlligatorSkiingMinionHardpointViewObject>();
			if (component != null)
			{
				component.Agent.OnRotateMinion(Quaternion.Euler(Rotation));
			}
		}
		else
		{
			AlligatorAgent component2 = other.GetComponent<AlligatorAgent>();
			if (component2 != null)
			{
				component2.OnRotateMinion(Quaternion.Euler(Rotation));
			}
		}
	}
}
