using Elevation.Logging;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	[RequireComponent(typeof(Text))]
	public class LocalizeView : KampaiView
	{
		public enum TextCaseType
		{
			None = 0,
			Lower = 1,
			Upper = 2
		}

		[Tooltip("The Localized Key to be looked in the correct localized source.")]
		[SerializeField]
		private string Key = string.Empty;

		[SerializeField]
		[Tooltip("Tell the view what type of case to apply to the text view when it is displayed.")]
		private TextCaseType m_textCaseType;

		private bool m_isOverriden;

		public IKampaiLogger logger = LogManager.GetClassLogger("LocalizeView") as IKampaiLogger;

		private Text textView;

		public TextCaseType TextCase
		{
			get
			{
				return m_textCaseType;
			}
			set
			{
				m_textCaseType = value;
				Translate();
			}
		}

		public string LocKey
		{
			get
			{
				return Key;
			}
			set
			{
				Key = value;
				Translate();
				m_isOverriden = false;
			}
		}

		public string text
		{
			get
			{
				return textView.text;
			}
			set
			{
				if (textView == null)
				{
					textView = GetComponent<Text>();
				}
				textView.text = ((!(textView.name == "txt_ItemQuantity")) ? GetCaseString(value) : ("x" + GetCaseString(value)));
				m_isOverriden = true;
			}
		}

		public Color color
		{
			get
			{
				return textView.color;
			}
			set
			{
				if (textView == null)
				{
					textView = GetComponent<Text>();
				}
				textView.color = value;
			}
		}

		[Inject]
		public ILocalizationService service { get; set; }

		protected override void Awake()
		{
			base.Awake();
			textView = GetComponent<Text>();
		}

		private void OnEnable()
		{
			KampaiView.BubbleToContextOnStart(this, ref currentContext);
		}

		protected override void Start()
		{
			base.Start();
			if (textView == null)
			{
				logger.Error("LocalizeView: GameObject {0} is missing parent component Text!", base.name);
			}
			else if (!m_isOverriden && !string.IsNullOrEmpty(LocKey))
			{
				Translate();
			}
		}

		public void Format(bool useLocKey, params object[] args)
		{
			text = string.Format((!useLocKey) ? text : Key, args);
		}

		public void Format(string locKey, params object[] args)
		{
			Key = locKey;
			if (service == null)
			{
				logger.Error("service is null!");
			}
			else
			{
				text = service.GetString(locKey, args);
			}
		}

		private void Translate()
		{
			if (service != null)
			{
				text = service.GetString(Key);
			}
		}

		private string GetCaseString(string str)
		{
			switch (m_textCaseType)
			{
			case TextCaseType.Lower:
				str = service.StringToLower(str);
				break;
			case TextCaseType.Upper:
				str = service.StringToUpper(str);
				break;
			}
			return str;
		}
	}
}
