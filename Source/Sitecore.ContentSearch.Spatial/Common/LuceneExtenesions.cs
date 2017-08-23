using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Spatial.Util;
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Sitecore.ContentSearch.Spatial
{
    internal static class FieldCacheExtensions
    {
        private readonly static ConcurrentDictionary<Key<string, IndexReader>, IBits> DocsWithFieldCache;

        static FieldCacheExtensions()
        {
            FieldCacheExtensions.DocsWithFieldCache = new ConcurrentDictionary<Key<string, IndexReader>, IBits>();
        }

        private static IBits DocsWithFieldCacheEntryCreateValue(IndexReader reader, Entry entryKey)
        {
            string field = entryKey.Field;
            FixedBitSet fixedBitSet = null;
            TermsEnumCompatibility termsEnumCompatibility = new TermsEnumCompatibility(reader, field);
            int maxDoc = reader.MaxDoc;
            Term term = termsEnumCompatibility.Next();
            if (term != null)
            {
                if (termsEnumCompatibility.GetDocCount() == maxDoc)
                {
                    return new MatchAllBits(maxDoc);
                }
                do
                {
                    if (fixedBitSet == null)
                    {
                        fixedBitSet = new FixedBitSet(maxDoc);
                    }
                    TermDocs termDoc = reader.TermDocs(term);
                    while (termDoc.Next())
                    {
                        fixedBitSet.Set(termDoc.Doc);
                    }
                    term = termsEnumCompatibility.Next();
                }
                while (term != null);
            }
            if (fixedBitSet == null)
            {
                return new MatchNoBits(maxDoc);
            }
            if (fixedBitSet.Cardinality() < maxDoc)
            {
                return fixedBitSet;
            }
            return new MatchAllBits(maxDoc);
        }

        internal static IBits GetDocsWithField(this FieldCache fc, IndexReader reader, string field)
        {
            return FieldCacheExtensions.DocsWithFieldCache.GetOrAdd(new Key<string, IndexReader>(field, reader), (Key<string, IndexReader> key) => FieldCacheExtensions.DocsWithFieldCacheEntryCreateValue(key.Item2, new Entry(key.Item1, null)));
        }
    }
}