using System;
using System.Collections.Generic;
using UnityEngine;

namespace Invasion.Tables.Runtime
{
    [Serializable]
    public abstract class TableItem<TKey>
    {
        public TKey key;
    }

    public class TableBase<KeyT, TableItemT> : ScriptableObject, ISerializationCallbackReceiver
        where TableItemT : TableItem<KeyT>
    {
        [SerializeField]
        public List<TableItemT> items = new List<TableItemT>();

        Dictionary<KeyT, TableItemT> table = new Dictionary<KeyT, TableItemT>();

        public TableItemT this[KeyT key]
        {
            get
            {
                if (table.TryGetValue(key, out var item))
                {
                    return item;
                }
                else
                {
                    Debug.LogWarning($"Key not found: {key}");
                    return null;
                }
            }
        }


        public T Get<T>(KeyT key) where T : TableItemT
        {
            if (table.TryGetValue(key, out var item))
            {
                return item as T;
            }
            else
            {
                Debug.LogWarning($"Key not found: {key}");
                return null;
            }
        }

        public void OnBeforeSerialize()
        {
            //nothing to do
        }

        public void OnAfterDeserialize()
        {
            table.Clear();
            foreach (var item in items)
            {
                if (!table.ContainsKey(item.key))
                {
                    table.Add(item.key, item);
                }
                else
                {
                    Debug.LogWarning($"Duplicate key found: {item.key}. Skipping.");
                }
            }
        }
    }
}
