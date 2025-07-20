using System;
using System.Collections.Generic;
using strange.extensions.signal.impl;

namespace Ea.Sharkbite.HttpPlugin.Http.Api
{
	public interface IRequest
	{
		string Uri { get; set; }

		string Method { get; set; }

		byte[] Body { get; set; }

		string Accept { get; set; }

		string ContentType { get; set; }

		string Username { get; set; }

		string Password { set; }

		int requestCount { get; set; }

		List<KeyValuePair<string, string>> QueryParams { get; set; }

		Dictionary<string, string> Headers { get; set; }

		List<KeyValuePair<string, string>> FormParams { get; set; }

		bool CanRetry { get; set; }

		int RetryCount { get; set; }

		bool TryResume { get; set; }

		string FilePath { get; set; }

		string Md5 { get; set; }

		bool UseGZip { get; set; }

		bool UseUdp { get; set; }

		bool AvoidBackup { get; set; }

		bool RunInBackground { get; set; }

		Signal<IResponse> ResponseSignal { get; set; }

		Signal<DownloadProgress, IRequest> ProgressSignal { get; set; }

		string GetTempFilePath();

		DownloadProgress GetProgress();

		void Get(Action<IResponse> callback);

		void Head(Action<IResponse> callback);

		void Options(Action<IResponse> callback);

		void Post(Action<IResponse> callback);

		void Put(Action<IResponse> callback);

		void Delete(Action<IResponse> callback);

		void Execute(Action<IResponse> callback);

		IRequest WithContentType(string contentType);

		IRequest WithAccept(string accept);

		IRequest WithQueryParam(string key, string value);

		IRequest WithHeaderParam(string key, string value);

		IRequest WithFormParam(string key, string value);

		IRequest WithBasicAuth(string username, string password);

		IRequest WithBody(byte[] body);

		IRequest WithPreprocessor(IRequestPreprocessor preprocessor);

		IRequest WithMethod(string method);

		IRequest WithOutputFile(string filePath);

		IRequest WithMd5(string md5);

		IRequest WithGZip(bool useGZip);

		IRequest WithUdp(bool useUdp);

		IRequest WithAvoidBackup(bool avoidBackup);

		IRequest WithRunInBackground(bool runInBackground);

		IRequest WithResponseSignal(Signal<IResponse> responseSignal);

		IRequest WithProgressSignal(Signal<DownloadProgress, IRequest> progressSignal);

		void RegisterNotifiable(Action<DownloadProgress, IRequest> notify);

		IRequest WithRetry(bool retry = true, int times = 3);

		IRequest WithResume(bool tryResume);

		IRequest WithEntity(object entity);

		IRequest WithRequestCount(int saveCount);

		void Abort();

		bool IsAborted();

		void Restart();

		bool IsRestarted();
	}
}
