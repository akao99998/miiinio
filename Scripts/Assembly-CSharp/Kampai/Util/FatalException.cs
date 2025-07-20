using System;

namespace Kampai.Util
{
	public class FatalException : Exception
	{
		public FatalCode FatalCode;

		public int ReferencedId;

		public FatalException(FatalCode fatalCode, int referenceId, Exception inner, string format, params object[] args)
			: base((format == null) ? string.Empty : string.Format(format, args), inner)
		{
			FatalCode = fatalCode;
			ReferencedId = referenceId;
		}

		public FatalException(FatalCode fatalCode, string format, params object[] args)
			: this(fatalCode, 0, null, format, args)
		{
		}

		public FatalException(FatalCode fatalCode, Exception inner, string format, params object[] args)
			: this(fatalCode, 0, inner, format, args)
		{
		}

		public FatalException(FatalCode fatalCode, int referenceId, string format, params object[] args)
			: this(fatalCode, referenceId, null, format, args)
		{
		}

		public FatalException(FatalCode fatalCode, int referenceId)
			: this(fatalCode, referenceId, null, string.Empty)
		{
		}

		public override string ToString()
		{
			return string.Format("FatalCode = {0}, ReferencedId = {1}, {2}", FatalCode, ReferencedId, base.ToString());
		}
	}
}
