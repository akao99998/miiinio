using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.Util
{
	public class ValueData
	{
		public object parentValue;

		public PropertyInfo info;

		public Text text;

		public bool isArray;

		public List<object> currentArrayElements;

		public GameObject expandableGo;
	}
}
