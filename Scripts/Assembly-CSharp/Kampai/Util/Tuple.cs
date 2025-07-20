using System;

namespace Kampai.Util
{
	public static class Tuple
	{
		public static Tuple<T1, T2> Create<T1, T2>(T1 first, T2 second)
		{
			return new Tuple<T1, T2>(first, second);
		}

		public static Tuple<T1, T2, T3> Create<T1, T2, T3>(T1 first, T2 second, T3 third)
		{
			return new Tuple<T1, T2, T3>(first, second, third);
		}

		public static Tuple<T1, T2, T3, T4> Create<T1, T2, T3, T4>(T1 first, T2 second, T3 third, T4 fourth)
		{
			return new Tuple<T1, T2, T3, T4>(first, second, third, fourth);
		}

		public static Tuple<T1, T2, T3, T4, T5> Create<T1, T2, T3, T4, T5>(T1 first, T2 second, T3 third, T4 fourth, T5 fifth)
		{
			return new Tuple<T1, T2, T3, T4, T5>(first, second, third, fourth, fifth);
		}

		public static Tuple<T1, T2, T3, T4, T5, T6> Create<T1, T2, T3, T4, T5, T6>(T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth)
		{
			return new Tuple<T1, T2, T3, T4, T5, T6>(first, second, third, fourth, fifth, sixth);
		}

		public static Tuple<T1, T2, T3, T4, T5, T6, T7> Create<T1, T2, T3, T4, T5, T6, T7>(T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh)
		{
			return new Tuple<T1, T2, T3, T4, T5, T6, T7>(first, second, third, fourth, fifth, sixth, seventh);
		}

		public static Tuple<T1, T2, T3, T4, T5, T6, T7, T8> Create<T1, T2, T3, T4, T5, T6, T7, T8>(T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eighth)
		{
			return new Tuple<T1, T2, T3, T4, T5, T6, T7, T8>(first, second, third, fourth, fifth, sixth, seventh, eighth);
		}
	}
	public sealed class Tuple<T1, T2> : IEquatable<Tuple<T1, T2>>, ITuple
	{
		public T1 Item1 { get; set; }

		public T2 Item2 { get; set; }

		public Tuple(T1 first, T2 second)
		{
			Item1 = first;
			Item2 = second;
		}

		public override string ToString()
		{
			return string.Format("Tuple({0}, {1})", Item1, Item2);
		}

		public override int GetHashCode()
		{
			return Item1.GetHashCode() ^ Item2.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}
			return Equals((Tuple<T1, T2>)obj);
		}

