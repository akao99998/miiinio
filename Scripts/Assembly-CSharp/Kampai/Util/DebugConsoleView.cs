using System.Collections;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Game;
using Kampai.Main;
using Kampai.UI.View;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.Util
{
	public class DebugConsoleView : KampaiView
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("DebugConsoleView") as IKampaiLogger;

		public DebugConsoleInputField inputField;

		public Text textField;

		public Scrollbar scrollbar;

		public RectTransform QuestItemPanel;

		private int maxLines = 100;

		private bool initialized;

		private GameObject questDebugPanelPrefab;

		private GameObject questDebugPanelInst;

		private List<string> commandStack = new List<string>();

		private int stackIndex;

		private bool arrowDown;

		[Inject(MainElement.CAMERA)]
		public GameObject cameraGO { get; set; }

		[Inject]
		public LogToScreenSignal logToScreenSignal { get; set; }

		[Inject]
		public ShowQuestPanelSignal showQuestPanel { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public DebugConsoleController controller { get; set; }

		[Inject(MainElement.UI_GLASSCANVAS)]
		public GameObject glassCanvas { get; set; }

		public void OnGUI()
		{
			if (!inputField.isFocused)
			{
				return;
			}
			if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
			{
				arrowDown = true;
			}
			if (!arrowDown)
			{
				return;
			}
			if (Input.GetKeyUp(KeyCode.UpArrow))
			{
				if (stackIndex != 0)
				{
					stackIndex--;
					inputField.text = commandStack[stackIndex];
					inputField.MoveTextEnd(false);
					arrowDown = false;
				}
			}
			else if (Input.GetKeyUp(KeyCode.DownArrow) && stackIndex < commandStack.Count - 1)
			{
				stackIndex++;
				inputField.text = commandStack[stackIndex];
				arrowDown = false;
			}
		}

		protected override void Start()
		{
			if (PlayerPrefs.HasKey("DebugHistory"))
			{
				commandStack.Add(PlayerPrefs.GetString("DebugHistory"));
				stackIndex = 1;
			}
			inputField.onEndEdit.AddListener(InputRecieved);
			RectTransform rectTransform = base.transform as RectTransform;
			rectTransform.anchoredPosition = new Vector2(0f, 0f - rectTransform.sizeDelta.y);
			RectTransform component = inputField.GetComponent<RectTransform>();
			RectTransform rectTransform2 = component;
			Vector2 offsetMin = (component.offsetMax = Vector2.zero);
			rectTransform2.offsetMin = offsetMin;
			component = inputField.GetComponentInChildren<Text>().GetComponent<RectTransform>();
			RectTransform rectTransform3 = component;
			offsetMin = (component.offsetMax = Vector2.zero);
			rectTransform3.offsetMin = offsetMin;
			component = textField.GetComponent<RectTransform>();
			RectTransform rectTransform4 = component;
			offsetMin = (component.offsetMax = Vector2.zero);
			rectTransform4.offsetMin = offsetMin;
			base.Start();
			if (!initialized)
			{
				textField.text = string.Empty;
				AddNewMessage("Kampai.  Type 'help' for help.  Press UP for previous command.\n");
				initialized = true;
				controller.CloseConsoleSignal.AddListener(OnClose);
				controller.FlushSignal.AddListener(OnFlush);
				controller.EnableQuestDebugSignal.AddListener(OnQuestDebug);
			}
			StartCoroutine(WaitAFrame());
			questDebugPanelPrefab = KampaiResources.Load<GameObject>("QuestDebugPanel");
		}

		protected override void OnDestroy()
		{
			controller.CloseConsoleSignal.RemoveListener(OnClose);
			controller.FlushSignal.RemoveListener(OnFlush);
		}

		private void InitQuestButtons()
		{
			questDebugPanelInst = Object.Instantiate(questDebugPanelPrefab);
			Transform transform = questDebugPanelInst.transform;
			transform.SetParent(glassCanvas.transform, true);
			RectTransform rectTransform = transform as RectTransform;
			rectTransform.localPosition = Vector3.zero;
			rectTransform.localScale = Vector3.one;
		}

		private void OnQuestDebug()
		{
			DebugQuestView component = questDebugPanelInst.GetComponent<DebugQuestView>();
			component.Toggle();
		}

		private IEnumerator WaitAFrame()
		{
			yield return null;
			logToScreenSignal.AddListener(AddNewMessage);
			InitQuestButtons();
		}

		public void OnEnable()
		{
			StartCoroutine(FocusAfterframe());
		}

		private void AddNewMessage(string text)
		{
			textField.text = cropLines(textField.text + string.Format("\n{0}", text));
		}

		private void InputRecieved(string field)
		{
			if (true)
			{
				StartCoroutine(DelayedInputRecieved(field));
			}
		}

		private IEnumerator DelayedInputRecieved(string field)
		{
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			commandStack.Add(field);
			stackIndex = commandStack.Count;
			PlayerPrefs.SetString("DebugHistory", field);
			UpdateTextFields();
			ProcessInput(field);
			scrollbar.value = 0f;
		}

		private IEnumerator FocusAfterframe()
		{
			yield return null;
			inputField.ActivateInputField();
			inputField.Select();
		}

		private void UpdateTextFields()
		{
			textField.text = cropLines(textField.text + string.Format("\n{0}", inputField.text));
			inputField.text = string.Empty;
		}

		private int countNewLines(string text)
		{
			int num = 0;
			foreach (char c in text)
			{
				if (c == '\n')
				{
					num++;
				}
			}
			return num;
		}

		private string cropLines(string text)
		{
			int num = countNewLines(text) - maxLines;
			if (num > 0)
			{
				int num2 = 0;
				foreach (char c in text)
				{
					if (c == '\n')
					{
						num--;
						if (num < 0)
						{
							return text.Substring(num2);
						}
					}
					num2++;
				}
			}
			return text;
		}

		private void ProcessInput(string input)
		{
			string[] array = input.Split(' ');
			array[0] = array[0].ToLower();
			if (array[0].Length != 0)
			{
				DebugCommand command;
				switch (controller.GetCommand(array, out command))
				{
				case DebugCommandError.NotFound:
					AddNewMessage(input + " -> INVALID COMMAND");
					break;
				case DebugCommandError.NotEnoughArguments:
					AddNewMessage(input + " -> NOT ENOUGH ARGUMENTS");
					break;
				default:
					command(controller, array);
					AddNewMessage(controller.GetOutput());
					break;
				}
			}
		}

		public void OnClose()
		{
			base.transform.gameObject.SetActive(false);
		}

		private void OnFlush()
		{
			AddNewMessage(controller.GetOutput());
		}
	}
}
