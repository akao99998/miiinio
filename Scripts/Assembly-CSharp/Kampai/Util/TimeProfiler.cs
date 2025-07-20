using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Kampai.Util
{
	public class TimeProfiler : IDisposable
	{
		private sealed class FrameCounter : MonoBehaviour
		{
			public int Frame;

			public Action<int, float> LongFrameOccured;

			private Coroutine updateCoroutine;

			private bool isRunning = true;

			public void Start()
			{
				updateCoroutine = StartCoroutine(UpdateFrameStats());
			}

			public void StopUpdateCoroutine()
			{
				isRunning = false;
				StopCoroutine(updateCoroutine);
			}

			public void Update()
			{
				float unscaledDeltaTime = Time.unscaledDeltaTime;
				if ((double)unscaledDeltaTime > 0.1)
				{
					LongFrameOccured(Time.frameCount - 1, unscaledDeltaTime);
				}
			}

			public IEnumerator UpdateFrameStats()
			{
				while (isRunning)
				{
					yield return new WaitForEndOfFrame();
					Frame = Time.frameCount + 1;
				}
			}
		}

		private sealed class Section
		{
			private string name;

			private Stopwatch timer;

			private TimeSpan assetsLoadTime;

			private TimeSpan subsectionLoadTime;

			private int numberOfAssetsLoaded;

			private bool wasReleased;

			public Section(string name)
			{
				this.name = name;
				timer = Stopwatch.StartNew();
			}

			public string FormatSectionTime()
			{
				return GetSectionTime().ToString();
			}

			public TimeSpan GetSectionTime()
			{
				return timer.Elapsed;
			}

			public string GetSectionName()
			{
				return name;
			}

			public void mergeSubSection(TimeSpan time)
			{
				assetsLoadTime += time;
				numberOfAssetsLoaded++;
			}

			public void mergeSubSection(Section section)
			{
				assetsLoadTime += section.assetsLoadTime;
				numberOfAssetsLoaded += section.numberOfAssetsLoaded;
				subsectionLoadTime += section.timer.Elapsed;
			}

			public string FormatAssetsLoadTime()
			{
				return GetAssetsLoadTime().ToString();
			}

			public TimeSpan GetAssetsLoadTime()
			{
				return assetsLoadTime;
			}

			public int GetNumberOfAssetsLoaded()
			{
				return numberOfAssetsLoaded;
			}

			public string FormatTotalSubSectionsTime()
			{
				return GetTotalSubSectionsTime().ToString();
			}

			public TimeSpan GetTotalSubSectionsTime()
			{
				return subsectionLoadTime;
			}

			public void ReleaseSection()
			{
				wasReleased = true;
			}

			public bool IsSectionReleased()
			{
				return wasReleased;
			}
		}

		private static TimeProfiler currentInstance;

		private StreamWriter stream;

		private StringBuilder stringBuilder;

		private IKampaiLogger logger;

		private TimeSpan totalAssetsLoadTime;

		private GameObject frameCounterGO;

		private FrameCounter frameCounter;

		private static readonly string PROFILER_LOG_PATH;

		private static readonly Type MonoProfiler_Profiler;

		private static readonly MethodInfo MonoProfiler_Profiler_Start;

		private static readonly MethodInfo MonoProfiler_Profiler_Stop;

		private List<Section> sections = new List<Section>();

		protected bool Disposed { get; private set; }

		private TimeProfiler()
		{
			string path = "timelog_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm") + ".txt";
			string path2 = Path.Combine(GameConstants.PERSISTENT_DATA_PATH, path);
			stream = new StreamWriter(path2);
			stream.AutoFlush = true;
			stringBuilder = new StringBuilder();
			frameCounterGO = new GameObject("FrameCounter");
			frameCounter = frameCounterGO.AddComponent<FrameCounter>();
			FrameCounter obj = frameCounter;
			obj.LongFrameOccured = (Action<int, float>)Delegate.Combine(obj.LongFrameOccured, new Action<int, float>(LogLongFrame));
		}

		static TimeProfiler()
		{
			PROFILER_LOG_PATH = GameConstants.PERSISTENT_DATA_PATH + "/profiler/";
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				if (assembly.GetName().Name.Equals("MonoProfiler"))
				{
					MonoProfiler_Profiler = assembly.GetType("MonoProfiler.Profiler");
				}
			}
			if (MonoProfiler_Profiler != null)
			{
				MonoProfiler_Profiler_Start = MonoProfiler_Profiler.GetMethod("Start", new Type[2]
				{
					typeof(string),
					typeof(bool)
				});
				MonoProfiler_Profiler_Stop = MonoProfiler_Profiler.GetMethod("Stop", Type.EmptyTypes);
			}
		}

		private Section GetLastSection(bool pop)
		{
			int count = sections.Count;
			if (count == 0)
			{
				return null;
			}
			Section section = sections[count - 1];
			if (pop)
			{
				sections.Remove(section);
			}
			return section;
		}

		private void Write(string message)
		{
			if (currentInstance != null)
			{
				currentInstance.stream.Write(message);
				currentInstance.stringBuilder.Append(message);
			}
		}

		private void Log()
		{
			if (currentInstance != null && currentInstance.logger != null)
			{
				currentInstance.logger.Log(KampaiLogLevel.Info, currentInstance.stringBuilder.ToString());
				currentInstance.stringBuilder.Length = 0;
			}
		}

		private void LogLongFrame(int frameNumber, float time)
		{
			string message = string.Format("!!!! Frame {0} took {1} s.\n", frameNumber, time);
			Write(message);
		}

		private static string GetTimeStamp()
		{
			return DateTime.Now.ToString("HH:mm:ss.ffff");
		}

		public static void Reset(bool enabled)
		{
			if (currentInstance != null)
			{
				Flush();
				if (currentInstance.frameCounter != null)
				{
					currentInstance.frameCounter.StopUpdateCoroutine();
					UnityEngine.Object.Destroy(currentInstance.frameCounterGO);
				}
				currentInstance.Dispose();
				currentInstance = null;
			}
			if (enabled)
			{
				currentInstance = new TimeProfiler();
			}
		}

		public static void Flush()
		{
			if (currentInstance != null)
			{
				currentInstance.stream.Flush();
				currentInstance.Log();
			}
		}

		public static void StartSection(string sectionName)
		{
			if (currentInstance != null)
			{
				Section section = new Section(sectionName);
				currentInstance.sections.Add(section);
				string message = string.Format("{0}>{1}{2} (frame {3})\n", GetTimeStamp(), new string('\t', currentInstance.sections.Count - 1), section.GetSectionName(), currentInstance.frameCounter.Frame);
				currentInstance.Write(message);
			}
		}

		public static void EndSection(string sectionName)
		{
			if (currentInstance == null)
			{
				return;
			}
			Section lastSection = currentInstance.GetLastSection(false);
			if (lastSection != null && lastSection.GetSectionName() == sectionName)
			{
				lastSection.ReleaseSection();
				while (lastSection != null && lastSection.IsSectionReleased())
				{
					PopLatestSection();
					lastSection = currentInstance.GetLastSection(false);
				}
			}
			else
			{
				foreach (Section section in currentInstance.sections)
				{
					if (section.GetSectionName() == sectionName)
					{
						section.ReleaseSection();
						break;
					}
				}
			}
			currentInstance.Log();
		}

		private static void PopLatestSection()
		{
			Section lastSection = currentInstance.GetLastSection(true);
			Section lastSection2 = currentInstance.GetLastSection(false);
			if (lastSection2 != null)
			{
				lastSection2.mergeSubSection(lastSection);
			}
			string message = string.Format("{0}<{1}{2} in {3}. ({4} assets loaded in {5}). {6} in section. (frame {7})\n", GetTimeStamp(), new string('\t', currentInstance.sections.Count), lastSection.GetSectionName(), lastSection.FormatSectionTime(), lastSection.GetNumberOfAssetsLoaded(), lastSection.FormatAssetsLoadTime(), (lastSection2 == null) ? string.Empty : lastSection2.FormatTotalSubSectionsTime(), currentInstance.frameCounter.Frame);
			currentInstance.Write(message);
		}

		public static void StartAssetLoadSection(string name)
		{
			if (currentInstance != null)
			{
				Section item = new Section(new string('\t', currentInstance.sections.Count) + "asset " + name);
				currentInstance.sections.Add(item);
			}
		}

		public static void EndAssetLoadSection()
		{
			if (currentInstance != null)
			{
				Section lastSection = currentInstance.GetLastSection(true);
				TimeSpan sectionTime = lastSection.GetSectionTime();
				currentInstance.totalAssetsLoadTime += sectionTime;
				if (currentInstance.sections.Count > 0)
				{
					currentInstance.GetLastSection(false).mergeSubSection(sectionTime);
				}
				string message = string.Format("{0}<{1} in {2}. total {3} (frame {4})\n", GetTimeStamp(), lastSection.GetSectionName(), sectionTime.ToString(), currentInstance.totalAssetsLoadTime.ToString(), currentInstance.frameCounter.Frame);
				currentInstance.Write(message);
			}
		}

		public static void StartMonoProfiler(string sectionName)
		{
			if (MonoProfiler_Profiler == null)
			{
				if (currentInstance != null && currentInstance.logger != null)
				{
					currentInstance.logger.Debug("Failed to start the profiler, MonoProfiler.Profiler class is not found");
				}
				return;
			}
			if (MonoProfiler_Profiler_Start == null)
			{
				if (currentInstance != null && currentInstance.logger != null)
				{
					currentInstance.logger.Debug("Failed to start the profiler, MonoProfiler.Profiler.Start method is not found");
				}
				return;
			}
			if (!Directory.Exists(PROFILER_LOG_PATH))
			{
				Directory.CreateDirectory(PROFILER_LOG_PATH);
			}
			string text = Path.Combine(PROFILER_LOG_PATH, sectionName);
			MonoProfiler_Profiler_Start.Invoke(null, new object[2] { text, false });
		}

		public static void StopMonoProfiler()
		{
			if (MonoProfiler_Profiler != null && MonoProfiler_Profiler_Stop != null)
			{
				MonoProfiler_Profiler_Stop.Invoke(null, new object[0]);
			}
		}

		public static void InitializeLogger(IKampaiLogger logger)
		{
			if (currentInstance != null)
			{
				currentInstance.logger = logger;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!Disposed && disposing)
			{
				stream.Dispose();
				stream = null;
			}
			Disposed = true;
		}

		~TimeProfiler()
		{
			Dispose(false);
		}
	}
}
