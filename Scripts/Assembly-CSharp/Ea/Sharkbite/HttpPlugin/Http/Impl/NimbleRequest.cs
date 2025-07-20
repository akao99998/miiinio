using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Ea.Sharkbite.HttpPlugin.Http.Api;
using Kampai.Util;

namespace Ea.Sharkbite.HttpPlugin.Http.Impl
{
	public class NimbleRequest : DefaultRequest
	{
		private NimbleBridge_HttpRequest.Method method;

		private NimbleBridge_NetworkConnectionHandle handle;

		public override string Method
		{
			get
			{
				return base.Method;
			}
			set
			{
				base.Method = value;
				switch (value)
				{
				case "HEAD":
					method = NimbleBridge_HttpRequest.Method.HTTP_HEAD;
					break;
				case "POST":
					method = NimbleBridge_HttpRequest.Method.HTTP_POST;
					break;
				case "PUT":
					method = NimbleBridge_HttpRequest.Method.HTTP_PUT;
					break;
				default:
					method = NimbleBridge_HttpRequest.Method.HTTP_GET;
					break;
				}
			}
		}

		public NimbleRequest(string uri)
			: base(uri)
		{
		}

		public override void Execute(Action<IResponse> callback)
		{
			GetResponse(callback);
		}

		public override void Abort()
		{
			bool flag = abort;
			base.Abort();
			if (flag || handle == null)
			{
				return;
			}
			using (NimbleBridge_HttpResponse nimbleBridge_HttpResponse = handle.GetResponse())
			{
				if (!nimbleBridge_HttpResponse.IsCompleted())
				{
					handle.Cancel();
				}
			}
		}

		public override DownloadProgress GetProgress()
		{
			return progress;
		}

		protected new virtual NimbleBridge_HttpRequest CreateRequest()
		{
			NimbleBridge_HttpRequest nimbleBridge_HttpRequest = NimbleBridge_HttpRequest.RequestWithUrl(GetUriWithQueryParams());
			nimbleBridge_HttpRequest.SetMethod(method);
			nimbleBridge_HttpRequest.SetTimeout((double)HttpRequestConfig.HttpRequestTimeout / 1000.0);
			nimbleBridge_HttpRequest.SetRunInBackground(RunInBackground);
			if (IsFileDownload())
			{
				string tempFilePath = GetTempFilePath();
				nimbleBridge_HttpRequest.SetTargetFilePath(tempFilePath);
				string cachedFilePath = GetCachedFilePath(tempFilePath);
				base.Range = ((!TryResume || !File.Exists(cachedFilePath)) ? 0 : new FileInfo(cachedFilePath).Length);
				if (base.Range != 0L)
				{
					if (!UseGZip && DownloadUtil.IsGZipped(cachedFilePath))
					{
						base.Range = 0L;
					}
					else
					{
						DownloadProgress downloadProgress = progress;
						long range = base.Range;
						progress.Delta = range;
						range = range;
						progress.CompletedBytes = range;
						downloadProgress.TotalBytes = range;
					}
				}
				nimbleBridge_HttpRequest.SetOverwritePolicy((base.Range != 0L) ? 1 : 0);
			}
			if (base.Range != 0L)
			{
				Headers["Range"] = string.Format("{0}={1}-", "bytes", base.Range);
			}
			else if (Headers.ContainsKey("Range"))
			{
				Headers.Remove("Range");
			}
			if (!string.IsNullOrEmpty(ContentType))
			{
				Headers["Content-Type"] = ContentType;
			}
			if (!string.IsNullOrEmpty(Username))
			{
				Headers["Authorization"] = GetBasicAuthHeader();
			}
			if (Headers.Count != 0)
			{
				nimbleBridge_HttpRequest.SetHeaders(Headers);
			}
			if (Body != null)
			{
				nimbleBridge_HttpRequest.SetData(Body);
			}
			return nimbleBridge_HttpRequest;
		}

