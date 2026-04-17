
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public class BlackboardVariable
{
    [field:SerializeField] public string Key {get;set;}
}

[Serializable]
public class BlackboardVariable<T> : BlackboardVariable
{
    [field:SerializeField] public T Value {get;set;}

    public BlackboardVariable(string key, T value)
    {
        Key = key;
        Value = value;
    }
}

public class FsmBlackboard: MonoBehaviour
{
    Dictionary<string, BlackboardVariable> variables
        = new Dictionary<string, BlackboardVariable>(16);

    public ReadOnlyDictionary<string, BlackboardVariable> Varibles
    {
        get
        {
            if(null == variables)
                return null;

            if(0 == variables.Count)
                return null;

            return new ReadOnlyDictionary<string, BlackboardVariable>(variables);
        }
    }

    public bool Has(string key)
    {
        if(null == variables)
            return false;

        return variables.ContainsKey(key);
    }

    public void Add(string key, BlackboardVariable variable)
    {
        Assert.IsFalse(Has(key));
        
        variables.Add(key, variable);
    }

    public void Remove(string key)
    {
        Assert.IsTrue(Has(key));
        
        variables.Remove(key);
    }

    public void SetOrAdd<T>(string key, T value)
    {
        if (Has(key))
        {
            var variable = variables[key] as BlackboardVariable<T>;
            Assert.IsNotNull(variable);
            variable.Value = value;
        }
        else
        {
            Add(key, new BlackboardVariable<T>(key, value));
        }
    }

    public T Get<T>(string key)
    {
        Assert.IsTrue(Has(key));
        
        var variable = variables[key] as BlackboardVariable<T>;
        Assert.IsNotNull(variable);
        
        return variable.Value;
    }
}