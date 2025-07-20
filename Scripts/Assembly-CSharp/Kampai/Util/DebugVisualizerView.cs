using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Kampai.Game;
using Kampai.UI;
using Kampai.UI.View;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.Util
{
	public class DebugVisualizerView : KampaiView
	{
		private const int HEIGHT = 25;

		private const int MAX_LEVEL = 10;

		public RectTransform content;

		public ButtonView CloseButton;

		private Dictionary<GameObject, ExpandableData> expandables = new Dictionary<GameObject, ExpandableData>();

		private Dictionary<GameObject, ValueData> values = new Dictionary<GameObject, ValueData>();

		private IPositionService positionService;

		private GameObject targetObject;

		private int count;

		private Font myFont;

		private float ALPHA = 0.6f;

		private bool arrayRebuilding;

		private float offset;

		private float WIDTH = 4.5f;

		[Inject]
		public ShowDebugVisualizerSignal showSignal { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		public void Init(IPositionService positionService, GameObject targetObject, float offset)
		{
			myFont = KampaiResources.Load<Font>("HelveticaLTStd-BoldCond");
			this.targetObject = targetObject;
			this.positionService = positionService;
			this.offset = offset;
			CloseButton.ClickedSignal.AddListener(OnCloseClicked);
		}

		public GameObject CreateProperty(DebugElement element, string name, object value, out Text rightText, int level = 0, int SiblingIndex = -1, bool exitIfNull = true)
		{
			rightText = null;
			if (value == null && exitIfNull)
			{
				return null;
			}
			GameObject go = new GameObject(string.Format("{0}_{1}Property{2}", level, element, count));
			Image image = go.AddComponent<Image>();
			float num = (10f - (float)level / 2f) / 10f;
			image.color = new Color(num, num, num, ALPHA);
			GameObject gameObject = new GameObject("Left");
			Image image2 = gameObject.AddComponent<Image>();
			float num2 = (10f - (float)(level - 1) / 2f) / 10f;
			image2.color = new Color(num2, num2, num2, ALPHA);
			Text text = AttachText(gameObject.transform, name);
			gameObject.transform.SetParent(go.transform, false);
			SetAnchor(gameObject.transform as RectTransform, 0f, 0.4f);
			GameObject gameObject2 = new GameObject("Right");
			Image image3 = gameObject2.AddComponent<Image>();
			image3.color = new Color(1f, 1f, 1f, 0f);
			rightText = AttachText(gameObject2.transform, (value != null) ? value.ToString() : "null");
			gameObject2.transform.SetParent(go.transform, false);
			SetAnchor(gameObject2.transform as RectTransform, 0.4f, 1f);
			go.transform.SetParent(content, false);
			if (SiblingIndex != -1)
			{
				go.transform.SetSiblingIndex(SiblingIndex + 1);
			}
			LayoutElement layoutElement = go.AddComponent<LayoutElement>();
			layoutElement.preferredHeight = 25f;
			switch (element)
			{
			case DebugElement.Expandable:
			{
				Button button3 = go.AddComponent<Button>();
				button3.onClick.AddListener(delegate
				{
					OnClick(go);
				});
				ExpandableData expandableData = new ExpandableData();
				expandableData.level = level;
				expandableData.value = value;
				expandableData.subItems = null;
				expandables.Add(go, expandableData);
				image.color = new Color(0f, 0f, 0f, ALPHA);
				text.color = Color.white;
				rightText.color = Color.white;
				break;
			}
			case DebugElement.Value:
			{
				if (value == null || value.GetType() != typeof(int) || (int)value < 100)
				{
					break;
				}
				int id = (int)value;
				Instance byInstanceId = playerService.GetByInstanceId<Instance>(id);
				if (byInstanceId != null)
				{
					rightText.color = Color.green;
					Button button = gameObject2.AddComponent<Button>();
					button.onClick.AddListener(delegate
					{
						OnNumberClick(id);
					});
					break;
				}
				Definition definition = null;
				definitionService.TryGet<Definition>(id, out definition);
				if (definition != null)
				{
					rightText.color = Color.green;
					Button button2 = gameObject2.AddComponent<Button>();
					button2.onClick.AddListener(delegate
					{
						OnNumberClick(id);
					});
				}
				break;
			}
			}
			go.SetLayerRecursively(5);
			count++;
			return go;
		}

		public GameObject CreateNoneValueProperty(DebugElement element, string name, int level = 0, int SiblingIndex = -1, object value = null)
		{
			GameObject go = new GameObject(string.Format("{0}_{1}Property{2}", level, element, count));
			Image image = go.AddComponent<Image>();
			float num = (10f - (float)level / 2f) / 10f;
			image.color = new Color(num, num, num, ALPHA);
			Text text = AttachText(go.transform, name);
			go.transform.SetParent(content, false);
			if (SiblingIndex != -1)
			{
				go.transform.SetSiblingIndex(SiblingIndex + 1);
			}
			LayoutElement layoutElement = go.AddComponent<LayoutElement>();
			layoutElement.preferredHeight = 25f;
			switch (element)
			{
			case DebugElement.Expandable:
			{
				Button button = go.AddComponent<Button>();
				button.onClick.AddListener(delegate
				{
					OnClick(go);
				});
				ExpandableData expandableData = new ExpandableData();
				expandableData.level = level;
				expandableData.value = value;
				expandableData.subItems = null;
				expandables.Add(go, expandableData);
				image.color = new Color(0f, 0f, 0f, ALPHA);
				text.color = Color.white;
				break;
			}
			}
			count++;
			return go;
		}

		internal void LateUpdate()
		{
			PositionData positionData = positionService.GetPositionData(targetObject.transform.position);
			if (base.transform.position != positionData.WorldPositionInUI)
			{
				base.transform.position = positionData.WorldPositionInUI + new Vector3(offset, 0f, 0f);
			}
			if (arrayRebuilding)
			{
				return;
			}
			foreach (KeyValuePair<GameObject, ValueData> value4 in values)
			{
				ValueData value = value4.Value;
				if (!value.isArray)
				{
					object value2 = value.info.GetValue(value.parentValue, null);
					if (value.text != null && value2 != null)
					{
						if (value2.GetType().IsValueType || value2.GetType() == typeof(string))
						{
							value.text.text = value2.ToString();
						}
						else
						{
							value.text.text = value2.GetType().Name;
						}
					}
					continue;
				}
				object value3 = value.info.GetValue(value.parentValue, null);
				ICollection collection = value3 as ICollection;
				if (collection == null)
				{
					continue;
				}
				if (collection.Count != value.currentArrayElements.Count)
				{
					StartCoroutine(RebuildArray(value));
					break;
				}
				int num = 0;
				foreach (object item in collection)
				{
					if (!item.Equals(value.currentArrayElements[num]))
					{
						StartCoroutine(RebuildArray(value));
						return;
					}
					num++;
				}
			}
		}

		private IEnumerator RebuildArray(ValueData data)
		{
			arrayRebuilding = true;
			OnClick(data.expandableGo);
			yield return null;
			OnClick(data.expandableGo);
			arrayRebuilding = false;
		}

		private void OnClick(GameObject gameObject)
		{
			if (expandables.ContainsKey(gameObject))
			{
				ExpandableData data = expandables[gameObject];
				if (data.subItems == null)
				{
					ProcessingPropertyInfo(gameObject.transform, ref data);
					return;
				}
				ClearSubItem(data.subItems);
				data.subItems = null;
			}
		}

		private void OnNumberClick(object number)
		{
			int type = (int)number;
			showSignal.Dispatch(targetObject, type, offset + WIDTH);
		}

		private void ClearSubItem(List<GameObject> subItems)
		{
			if (subItems == null)
			{
				return;
			}
			foreach (GameObject subItem in subItems)
			{
				if (values.ContainsKey(subItem))
				{
					values[subItem] = null;
					values.Remove(subItem);
				}
				if (expandables.ContainsKey(subItem))
				{
					ClearSubItem(expandables[subItem].subItems);
					expandables.Remove(subItem);
				}
				Object.Destroy(subItem);
			}
			subItems.Clear();
		}

		public void AddValueData(GameObject go, Text rightText, object currentObj, PropertyInfo info)
		{
			ValueData valueData = new ValueData();
			valueData.parentValue = currentObj;
			valueData.text = rightText;
			valueData.info = info;
			values.Add(go, valueData);
		}

		public void ProcessingPropertyInfo(Transform targetParent, ref ExpandableData data)
		{
			object value = data.value;
			data.subItems = new List<GameObject>();
			PropertyInfo[] properties = value.GetType().GetProperties();
			PropertyInfo[] array = properties;
			foreach (PropertyInfo propertyInfo in array)
			{
				if (propertyInfo.PropertyType.IsValueType || propertyInfo.PropertyType == typeof(string))
				{
					Text rightText;
					GameObject gameObject = CreateProperty(DebugElement.Value, propertyInfo.Name, propertyInfo.GetValue(value, null), out rightText, data.level + 1, targetParent.GetSiblingIndex() + data.subItems.Count);
					if (gameObject != null)
					{
						ValueData valueData = new ValueData();
						valueData.parentValue = value;
						valueData.text = rightText;
						valueData.info = propertyInfo;
						values.Add(gameObject, valueData);
						data.subItems.Add(gameObject);
					}
					continue;
				}
				object value2 = propertyInfo.GetValue(value, null);
				if (value2 == null)
				{
					continue;
				}
				ICollection collection = value2 as ICollection;
				if (collection != null)
				{
					GameObject gameObject2 = CreateNoneValueProperty(DebugElement.Title, propertyInfo.Name, data.level + 1, targetParent.GetSiblingIndex() + data.subItems.Count);
					data.subItems.Add(gameObject2);
					ValueData valueData2 = new ValueData();
					valueData2.parentValue = value;
					valueData2.info = propertyInfo;
					valueData2.isArray = true;
					valueData2.currentArrayElements = new List<object>();
					foreach (object item2 in collection)
					{
						if (item2.GetType().IsValueType || item2.GetType() == typeof(string))
						{
							Text rightText2;
							GameObject gameObject3 = CreateProperty(DebugElement.Value, item2.GetType().Name, item2, out rightText2, data.level + 1, targetParent.GetSiblingIndex() + data.subItems.Count);
							if (gameObject3 != null)
							{
								data.subItems.Add(gameObject3);
							}
						}
						else
						{
							GameObject gameObject4 = CreateNoneValueProperty(DebugElement.Expandable, item2.GetType().Name, data.level + 1, targetParent.GetSiblingIndex() + data.subItems.Count, item2);
							if (gameObject4 != null)
							{
								data.subItems.Add(gameObject4);
							}
						}
						valueData2.currentArrayElements.Add(item2);
					}
					valueData2.expandableGo = targetParent.gameObject;
					values.Add(gameObject2, valueData2);
				}
				else
				{
					GameObject item = CreateNoneValueProperty(DebugElement.Expandable, propertyInfo.Name, data.level + 1, targetParent.GetSiblingIndex() + data.subItems.Count, value2);
					data.subItems.Add(item);
				}
			}
		}

		private Text AttachText(Transform parent, string value)
		{
			GameObject gameObject = new GameObject("Text");
			Text text = gameObject.AddComponent<Text>();
			text.text = value;
			text.color = Color.black;
			text.font = myFont;
			text.resizeTextForBestFit = true;
			gameObject.transform.SetParent(parent, false);
			SetAnchor(gameObject.transform as RectTransform, 0f, 1f, true);
			return text;
		}

		private void SetAnchor(RectTransform rt, float xMin, float xMax, bool isText = false)
		{
			rt.localPosition = Vector3.zero;
			rt.localScale = Vector3.one;
			rt.offsetMin = Vector3.zero;
			rt.offsetMax = Vector3.zero;
			rt.pivot = Vector3.zero;
			rt.anchorMin = new Vector2(xMin, 0f);
			rt.anchorMax = new Vector2(xMax, 1f);
			rt.offsetMin = new Vector2(0f, -2f);
			rt.offsetMax = new Vector2(0f, -2f);
		}

		private void OnCloseClicked()
		{
			CloseButton.ClickedSignal.RemoveListener(OnCloseClicked);
			Object.Destroy(base.gameObject);
		}
	}
}