		protected override void GetResponse(Action<IResponse> callback)
		{
			TryRestart();
			bool isFileDownload = IsFileDownload();
			if (abort)
			{
				if (callback != null)
				{
					callback((isFileDownload ? new FileDownloadResponse() : new DefaultResponse()).WithRequest(this).WithCode(500).WithError("Request was aborted before execution."));
				}
				return;
			}
			if (isFileDownload)
			{
				PrepareDirectory();
			}
			NimbleBridge_HttpRequest request = CreateRequest();
			progress.StartTimer();
			handle = NimbleBridge_Network.GetComponent().SendRequest(request, delegate(NimbleBridge_NetworkConnectionHandle connection)
			{
				progress.StopTimer();
				IResponse response2;
				if (!isFileDownload)
				{
					IResponse response = OnResponseCallback(connection);
					response2 = response;
				}
				else
				{
					response2 = OnDownloadCallback(connection);
				}
				IResponse obj = response2;
				request.Dispose();
				handle = null;
				if (isRestarted && !abort)
				{
					Execute(callback);
				}
				else if (callback != null)
				{
					callback(obj);
				}
			});
			using (NimbleBridge_HttpResponse nimbleBridge_HttpResponse = handle.GetResponse())
			{
				if (nimbleBridge_HttpResponse.IsCompleted())
				{
					return;
				}
				using (NimbleBridge_Error nimbleBridge_Error = nimbleBridge_HttpResponse.GetError())
				{
					if (nimbleBridge_Error == null || nimbleBridge_Error.IsNull())
					{
						handle.SetHeaderCallback(OnHeaderCallback);
						handle.SetProgressCallback(OnProgressCallback);
					}
				}
			}
		}

		private void OnHeaderCallback(NimbleBridge_NetworkConnectionHandle connection)
		{
			using (NimbleBridge_HttpRequest nimbleBridge_HttpRequest = connection.GetRequest())
			{
				using (NimbleBridge_HttpResponse nimbleBridge_HttpResponse = connection.GetResponse())
				{
					Dictionary<string, string> headers = nimbleBridge_HttpResponse.GetHeaders();
					long expectedContentLength = nimbleBridge_HttpResponse.GetExpectedContentLength();
					progress.TotalBytes = ((expectedContentLength <= 0) ? GetContentLength(headers) : expectedContentLength);
					if (progress.TotalBytes != 0L)
					{
						progress.TotalBytes += progress.CompletedBytes;
					}
					progress.IsGZipped = GetHeader(headers, "Content-Encoding").ToLower().Contains("gzip");
					if (abort || nimbleBridge_HttpResponse.IsCompleted() || nimbleBridge_HttpRequest.GetOverwritePolicy() != 1)
					{
						return;
					}
					string cachedFilePath = GetCachedFilePath(nimbleBridge_HttpRequest.GetTargetFilePath());
					bool flag = File.Exists(cachedFilePath);
					if (nimbleBridge_HttpResponse.GetStatusCode() != 206 || !flag || DownloadUtil.IsGZipped(cachedFilePath) != progress.IsGZipped || !GetHeader(headers, "Accept-Ranges").ToLower().Contains("bytes") || !GetHeader(headers, "Content-Range").StartsWith(string.Format("{0}={1}-".Replace('=', ' '), "bytes", base.Range), StringComparison.OrdinalIgnoreCase))
					{
						if (flag)
						{
							File.Delete(cachedFilePath);
						}
						Restart();
						abort = false;
					}
				}
			}
		}

		private void OnProgressCallback(NimbleBridge_NetworkConnectionHandle connection)
		{
			using (NimbleBridge_HttpResponse nimbleBridge_HttpResponse = connection.GetResponse())
			{
				NotifyProgress(nimbleBridge_HttpResponse.GetDownloadedContentLength() - progress.DownloadedBytes, 102400L);
			}
		}

