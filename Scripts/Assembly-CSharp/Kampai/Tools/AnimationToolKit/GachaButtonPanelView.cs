using System;
using System.Collections.Generic;
using Kampai.Game;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.Tools.AnimationToolKit
{
	public class GachaButtonPanelView : KampaiView
	{
		private const float itemHeight = 64f;

		private List<GachaButtonView> views;

		private RectTransform scrollViewContent;

		private GameObject gachaButtonPrefab;

		internal void Init(ICollection<MinionAnimationDefinition> mads, ICollection<GachaAnimationDefinition> gads)
		{
			views = new List<GachaButtonView>();
			gachaButtonPrefab = Resources.Load("GachaButton") as GameObject;
			scrollViewContent = base.gameObject.FindChild("Scroll_Box").GetComponent<RectTransform>();
			List<AnimationDefinition> list = new List<AnimationDefinition>();
			foreach (GachaAnimationDefinition gad in gads)
			{
				list.Add(gad);
			}
			foreach (MinionAnimationDefinition mad in mads)
			{
				if (mad.arguments == null || !mad.arguments.ContainsKey("actor"))
				{
					list.Add(mad);
				}
			}
			for (int i = 0; i < list.Count; i++)
			{
				GachaButtonView item = CreateView(list[i], i);
				views.Add(item);
			}
			scrollViewContent.offsetMin = new Vector2(0f, 0f - (float)list.Count * 64f);
			scrollViewContent.offsetMax = Vector2.zero;
		}

		public void SetButtonCallback(Action<AnimationDefinition> callback)
		{
			foreach (GachaButtonView view in views)
			{
				view.FireGachaSignal.AddListener(callback);
			}
		}

		private GachaButtonView CreateView(AnimationDefinition def, int index)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(gachaButtonPrefab);
			GachaButtonView component = gameObject.GetComponent<GachaButtonView>();
			component.SetGachaDefinition(def);
			string text = string.Empty;
			MinionAnimationDefinition minionAnimationDefinition = def as MinionAnimationDefinition;
			if (minionAnimationDefinition != null)
			{
				text = FormatMinionAnimationDefinition(minionAnimationDefinition);
			}
			GachaAnimationDefinition gachaAnimationDefinition = def as GachaAnimationDefinition;
			if (gachaAnimationDefinition != null)
			{
				text = FormatGachaAnimationDefinition(gachaAnimationDefinition);
			}
			Transform transform = gameObject.transform;
			Text componentInChildren = gameObject.GetComponentInChildren<Text>();
			componentInChildren.text = text;
			transform.parent = scrollViewContent;
			UIUtils.ScaleFonts(gameObject);
			RectTransform rectTransform = transform as RectTransform;
			rectTransform.offsetMin = new Vector2(0f, -64f * (float)(index + 1));
			rectTransform.offsetMax = new Vector2(0f, -64f * (float)index);
			return component;
		}

		private string FormatMinionAnimationDefinition(MinionAnimationDefinition def)
		{
			string[] array = def.StateMachine.Split('/');
			string arg = array[array.Length - 1];
			string[] array2 = def.GetType().ToString().Split('.');
			arg = string.Format("{0}\ntype: {1}\n", arg, array2[array2.Length - 1]);
			if (def.arguments != null)
			{
				foreach (KeyValuePair<string, object> argument in def.arguments)
				{
					arg = string.Format("{0}\t{1}: {2}", arg, argument.Key, argument.Value);
				}
			}
			return arg;
		}

		private string FormatGachaAnimationDefinition(GachaAnimationDefinition def)
		{
			string arg = string.Format("GachaId: {0}\tAnimationId: {1}", def.ID, def.AnimationID);
			arg = ((def.Minions != 0) ? string.Format("{0}\ntype: Coordinated Gacha\tMinionCount: {1}", arg, def.Minions) : string.Format("{0}\ntype: Solo Gacha", arg));
			if (!string.IsNullOrEmpty(def.Prefab))
			{
				arg = string.Format("{0}\nprefab: {1}", arg, def.Prefab);
			}
			return arg;
		}
	}
}
