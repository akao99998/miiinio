using System.Collections.Generic;
using Kampai.Main;
using Newtonsoft.Json;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Util.AnimatorStateInfo
{
	public class LoadAnimatorStateInfoCommand : Command
	{
		private const string dataResourcePath = "Debug/animator_state_info";

		private const string viewResourcePath = "Debug/UI/AnimatorStateView";

		[Inject(MainElement.UI_GLASSCANVAS)]
		public GameObject glassCanvas { get; set; }

		public override void Execute()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("Debug/animator_state_info");
			Dictionary<int, string> o = JsonConvert.DeserializeObject<Dictionary<int, string>>(textAsset.text);
			base.injectionBinder.Bind<Dictionary<int, string>>().ToValue(o).ToName(UtilElement.ANIMATOR_STATE_DEBUG_INFO)
				.CrossContext()
				.Weak();
			GameObject gameObject = new GameObject("Animator State Views");
			gameObject.transform.parent = glassCanvas.transform;
			gameObject.transform.localScale = Vector3.one;
			GameObject original = Resources.Load<GameObject>("Debug/UI/AnimatorStateView");
			List<Transform> list = new List<Transform>();
			Animator[] array = Object.FindObjectsOfType<Animator>();
			foreach (Animator animator in array)
			{
				if (!list.Contains(animator.transform))
				{
					GameObject gameObject2 = Object.Instantiate(original);
					gameObject2.transform.parent = gameObject.transform;
					gameObject2.GetComponent<AnimatorStateView>().Initialize(animator);
					list.Add(animator.transform);
				}
			}
			gameObject.transform.localPosition = new Vector3((float)Screen.width / -2f, (float)Screen.height / -2f, 100f);
		}
	}
}
