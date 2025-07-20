using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kampai.Game
{
	public static class InputUtils
	{
		private sealed class TouchComparer : IComparer<Touch>
		{
			int IComparer<Touch>.Compare(Touch x, Touch y)
			{
				return x.fingerId - y.fingerId;
			}
		}

		private const int MAX_HANDLED_TOUCHES = 256;

		private static TouchComparer comparer = new TouchComparer();

		private static bool[] touchCorruptedBySamsung = new bool[256];

		private static Touch[] touches = new Touch[256];

		private static int _touchCount;

		private static int frameUpdated;

		public static int touchCount
		{
			get
			{
				if (frameUpdated < Time.frameCount)
				{
					UpdateTouchStates();
				}
				return _touchCount;
			}
		}

		private static void UpdateTouchStates()
		{
			frameUpdated = Time.frameCount;
			for (int i = 0; i < Input.touchCount; i++)
			{
				Touch touch = Input.GetTouch(i);
				TouchPhase phase = touch.phase;
				int fingerId = touch.fingerId;
				if (phase != TouchPhase.Moved && phase != TouchPhase.Stationary)
				{
					CorruptTouch(fingerId, false);
				}
			}
			int num = 0;
			if (Input.touchCount > 0)
			{
				for (int j = 0; j < Input.touchCount; j++)
				{
					Touch touch2 = Input.GetTouch(j);
					if (!IsTouchCorrupted(touch2.fingerId))
					{
						touches[num] = touch2;
						num++;
					}
				}
			}
			_touchCount = num;
			if (_touchCount > 0)
			{
				Array.Sort(touches, 0, _touchCount, comparer);
			}
		}

		private static bool IsTouchCorrupted(int fingerId)
		{
			return fingerId >= 256 || touchCorruptedBySamsung[fingerId];
		}

		private static void CorruptTouch(int fingerId, bool isCorrupted)
		{
			if (fingerId < 256)
			{
				touchCorruptedBySamsung[fingerId] = isCorrupted;
			}
		}

		public static Touch GetTouch(int i)
		{
			if (frameUpdated < Time.frameCount)
			{
				UpdateTouchStates();
			}
			return touches[i];
		}
	}
}
