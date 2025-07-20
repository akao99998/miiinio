using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using Ea.Sharkbite.HttpPlugin.Http.Api;
using ICSharpCode.SharpZipLib.GZip;
using Kampai.Util;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Ea.Sharkbite.HttpPlugin.Http.Impl
{
	public class DefaultRequest : IRequest
	{
		public const long UNDEFINED_CONTENT_LENGTH = 0L;

		private const int BLOCK_SIZE = 8192;

		protected const long MIN_NOTIFY_DELTA = 102400L;

		private bool useGZip;

		protected DownloadProgress progress;

		protected Action<DownloadProgress, IRequest> notifyAction;

		protected bool abort;

		protected bool isRestarted;

		public string Uri { get; set; }

		public virtual string Method { get; set; }

		public byte[] Body { get; set; }

		public string Accept { get; set; }

		public string ContentType { get; set; }

		public string Username { get; set; }

		public string Password { private get; set; }

		public List<KeyValuePair<string, string>> QueryParams { get; set; }

		public Dictionary<string, string> Headers { get; set; }

		public List<KeyValuePair<string, string>> FormParams { get; set; }

		protected long Range { get; set; }

		public bool CanRetry { get; set; }

		public int RetryCount { get; set; }

		public bool TryResume { get; set; }

		public string FilePath { get; set; }

		public string Md5 { get; set; }

		public bool UseUdp { get; set; }

		public int requestCount { get; set; }

		public bool UseGZip
		{
			get
			{
				return useGZip;
			}
			set
			{
				useGZip = value;
				Headers["Accept-Encoding"] = ((!useGZip) ? "identity" : "gzip");
			}
		}

		public bool AvoidBackup { get; set; }

		public bool RunInBackground { get; set; }

		public Signal<IResponse> ResponseSignal { get; set; }

		public Signal<DownloadProgress, IRequest> ProgressSignal { get; set; }

		public DefaultRequest(string uri)
		{
			if (string.IsNullOrEmpty(uri))
			{
				throw new ArgumentNullException();
			}
			Uri uri2 = new Uri(uri);
			switch (uri2.Scheme.ToLower())
			{
			default:
				throw new ArgumentException("Only HTTP and HTTPS schemes supported");
			case "http":
			case "https":
				if (!string.IsNullOrEmpty(uri2.Query))
				{
					throw new ArgumentException("Query parameters should be set using the WithQueryParam method, rather than set directly in the Uri");
				}
				Uri = uri;
				Method = "GET";
				Body = null;
				Accept = string.Empty;
				ContentType = string.Empty;
				QueryParams = new List<KeyValuePair<string, string>>();
				Headers = new Dictionary<string, string>();
				FormParams = new List<KeyValuePair<string, string>>();
				Range = 0L;
				UseGZip = false;
				requestCount = 0;
				AvoidBackup = false;
				progress = new DownloadProgress(uri);
				break;
			}
		}

		public virtual void Get(Action<IResponse> callback)
		{
			Method = "GET";
			Execute(callback);
		}

		public virtual void Head(Action<IResponse> callback)
		{
			Method = "HEAD";
			Execute(callback);
		}

		public virtual void Options(Action<IResponse> callback)
		{
			Method = "OPTIONS";
			Execute(callback);
		}

		public virtual void Post(Action<IResponse> callback)
		{
			Method = "POST";
			Execute(callback);
		}

		public virtual void Put(Action<IResponse> callback)
		{
			Method = "PUT";
			Execute(callback);
		}

		public virtual void Delete(Action<IResponse> callback)
		{
			Method = "DELETE";
			Execute(callback);
		}

		public virtual void Execute(Action<IResponse> callback)
		{
			ThreadPool.QueueUserWorkItem(delegate
			{
				GetResponse(callback);
			});
		}

		public IRequest WithContentType(string contentType)
		{
			ContentType = contentType;
			return this;
		}

		public IRequest WithAccept(string accept)
		{
			Accept = accept;
			return this;
		}

		public IRequest WithQueryParam(string key, string value)
		{
			QueryParams.Add(new KeyValuePair<string, string>(key, value));
			return this;
		}

		public IRequest WithHeaderParam(string key, string value)
		{
			Headers[key] = value;
			return this;
		}

		public IRequest WithFormParam(string key, string value)
		{
			FormParams.Add(new KeyValuePair<string, string>(key, value));
			return this;
		}

		public IRequest WithBasicAuth(string username, string password)
		{
			Username = username;
			Password = password;
			return this;
		}

		public IRequest WithBody(byte[] body)
		{
			Body = body;
			return this;
		}

		public IRequest WithPreprocessor(IRequestPreprocessor preprocessor)
		{
			preprocessor.preprocess(this);
			return this;
		}

		public IRequest WithMethod(string method)
		{
			Method = method;
			return this;
		}

		public IRequest WithOutputFile(string filePath)
		{
			FilePath = filePath;
			return this;
		}

		public IRequest WithMd5(string md5)
		{
			Md5 = md5;
			return this;
		}

		public IRequest WithGZip(bool useGZip)
		{
			UseGZip = useGZip;
			return this;
		}

		public IRequest WithUdp(bool useUdp)
		{
			UseUdp = useUdp;
			return this;
		}

		public IRequest WithAvoidBackup(bool avoidBackup)
		{
			AvoidBackup = avoidBackup;
			return this;
		}

		public IRequest WithRunInBackground(bool runInBackground)
		{
			RunInBackground = runInBackground;
			return this;
		}

		public IRequest WithResponseSignal(Signal<IResponse> responseSignal)
		{
			ResponseSignal = responseSignal;
			return this;
		}

		public IRequest WithProgressSignal(Signal<DownloadProgress, IRequest> progressSignal)
		{
			ProgressSignal = progressSignal;
			return this;
		}

		public void RegisterNotifiable(Action<DownloadProgress, IRequest> notify)
		{
			notifyAction = notify;
		}

		public IRequest WithRetry(bool retry = true, int times = 3)
		{
			CanRetry = retry;
			RetryCount = times;
			return this;
		}

		public IRequest WithResume(bool tryResume)
		{
			TryResume = tryResume;
			return this;
		}

		public IRequest WithEntity(object entity)
		{
			Body = FastJSONSerializer.SerializeUTF8(entity);
			return this;
		}

		public IRequest WithRequestCount(int requestCount)
		{
			this.requestCount = requestCount;
			return this;
		}

		public virtual void Abort()
		{
			abort = true;
			if (isRestarted)
			{
				isRestarted = false;
			}
		}

		public virtual bool IsAborted()
		{
			return abort;
		}

		public virtual void Restart()
		{
			Abort();
			isRestarted = true;
		}

		public virtual bool IsRestarted()
		{
			return isRestarted;
		}

		protected virtual void TryRestart()
		{
			if (isRestarted)
			{
				abort = false;
				isRestarted = false;
			}
			if (!abort)
			{
				progress = new DownloadProgress(Uri);
			}
		}

		public virtual DownloadProgress GetProgress()
		{
			return (progress == null) ? null : progress.Clone();
		}

		public virtual string GetTempFilePath()
		{
			return string.Format("{0}.download", FilePath);
		}

		protected string GetUriWithQueryParams()
		{
			string value = ((!UseUdp) ? Uri : MediaClient.ConvertUrl(Uri));
			StringBuilder stringBuilder = new StringBuilder(value);
			if (QueryParams.Count > 0)
			{
				stringBuilder.Append("?");
				List<string> list = new List<string>();
				foreach (KeyValuePair<string, string> queryParam in QueryParams)
				{
					list.Add(string.Format("{0}={1}", WWW.EscapeURL(queryParam.Key), WWW.EscapeURL(queryParam.Value)));
				}
				stringBuilder.Append(string.Join("&", list.ToArray()));
			}
			return stringBuilder.ToString();
		}

		protected string GetBasicAuthHeader()
		{
			return string.Format("Basic {0}", Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:{1}", Username, Password))));
		}

		protected virtual HttpWebRequest CreateRequest()
		{
			HttpWebRequest httpWebRequest = null;
			try
			{
				httpWebRequest = WebRequest.Create(new Uri(GetUriWithQueryParams())) as HttpWebRequest;
				httpWebRequest.Timeout = HttpRequestConfig.HttpRequestTimeout;
				httpWebRequest.ReadWriteTimeout = HttpRequestConfig.HttpRequestReadWriteTimeout;
				if (ConnectionSettings.ConnectionLimit != 0)
				{
					httpWebRequest.ServicePoint.ConnectionLimit = ConnectionSettings.ConnectionLimit;
				}
				if (string.IsNullOrEmpty(Method))
				{
					throw new InvalidOperationException("A request Method (GET, POST, PUT, DELETE) must be provided.");
				}
				httpWebRequest.Method = Method;
				if (!string.IsNullOrEmpty(Accept))
				{
					httpWebRequest.Accept = Accept;
				}
				if (!string.IsNullOrEmpty(ContentType))
				{
					httpWebRequest.ContentType = ContentType;
				}
				foreach (KeyValuePair<string, string> header in Headers)
				{
					httpWebRequest.Headers.Add(header.Key, header.Value);
				}
				if (Range != 0L)
				{
					httpWebRequest.AddRange((int)Range);
				}
				if (!string.IsNullOrEmpty(Username))
				{
					httpWebRequest.Headers.Add(HttpRequestHeader.Authorization, GetBasicAuthHeader());
				}
				int num = 0;
				if (Body != null)
				{
					num++;
				}
				if (FormParams.Count > 0)
				{
					num++;
				}
				if (num > 1)
				{
					throw new InvalidOperationException("Request must contain only form params, or a body, or an entity, without combination.");
				}
				if (FormParams.Count > 0)
				{
					if (string.IsNullOrEmpty(ContentType))
					{
						ContentType = "application/x-www-form-urlencoded";
						httpWebRequest.MediaType = "application/x-www-form-urlencoded";
					}
					List<string> list = new List<string>();
					foreach (KeyValuePair<string, string> formParam in FormParams)
					{
						list.Add(string.Format("{0}={1}", WWW.EscapeURL(formParam.Key), WWW.EscapeURL(formParam.Value)));
					}
					Body = Encoding.UTF8.GetBytes(string.Join("&", list.ToArray()));
				}
				ServicePointManager.ServerCertificateValidationCallback = (RemoteCertificateValidationCallback)Delegate.Combine(ServicePointManager.ServerCertificateValidationCallback, new RemoteCertificateValidationCallback(CertificateValidationCallback));
				byte[] body = Body;
				if (body != null)
				{
					switch (Method)
					{
					case "GET":
					case "DELETE":
						throw new ProtocolViolationException();
					default:
					{
						httpWebRequest.ContentLength = body.Length;
						Stream requestStream = httpWebRequest.GetRequestStream();
						requestStream.Write(body, 0, body.Length);
						requestStream.Close();
						break;
					}
					}
				}
			}
			catch (WebException ex)
			{
				ServicePointManager.ServerCertificateValidationCallback = (RemoteCertificateValidationCallback)Delegate.Remove(ServicePointManager.ServerCertificateValidationCallback, new RemoteCertificateValidationCallback(CertificateValidationCallback));
				Native.LogError(ex.Message);
			}
			catch (Exception ex2)
			{
				Native.LogError(ex2.Message);
			}
			return httpWebRequest;
		}

		protected virtual HttpWebResponse ExecuteRequest()
		{
			HttpWebResponse result = null;
			HttpWebRequest httpWebRequest = CreateRequest();
			if (httpWebRequest == null)
			{
				Native.LogError(string.Format("Null request for Uri {0}", Uri));
				return null;
			}
			try
			{
				result = httpWebRequest.GetResponse() as HttpWebResponse;
			}
			catch (WebException ex)
			{
				Native.LogWarning(string.Format("WebException Downloading {0}: {1}", Uri, ex.Message));
				if (ex.Response != null)
				{
					result = ex.Response as HttpWebResponse;
				}
				else
				{
					ServicePointManager.ServerCertificateValidationCallback = (RemoteCertificateValidationCallback)Delegate.Remove(ServicePointManager.ServerCertificateValidationCallback, new RemoteCertificateValidationCallback(CertificateValidationCallback));
					Native.LogError(string.Format("WebException Response is NULL: {0}", ex.Message));
				}
			}
			catch (Exception ex2)
			{
				Native.LogError(string.Format("Exception Downloading {0}: {1}", Uri, ex2.Message));
			}
			return result;
		}

		protected virtual Dictionary<string, string> ProcessResponse(HttpWebResponse response, string body = "")
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			if (response != null && response.Headers != null)
			{
				string[] allKeys = response.Headers.AllKeys;
				foreach (string text in allKeys)
				{
					dictionary.Add(text, response.Headers.Get(text));
				}
			}
			ServicePointManager.ServerCertificateValidationCallback = (RemoteCertificateValidationCallback)Delegate.Remove(ServicePointManager.ServerCertificateValidationCallback, new RemoteCertificateValidationCallback(CertificateValidationCallback));
			return dictionary;
		}

		protected virtual string ReadResponse(HttpWebResponse response)
		{
			if (!IsFileDownload())
			{
				string result = string.Empty;
				try
				{
					if (response.GetResponseStream() != null)
					{
						Encoding encoding;
						try
						{
							encoding = Encoding.GetEncoding(response.CharacterSet);
						}
						catch (ArgumentException)
						{
							encoding = Encoding.UTF8;
						}
						byte[] array = new byte[8192];
						int num = 0;
						using (Stream stream = response.GetResponseStream())
						{
							for (int num2 = stream.Read(array, 0, array.Length); num2 > 0; num2 = stream.Read(array, num, array.Length - num))
							{
								num += num2;
								if (num == array.Length)
								{
									Array.Resize(ref array, array.Length + 8192);
								}
							}
						}
						if (num > 0)
						{
							result = encoding.GetString(array, 0, num);
						}
					}
				}
				catch (Exception ex2)
				{
					Native.LogError(ex2.Message);
				}
				return result;
			}
			using (Stream stream2 = response.GetResponseStream())
			{
				string text = response.Headers["Content-Encoding"];
				if (!string.IsNullOrEmpty(text) && text.ToLower().Contains("gzip"))
				{
					using (GZipInputStream input = new GZipInputStream(stream2))
					{
						return ReadFileResponse(input);
					}
				}
				return ReadFileResponse(stream2);
			}
		}

		protected virtual void GetResponse(Action<IResponse> callback)
		{
			DefaultResponse defaultResponse = null;
			TryRestart();
			bool flag = IsFileDownload();
			if (!abort)
			{
				progress.StartTimer();
				using (HttpWebResponse httpWebResponse = ExecuteRequest())
				{
					if (httpWebResponse != null)
					{
						if (!flag)
						{
							string body = ReadResponse(httpWebResponse);
							defaultResponse = new DefaultResponse().WithBody(body).WithCode((int)httpWebResponse.StatusCode).WithRequest(this)
								.WithContentLength(httpWebResponse.ContentLength)
								.WithContentType(httpWebResponse.ContentType)
								.WithHeaders(ProcessResponse(httpWebResponse, body));
						}
						else
						{
							PrepareDirectory();
							try
							{
								Dictionary<string, string> headers = ProcessResponse(httpWebResponse, string.Empty);
								progress.TotalBytes = GetContentLength(headers);
								string text = ((httpWebResponse.StatusCode == HttpStatusCode.RequestedRangeNotSatisfiable) ? string.Empty : ReadResponse(httpWebResponse));
								string tempFilePath = GetTempFilePath();
								string error = null;
								int code = (int)httpWebResponse.StatusCode;
								if (!abort && (string.IsNullOrEmpty(Md5) || Md5 == text))
								{
									File.Move(tempFilePath, FilePath);
								}
								else
								{
									File.Delete(tempFilePath);
									error = (abort ? "Aborting file download" : string.Format("Invalid MD5SUM {0} != {1}", Md5, text));
									code = 418;
								}
								defaultResponse = new FileDownloadResponse().WithError(error).WithCode(code).WithRequest(this)
									.WithContentLength(httpWebResponse.ContentLength)
									.WithContentType(httpWebResponse.ContentType)
									.WithHeaders(headers);
							}
							catch (Exception ex)
							{
								defaultResponse = new FileDownloadResponse().WithError(ex.Message).WithRequest(this).WithCode(500);
								Native.LogError(ex.Message);
							}
						}
					}
					else
					{
						Native.LogError("Null response for " + Uri);
						defaultResponse = (flag ? new FileDownloadResponse().WithError("The request timed out?") : new DefaultResponse()).WithCode(flag ? 408 : 500).WithRequest(this).WithConnectionLoss(true);
					}
				}
				progress.StopTimer();
				defaultResponse = defaultResponse.WithDownloadTime(progress.GetDownloadTime());
			}
			else
			{
				defaultResponse = (flag ? new FileDownloadResponse() : new DefaultResponse()).WithRequest(this).WithCode(500).WithError("Request was aborted before execution.");
			}
			if (callback != null)
			{
				callback(defaultResponse);
			}
		}

		private string ReadFileResponse(Stream input)
		{
			string tempFilePath = GetTempFilePath();
			using (MD5 mD = MD5.Create())
			{
				using (FileStream fileStream = File.Create(tempFilePath))
				{
					try
					{
						int num = 0;
						byte[] array = new byte[4096];
						while (!abort && (num = input.Read(array, 0, array.Length)) != 0)
						{
							fileStream.Write(array, 0, num);
							mD.TransformBlock(array, 0, num, array, 0);
							NotifyProgress(num, 102400L);
						}
						if (!abort)
						{
							mD.TransformFinalBlock(array, 0, 0);
						}
					}
					finally
					{
						NotifyProgress(0L, 0L);
					}
					return abort ? string.Empty : BitConverter.ToString(mD.Hash).Replace("-", string.Empty).ToLower();
				}
			}
		}

		protected virtual void NotifyProgress(long downloadedAmount, long downloadSizeDeltaMin = 0L)
		{
			progress.CompletedBytes += downloadedAmount;
			progress.DownloadedBytes += downloadedAmount;
			progress.Delta += downloadedAmount;
			if (notifyAction != null && progress.Delta > downloadSizeDeltaMin)
			{
				notifyAction(GetProgress(), this);
				progress.Delta = 0L;
			}
		}

		protected bool IsFileDownload()
		{
			return !string.IsNullOrEmpty(FilePath);
		}

		protected void PrepareDirectory()
		{
			if (IsFileDownload())
			{
				if (File.Exists(FilePath))
				{
					File.Delete(FilePath);
				}
				string tempFilePath = GetTempFilePath();
				if (File.Exists(tempFilePath))
				{
					File.Delete(tempFilePath);
				}
				string directoryName = Path.GetDirectoryName(FilePath);
				if (!Directory.Exists(directoryName))
				{
					Directory.CreateDirectory(directoryName);
				}
			}
		}

		protected long GetContentLength(IDictionary<string, string> headers)
		{
			string key = "Content-Length";
			long result;
			if (!headers.ContainsKey(key) || !long.TryParse(headers[key], out result))
			{
				result = -1L;
			}
			return (result <= 0) ? 0 : result;
		}

		protected string GetHeader(IDictionary<string, string> headers, string key)
		{
			string value;
			headers.TryGetValue(key, out value);
			return value ?? string.Empty;
		}

		protected static bool CertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}
	}
}
