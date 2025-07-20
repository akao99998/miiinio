using System;
using UnityEngine;

namespace Kampai.UI.View
{
	public interface IGUIService
	{
		GameObject Execute(IGUICommand command);

		GameObject Execute(GUIOperation operation, string prefab);

		GameObject Execute(GUIOperation operation, GUIPriority priority, string prefab);

		IGUICommand BuildCommand(GUIOperation operation, string prefab);

		IGUICommand BuildCommand(GUIOperation operation, string prefab, string guiLabel);

		IGUICommand BuildCommand(GUIOperation operation, GUIPriority priority, string prefab);

		void AddToArguments(object arg);

		void RemoveFromArguments(Type arg);
	}
}
