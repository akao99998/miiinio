using System.Collections;
using Kampai.Game;
using Kampai.Util;
using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class DiscoGlobePanelView : strange.extensions.mediation.impl.View
	{
		private DiscoGlobeView discoGlobeView;

		internal void PreLoadDiscoGlobe()
		{
			if (!(discoGlobeView != null))
			{
				GameObject gameObject = Object.Instantiate(KampaiResources.Load<GameObject>("screen_DiscoBall"));
				gameObject.transform.SetParent(base.transform, false);
				gameObject.transform.SetAsFirstSibling();
				discoGlobeView = gameObject.GetComponent<DiscoGlobeView>();
			}
		}

		internal void DisplayDiscoGlobe(bool display, MinionParty party)
		{
			if (display)
			{
				DisplayDiscoGlobeView();
			}
			else
			{
				RemoveDiscoGlobeView(party);
			}
		}

		private void DisplayDiscoGlobeView()
		{
			if (discoGlobeView == null)
			{
				PreLoadDiscoGlobe();
			}
			StartCoroutine(WaitAFrameDisplayGlobe());
		}

		private IEnumerator WaitAFrameDisplayGlobe()
		{
			yield return null;
			DiscoGlobeMediator discoGlobeMediator = discoGlobeView.GetComponent<DiscoGlobeMediator>();
			discoGlobeMediator.DisplayDiscoGlobe();
		}

		private void RemoveDiscoGlobeView(MinionParty party)
		{
			if (!(discoGlobeView == null))
			{
				if (party.PartyPreSkip)
				{
					DestroyDiscoGlobeView();
				}
				else
				{
					discoGlobeView.RemoveDiscoBallAwesomeness(DestroyDiscoGlobeView);
				}
			}
		}

		internal void DestroyDiscoGlobeView()
		{
			if (!(discoGlobeView == null))
			{
				Object.Destroy(discoGlobeView.gameObject);
				discoGlobeView = null;
			}
		}
	}
}
