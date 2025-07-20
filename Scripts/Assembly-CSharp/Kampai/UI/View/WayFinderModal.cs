using Kampai.Game;
using UnityEngine;

namespace Kampai.UI.View
{
	public class WayFinderModal : WorldToGlassUIModal
	{
		public Animator Animator;

		public ButtonView GoToButton;

		public WayFinderInnerModel GenericModel;

		public WayFinderInnerModel SpecificModel;

		public Prestige Prestige { get; set; }
	}
}
