using Kampai.Util;

namespace Kampai.Game.View
{
	public abstract class KampaiAction
	{
		protected readonly IKampaiLogger logger;

		protected bool instant;

		public bool Done { get; protected set; }

		public KampaiAction(IKampaiLogger logger)
		{
			this.logger = logger;
		}

		public virtual void Execute()
		{
		}

		public virtual void Abort()
		{
			Done = true;
		}

		public virtual void Update()
		{
		}

		public virtual void LateUpdate()
		{
		}

		public override string ToString()
		{
			return string.Format("{0}:{1}", base.ToString(), Done);
		}

		public KampaiAction SetInstant()
		{
			instant = true;
			return this;
		}

		public bool IsInstant()
		{
			return instant;
		}
	}
}
