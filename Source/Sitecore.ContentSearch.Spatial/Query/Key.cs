using System;
using System.Collections.Generic;

namespace Sitecore.ContentSearch.Spatial
{
    internal struct Key<T1, T2> : IEquatable<Key<T1, T2>>
    {
        public readonly T1 Item1;

        public readonly T2 Item2;

        public Key(T1 item1, T2 item2)
        {
            this.Item1 = item1;
            this.Item2 = item2;
        }

        public bool Equals(Key<T1, T2> other)
        {
            if (!EqualityComparer<T1>.Default.Equals(this.Item1, other.Item1))
            {
                return false;
            }
            return EqualityComparer<T2>.Default.Equals(this.Item2, other.Item2);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is Key<T1, T2>))
            {
                return false;
            }
            return this.Equals((Key<T1, T2>)obj);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<T1>.Default.GetHashCode(this.Item1) * 397 ^ EqualityComparer<T2>.Default.GetHashCode(this.Item2);
        }

        public static bool operator ==(Key<T1, T2> left, Key<T1, T2> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Key<T1, T2> left, Key<T1, T2> right)
        {
            return !left.Equals(right);
        }
    }
}