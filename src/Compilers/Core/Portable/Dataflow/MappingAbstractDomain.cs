using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.Semantics.Dataflow
{
    internal class MappingAbstractDomain<TKey, TValue> : AbstractDomain<IDictionary<TKey, TValue>>
    {
        private AbstractDomain<TValue> _valueDomain;

        public MappingAbstractDomain(AbstractDomain<TValue> valueDomain)
        {
            _valueDomain = valueDomain;
        }

        public override IDictionary<TKey, TValue> Bottom => new Dictionary<TKey, TValue>();

        public override int Compare(IDictionary<TKey, TValue> oldValue, IDictionary<TKey, TValue> newValue)
        {
            if ((oldValue == null && newValue != null) ||
                (oldValue != null && newValue == null))
                return -1;

            if (oldValue == null && newValue == null) return 0;

            if (ReferenceEquals(oldValue, newValue)) return 0;
            if (oldValue.Count != newValue.Count) return -1;

            foreach (var key in oldValue.Keys)
            {
                var newValueContainsKey = newValue.ContainsKey(key);
                if (!newValueContainsKey) return -1;
            }

            foreach (var entry in oldValue)
            {
                var value = newValue[entry.Key];
                var valuesAreEquals = _valueDomain.Compare(entry.Value, value);

                // old < new ?
                if (valuesAreEquals < 0) return -1;
            }

            return 0;
        }

        public override IDictionary<TKey, TValue> Merge(IDictionary<TKey, TValue> value1, IDictionary<TKey, TValue> value2)
        {
            if (value1 == null && value2 != null) return value2;
            if (value1 != null && value2 == null) return value1;
            if (value1 == null && value2 == null) return null;

            var result = new Dictionary<TKey, TValue>(value1);

            foreach (var entry in value2)
            {
                if (result.TryGetValue(entry.Key, out TValue value))
                {
                    value = _valueDomain.Merge(value, entry.Value);

                    if (value != null)
                    {
                        result[entry.Key] = value;
                    }
                    else
                    {
                        result.Remove(entry.Key);
                    }
                }
                else
                {
                    result.Add(entry.Key, entry.Value);
                }
            }

            return result;
        }
    }
}