		private IResponse OnResponseCallback(NimbleBridge_NetworkConnectionHandle connection)
		{
			using (NimbleBridge_HttpResponse nimbleBridge_HttpResponse = connection.GetResponse())
			{
				using (NimbleBridge_Error nimbleBridge_Error = nimbleBridge_HttpResponse.GetError())
				{
					int statusCode = nimbleBridge_HttpResponse.GetStatusCode();
					Dictionary<string, string> headers = nimbleBridge_HttpResponse.GetHeaders();
					bool flag = false;
					string text = null;
					if (nimbleBridge_Error == null || nimbleBridge_Error.IsNull())
					{
						flag = !NetworkUtil.IsConnected();
						text = null;
						if (statusCode < 200 || statusCode > 299)
						{
							text = string.Format("{0}", statusCode);
						}
					}
					else
					{
						switch (nimbleBridge_Error.GetCode())
						{
						case NimbleBridge_Error.Code.NETWORK_NO_CONNECTION:
						case NimbleBridge_Error.Code.NETWORK_UNREACHABLE:
						case NimbleBridge_Error.Code.NETWORK_TIMEOUT:
							flag = true;
							break;
						default:
							flag = !NetworkUtil.IsConnected();
							break;
						}
						text = string.Format("[{0}] {1} {2}", (int)nimbleBridge_Error.GetCode(), nimbleBridge_Error.GetCode(), nimbleBridge_Error.GetReason());
					}
					DefaultResponse defaultResponse = new DefaultResponse().WithRequest(this).WithCode(statusCode).WithError(text)
						.WithConnectionLoss(flag)
						.WithContentLength(nimbleBridge_HttpResponse.GetExpectedContentLength())
						.WithDownloadTime(progress.GetDownloadTime());
					if (headers != null)
					{
						defaultResponse.Headers = headers;
						defaultResponse.ContentType = GetHeader(headers, "Content-Type");
					}
					byte[] data = nimbleBridge_HttpResponse.GetData();
					if (data != null && data.Length > 0)
					{
						defaultResponse.Body = Encoding.UTF8.GetString(data);
					}
					return defaultResponse;
				}
			}
		}

		private IResponse OnDownloadCallback(NimbleBridge_NetworkConnectionHandle connection)
		{
			NotifyProgress(0L, 0L);
			using (NimbleBridge_HttpResponse nimbleBridge_HttpResponse = connection.GetResponse())
			{
				using (NimbleBridge_Error nimbleBridge_Error = nimbleBridge_HttpResponse.GetError())
				{
					bool isConnectionLost = false;
					string text = null;
					int num = nimbleBridge_HttpResponse.GetStatusCode();
					Dictionary<string, string> headers = nimbleBridge_HttpResponse.GetHeaders();
					string tempFilePath = GetTempFilePath();
					bool flag = nimbleBridge_Error != null && !nimbleBridge_Error.IsNull();
					if (File.Exists(tempFilePath) && !flag)
					{
						text = (abort ? "Aborting file download." : DownloadUtil.UnpackFile(tempFilePath, FilePath, Md5, AvoidBackup));
						if (!string.IsNullOrEmpty(text))
						{
							num = 418;
						}
					}
					else if (flag)
					{
						switch (nimbleBridge_Error.GetCode())
						{
						case NimbleBridge_Error.Code.NETWORK_NO_CONNECTION:
						case NimbleBridge_Error.Code.NETWORK_UNREACHABLE:
						case NimbleBridge_Error.Code.NETWORK_TIMEOUT:
							isConnectionLost = true;
							break;
						default:
							isConnectionLost = !NetworkUtil.IsConnected();
							break;
						}
						text = string.Format("[{0}] {1} {2}", (int)nimbleBridge_Error.GetCode(), nimbleBridge_Error.GetCode(), nimbleBridge_Error.GetReason());
					}
					else
					{
						text = (abort ? "Aborting file download." : "Temp file doesn't exist upon download finish.");
						num = 418;
					}
					if (File.Exists(tempFilePath))
					{
						File.Delete(tempFilePath);
					}
					return new FileDownloadResponse().WithError(text).WithCode((num == 0) ? 408 : num).WithRequest(this)
						.WithContentLength(nimbleBridge_HttpResponse.GetExpectedContentLength())
						.WithContentType(GetHeader(headers, "Content-Type"))
						.WithHeaders(headers)
						.WithConnectionLoss(isConnectionLost)
						.WithDownloadTime(progress.GetDownloadTime());
				}
			}
		}

		private string GetCachedFilePath(string targetFilePath)
		{
			return Path.Combine(NimbleBridge_ApplicationEnvironment.GetComponent().GetCachePath(), Path.GetFileName(targetFilePath));
		}
	}
}
