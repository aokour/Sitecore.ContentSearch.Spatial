using System;

namespace Sitecore.ContentSearch.Spatial
{
    internal class Entry
    {
        internal readonly string Field;

        internal readonly object Custom;

        public Entry(string field, object custom)
        {
            this.Field = field;
            this.Custom = custom;
        }

        public override bool Equals(object o)
        {
            Entry entry = o as Entry;
            if (entry != null && entry.Field.Equals(this.Field))
            {
                if (entry.Custom == null)
                {
                    if (this.Custom == null)
                    {
                        return true;
                    }
                }
                else if (entry.Custom.Equals(this.Custom))
                {
                    return true;
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.Field.GetHashCode() ^ (this.Custom == null ? 0 : this.Custom.GetHashCode());
        }
    }
}