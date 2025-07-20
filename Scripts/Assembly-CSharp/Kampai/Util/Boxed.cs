namespace Kampai.Util
{
	public class Boxed<T> : IBoxed
	{
		protected T value;

		public T Value
		{
			get
			{
				return value;
			}
		}

		public Boxed(T value)
		{
			this.value = value;
		}
	}
}
