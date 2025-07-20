using System.Collections.Generic;

namespace Facebook.Unity
{
	internal class PayResult : ResultBase, IPayResult, IResult
	{
		internal const long CancelPaymentFlowCode = 1383010L;

		public long ErrorCode
		{
			get
			{
				return base.CanvasErrorCode.GetValueOrDefault();
			}
		}

		internal PayResult(ResultContainer resultContainer)
			: base(resultContainer)
		{
			if (base.CanvasErrorCode.HasValue && base.CanvasErrorCode.Value == 1383010)
			{
				Cancelled = true;
			}
		}

		public override string ToString()
		{
			return Utilities.FormatToString(base.ToString(), GetType().Name, new Dictionary<string, string> { 
			{
				"ErrorCode",
				ErrorCode.ToString()
			} });
		}
	}
}
