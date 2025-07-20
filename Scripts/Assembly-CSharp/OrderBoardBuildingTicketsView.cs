using Kampai.Game;
using UnityEngine;

public class OrderBoardBuildingTicketsView : MonoBehaviour
{
	public GameObject[] Tickets;

	private float oneThird = 0.3333334f;

	private float twoThird = 2f / 3f;

	internal void DisableTickets()
	{
		GameObject[] tickets = Tickets;
		foreach (GameObject gameObject in tickets)
		{
			gameObject.SetActive(false);
		}
	}

	internal Vector3 GetTicketPosition(int index)
	{
		return Tickets[index].transform.position;
	}

	internal bool IsOrderboardSetupCorrectly()
	{
		if (Tickets == null || Tickets.Length == 0)
		{
			return false;
		}
		return true;
	}

	internal void SetTicketState(int ticketIndex, OrderBoardTicketState state)
	{
		if (state == OrderBoardTicketState.NOT_AVAILABLE)
		{
			Tickets[ticketIndex].SetActive(false);
		}
		else
		{
			Tickets[ticketIndex].SetActive(true);
		}
		Material material = Tickets[ticketIndex].GetComponent<Renderer>().material;
		switch (state)
		{
		case OrderBoardTicketState.PRESTIGE_CHECKED:
			material.mainTextureOffset = new Vector2(oneThird, twoThird);
			break;
		case OrderBoardTicketState.CHECKED:
			material.mainTextureOffset = new Vector2(oneThird, 0f);
			break;
		case OrderBoardTicketState.UNCHECKED:
			material.mainTextureOffset = new Vector2(0f, 0f);
			break;
		case OrderBoardTicketState.PRESTIGE_UNCHECKED:
			material.mainTextureOffset = new Vector2(0f, twoThird);
			break;
		case OrderBoardTicketState.VILLAIN_CHECKED:
			material.mainTextureOffset = new Vector2(oneThird, oneThird);
			break;
		case OrderBoardTicketState.VILLAIN_UNCHECKED:
			material.mainTextureOffset = new Vector2(0f, oneThird);
			break;
		case OrderBoardTicketState.TIMER:
			material.mainTextureOffset = new Vector2(twoThird, 0f);
			break;
		}
	}
}
