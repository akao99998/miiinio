using Kampai.Game.Mignette.AlligatorSkiing.View;
using UnityEngine;

public class AlligatorSkiingSpeedTrigger : MonoBehaviour
{
	public enum TriggerType
	{
		Alligator = 0,
		Minion = 1
	}

	public float Speed;

	public float acceleration;

	public TriggerType TriggerCause;

	private void OnTriggerEnter(Collider other)
	{
		if (TriggerCause == TriggerType.Minion)
		{
			AlligatorSkiingMinionHardpointViewObject component = other.GetComponent<AlligatorSkiingMinionHardpointViewObject>();
			if (component != null)
			{
				component.Agent.ChangeSpeed(Speed, acceleration);
			}
		}
		else
		{
			AlligatorAgent component2 = other.GetComponent<AlligatorAgent>();
			if (component2 != null)
			{
				component2.ChangeSpeed(Speed, acceleration);
			}
		}
	}
}
