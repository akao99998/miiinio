using System;
using System.IO;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI
{
	internal sealed class KampaiFPSCounter : MonoBehaviour
	{
		private int m_sampleInterval;

		private float[] filteringKernel;

		private int currentSample;

		private FastTextStreamWriter fpsStream;

		private int bufferSize = 30;

		private float min;

		private float max;

		public Text TextComponent { get; set; }

		internal int SampleInterval
		{
			get
			{
				return m_sampleInterval * bufferSize;
			}
			set
			{
				m_sampleInterval = ((value == 0) ? 1 : value);
			}
		}

		private float getFilteredFrameTime()
		{
			int num = Math.Min(bufferSize, currentSample);
			float num2 = 0f;
			for (int i = 0; i < num; i++)
			{
				num2 += filteringKernel[i];
			}
			return num2 / (float)num;
		}

		private void Awake()
		{
			fpsStream = new FastTextStreamWriter(new FileStream(Path.Combine(GameConstants.PERSISTENT_DATA_PATH, "fps.log.txt"), FileMode.Create, FileAccess.Write));
			bufferSize = ((Application.targetFrameRate > 0) ? Application.targetFrameRate : 30);
		}

		private void Update()
		{
			if ((filteringKernel == null && bufferSize > 0) || (filteringKernel != null && bufferSize != filteringKernel.Length))
			{
				filteringKernel = new float[bufferSize];
			}
			float num = 1f / Time.smoothDeltaTime;
			int num2 = currentSample++ % bufferSize;
			int num3 = Mathf.FloorToInt(getFilteredFrameTime());
			if (num > max)
			{
				max = num;
			}
			if (num < min)
			{
				min = num;
			}
			filteringKernel[num2] = num;
			TextComponent.text = string.Format("{0} {1}/{2}", num3, Mathf.FloorToInt(min), Mathf.FloorToInt(max));
			if (currentSample == SampleInterval)
			{
				currentSample = 0;
				min = (max = num);
			}
			fpsStream.WriteLine(num);
		}

		public void OnDestroy()
		{
			fpsStream.Flush();
			fpsStream.Dispose();
		}
	}
}