		public bool Equals(Tuple<T1, T2> other)
		{
			return other.Item1.Equals(Item1) && other.Item2.Equals(Item2);
		}
	}
	public sealed class Tuple<T1, T2, T3> : IEquatable<Tuple<T1, T2, T3>>, ITuple
	{
		public T1 Item1 { get; set; }

		public T2 Item2 { get; set; }

		public T3 Item3 { get; set; }

		public Tuple(T1 first, T2 second, T3 third)
		{
			Item1 = first;
			Item2 = second;
			Item3 = third;
		}

		public override string ToString()
		{
			return string.Format("Tuple({0}, {1}, {2})", Item1, Item2, Item3);
		}

		public override int GetHashCode()
		{
			return Item1.GetHashCode() ^ Item2.GetHashCode() ^ Item3.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}
			return Equals((Tuple<T1, T2, T3>)obj);
		}

		public bool Equals(Tuple<T1, T2, T3> other)
		{
			return other.Item1.Equals(Item1) && other.Item2.Equals(Item2) && other.Item3.Equals(Item3);
		}
	}
	public sealed class Tuple<T1, T2, T3, T4> : IEquatable<Tuple<T1, T2, T3, T4>>, ITuple
	{
		public T1 Item1 { get; set; }

		public T2 Item2 { get; set; }

		public T3 Item3 { get; set; }

		public T4 Item4 { get; set; }

		public Tuple(T1 first, T2 second, T3 third, T4 fourth)
		{
			Item1 = first;
			Item2 = second;
			Item3 = third;
			Item4 = fourth;
		}

		public override string ToString()
		{
			return string.Format("Tuple({0}, {1}, {2}, {3})", Item1, Item2, Item3, Item4);
		}

		public override int GetHashCode()
		{
			return Item1.GetHashCode() ^ Item2.GetHashCode() ^ Item3.GetHashCode() ^ Item4.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}
			return Equals((Tuple<T1, T2, T3, T4>)obj);
		}

		public bool Equals(Tuple<T1, T2, T3, T4> other)
		{
			return other.Item1.Equals(Item1) && other.Item2.Equals(Item2) && other.Item3.Equals(Item3) && other.Item4.Equals(Item4);
		}
	}
	public sealed class Tuple<T1, T2, T3, T4, T5> : IEquatable<Tuple<T1, T2, T3, T4, T5>>, ITuple
	{
		public T1 Item1 { get; set; }

		public T2 Item2 { get; set; }

		public T3 Item3 { get; set; }

		public T4 Item4 { get; set; }

		public T5 Item5 { get; set; }

		public Tuple(T1 first, T2 second, T3 third, T4 fourth, T5 fifth)
		{
			Item1 = first;
			Item2 = second;
			Item3 = third;
			Item4 = fourth;
			Item5 = fifth;
		}

		public override string ToString()
		{
			return string.Format("Tuple({0}, {1}, {2}, {3}, {4})", Item1, Item2, Item3, Item4, Item5);
		}

		public override int GetHashCode()
		{
			return Item1.GetHashCode() ^ Item2.GetHashCode() ^ Item3.GetHashCode() ^ Item4.GetHashCode() ^ Item5.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}
			return Equals((Tuple<T1, T2, T3, T4, T5>)obj);
		}

		public bool Equals(Tuple<T1, T2, T3, T4, T5> other)
		{
			return other.Item1.Equals(Item1) && other.Item2.Equals(Item2) && other.Item3.Equals(Item3) && other.Item4.Equals(Item4) && other.Item5.Equals(Item5);
		}
	}
	public sealed class Tuple<T1, T2, T3, T4, T5, T6> : IEquatable<Tuple<T1, T2, T3, T4, T5, T6>>, ITuple
	{
		public T1 Item1 { get; set; }

		public T2 Item2 { get; set; }

		public T3 Item3 { get; set; }

		public T4 Item4 { get; set; }

		public T5 Item5 { get; set; }

		public T6 Item6 { get; set; }

		public Tuple(T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth)
		{
			Item1 = first;
			Item2 = second;
			Item3 = third;
			Item4 = fourth;
			Item5 = fifth;
			Item6 = sixth;
		}

		public override string ToString()
		{
			return string.Format("Tuple({0}, {1}, {2}, {3}, {4}, {5})", Item1, Item2, Item3, Item4, Item5, Item6);
		}

		public override int GetHashCode()
		{
			return Item1.GetHashCode() ^ Item2.GetHashCode() ^ Item3.GetHashCode() ^ Item4.GetHashCode() ^ Item5.GetHashCode() ^ Item6.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}
			return Equals((Tuple<T1, T2, T3, T4, T5, T6>)obj);
		}

		public bool Equals(Tuple<T1, T2, T3, T4, T5, T6> other)
		{
			return other.Item1.Equals(Item1) && other.Item2.Equals(Item2) && other.Item3.Equals(Item3) && other.Item4.Equals(Item4) && other.Item5.Equals(Item5) && other.Item6.Equals(Item6);
		}
	}
	public sealed class Tuple<T1, T2, T3, T4, T5, T6, T7> : IEquatable<Tuple<T1, T2, T3, T4, T5, T6, T7>>, ITuple
	{
		public T1 Item1 { get; set; }

		public T2 Item2 { get; set; }

		public T3 Item3 { get; set; }

		public T4 Item4 { get; set; }

		public T5 Item5 { get; set; }

		public T6 Item6 { get; set; }

		public T7 Item7 { get; set; }

		public Tuple(T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh)
		{
			Item1 = first;
			Item2 = second;
			Item3 = third;
			Item4 = fourth;
			Item5 = fifth;
			Item6 = sixth;
			Item7 = seventh;
		}

		public override string ToString()
		{
			return string.Format("Tuple({0}, {1}, {2}, {3}, {4}, {5}, {6})", Item1, Item2, Item3, Item4, Item5, Item6, Item7);
		}

		public override int GetHashCode()
		{
			return Item1.GetHashCode() ^ Item2.GetHashCode() ^ Item3.GetHashCode() ^ Item4.GetHashCode() ^ Item5.GetHashCode() ^ Item6.GetHashCode() ^ Item7.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}
			return Equals((Tuple<T1, T2, T3, T4, T5, T6, T7>)obj);
		}

		public bool Equals(Tuple<T1, T2, T3, T4, T5, T6, T7> other)
		{
			return other.Item1.Equals(Item1) && other.Item2.Equals(Item2) && other.Item3.Equals(Item3) && other.Item4.Equals(Item4) && other.Item5.Equals(Item5) && other.Item6.Equals(Item6) && other.Item7.Equals(Item7);
		}
	}
	public sealed class Tuple<T1, T2, T3, T4, T5, T6, T7, T8> : IEquatable<Tuple<T1, T2, T3, T4, T5, T6, T7, T8>>, ITuple
	{
		public T1 Item1 { get; set; }

		public T2 Item2 { get; set; }

		public T3 Item3 { get; set; }

		public T4 Item4 { get; set; }

		public T5 Item5 { get; set; }

		public T6 Item6 { get; set; }

		public T7 Item7 { get; set; }

		public T8 Item8 { get; set; }

		public Tuple(T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eighth)
		{
			Item1 = first;
			Item2 = second;
			Item3 = third;
			Item4 = fourth;
			Item5 = fifth;
			Item6 = sixth;
			Item7 = seventh;
			Item8 = eighth;
		}

		public override string ToString()
		{
			return string.Format("Tuple({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})", Item1, Item2, Item3, Item4, Item5, Item6, Item7, Item8);
		}

		public override int GetHashCode()
		{
			return Item1.GetHashCode() ^ Item2.GetHashCode() ^ Item3.GetHashCode() ^ Item4.GetHashCode() ^ Item5.GetHashCode() ^ Item6.GetHashCode() ^ Item7.GetHashCode() ^ Item8.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}
			return Equals((Tuple<T1, T2, T3, T4, T5, T6, T7, T8>)obj);
		}

		public bool Equals(Tuple<T1, T2, T3, T4, T5, T6, T7, T8> other)
		{
			return other.Item1.Equals(Item1) && other.Item2.Equals(Item2) && other.Item3.Equals(Item3) && other.Item4.Equals(Item4) && other.Item5.Equals(Item5) && other.Item6.Equals(Item6) && other.Item7.Equals(Item7) && other.Item8.Equals(Item8);
		}
	}
}
