using System;
using System.Collections;
using System.Collections.Generic;
using Jy.NetworkComponents;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine.Pool;


public class NetworkComponentCollections : IEnumerable<NetworkComponent>
{
    Dictionary<ulong, NetworkComponent> components = new Dictionary<ulong, NetworkComponent>();

    public NetworkComponent this[ulong clientId]
    {
        get
        {
            if (components.TryGetValue(clientId, out var component))
            {
                return component;
            }
            return null;
        }
    }
    
    public NetworkComponentCollections(int capacity = 16)
    {
        components = new Dictionary<ulong, NetworkComponent>(capacity);
    }

    public void Add(ulong clientId, NetworkComponent component)
    {
        components[clientId] = component;
    }

    public bool Contains(ulong clientId)
    {
        return components.ContainsKey(clientId);
    }

    public bool TryGet(ulong clientId, out NetworkComponent component)
    {
        return components.TryGetValue(clientId, out component);
    }

    public NetworkComponent Get(ulong clientId)
    {
        if (components.TryGetValue(clientId, out var component))
        {
            return component;
        }
        return null;
    }

    public bool Remove(ulong clientId)
    {
        return components.Remove(clientId);
    }
    
    public IEnumerator<NetworkComponent> GetEnumerator()
    {
        return components.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class NetworkWorld
{
    uint frameNumber = 0;
    Dictionary<Type, NetworkComponentCollections> componentCollectionsByType = new Dictionary<Type, NetworkComponentCollections>();

    public void Add(ulong clientId, NetworkComponent component)
    {
        Type componentType = component.GetType();
        if (!componentCollectionsByType.TryGetValue(componentType, out var collection))
        {
            collection = new NetworkComponentCollections();
            componentCollectionsByType[componentType] = collection;
        }
        collection.Add(clientId, component);
    }

    public void Remove(ulong clientId, NetworkComponent component)
    {
        Type componentType = component.GetType();
        if (componentCollectionsByType.TryGetValue(componentType, out var collection))
        {
            Assert.IsTrue(collection[clientId] == component, $"Attempting to remove a different component for clientId {clientId} than the one stored in the collection."); 

            collection.Remove(clientId);
        }
    }

    public void NetworkUpdate()
    {
        foreach (var collection in componentCollectionsByType.Values)
        {
            foreach (var component in collection)
            {
                component.UpdateNetwork();  
            }
        }
    }

    public void IncreaseFrameNumber()
    {
        frameNumber++;
    }

    //public Jy.Packets.FrameSnapshot MakeCurrentWorldSnapshot()
    //{
    //    PooledObject<Jy.Packets.FrameSnapshot> snapshotPool = Jy.Packets.FrameSnapshot.GetPool();
    //}
}