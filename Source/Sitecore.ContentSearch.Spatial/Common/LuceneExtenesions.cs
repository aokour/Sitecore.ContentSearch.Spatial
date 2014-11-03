using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spatial4n.Core.Distance;
using Spatial4n.Core.Shapes;
using Spatial4n.Core.Shapes.Impl;
using System.Diagnostics;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Spatial.Util;
using System.Collections.Concurrent;

namespace Sitecore.ContentSearch.Spatial.Common
{
    internal static class LuceneExtensions
    {
        private static readonly ConcurrentDictionary<Key<string, IndexReader>, IBits> _docsWithFieldCache =
            new ConcurrentDictionary<Key<string, IndexReader>, IBits>();

        internal static IBits GetDocsWithField(this FieldCache fc, IndexReader reader, String field)
        {
            return _docsWithFieldCache.GetOrAdd(new Key<string, IndexReader>(field, reader),
                                                key =>
                                                DocsWithFieldCacheEntry_CreateValue(key.Item2,
                                                                                    new Entry(key.Item1, null), false));
        }

        private static IBits DocsWithFieldCacheEntry_CreateValue(IndexReader reader, Entry entryKey,
                                                                 bool setDocsWithField /* ignored */)
        {
            var field = entryKey.field;
            FixedBitSet res = null;
            var terms = new TermsEnumCompatibility(reader, field);
            var maxDoc = reader.MaxDoc;

            var term = terms.Next();
            if (term != null)
            {
                int termsDocCount = terms.GetDocCount();
                Debug.Assert(termsDocCount <= maxDoc);
                if (termsDocCount == maxDoc)
                {
                    // Fast case: all docs have this field:
                    return new MatchAllBits(maxDoc);
                }

                while (true)
                {
                    if (res == null)
                    {
                        // lazy init
                        res = new FixedBitSet(maxDoc);
                    }

                    var termDocs = reader.TermDocs(term);
                    while (termDocs.Next())
                    {
                        res.Set(termDocs.Doc);
                    }

                    term = terms.Next();
                    if (term == null)
                    {
                        break;
                    }
                }
            }
            if (res == null)
            {
                return new MatchNoBits(maxDoc);
            }
            int numSet = res.Cardinality();
            if (numSet >= maxDoc)
            {
                // The cardinality of the BitSet is maxDoc if all documents have a value.
                Debug.Assert(numSet == maxDoc);
                return new MatchAllBits(maxDoc);
            }
            return res;
        }
    }

    internal struct Key<T1, T2> : IEquatable<Key<T1, T2>>
    {
        public bool Equals(Key<T1, T2> other)
        {
            return EqualityComparer<T1>.Default.Equals(Item1, other.Item1) &&
                   EqualityComparer<T2>.Default.Equals(Item2, other.Item2);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            return obj is Key<T1, T2> && Equals((Key<T1, T2>)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (EqualityComparer<T1>.Default.GetHashCode(Item1) * 397) ^
                       EqualityComparer<T2>.Default.GetHashCode(Item2);
            }
        }

        public static bool operator ==(Key<T1, T2> left, Key<T1, T2> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Key<T1, T2> left, Key<T1, T2> right)
        {
            return !left.Equals(right);
        }

        public readonly T1 Item1;
        public readonly T2 Item2;

        public Key(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
    }

    /// <summary>
    /// Expert: Every composite-key in the internal cache is of this type.
    /// </summary>
    internal class Entry
    {
        internal readonly String field; // which Fieldable
        internal readonly Object custom; // which custom comparator or parser

        /* Creates one of these objects for a custom comparator/parser. */

        public Entry(String field, Object custom)
        {
            this.field = field;
            this.custom = custom;
        }

        /* Two of these are equal iff they reference the same field and type. */

        public override bool Equals(Object o)
        {
            var other = o as Entry;
            if (other != null)
            {
                if (other.field.Equals(field))
                {
                    if (other.custom == null)
                    {
                        if (custom == null) return true;
                    }
                    else if (other.custom.Equals(custom))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /* Composes a hashcode based on the field and type. */

        public override int GetHashCode()
        {
            return field.GetHashCode() ^ (custom == null ? 0 : custom.GetHashCode());
        }
    }
}
