
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
}

public class FsmBlackboard: MonoBehaviour
{
    Dictionary<string, BlackboardVariable> variables;

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
        return variables.ContainsKey(key);
    }
    
    public BlackboardVariable this [string key]
    {
        get
        {
            Assert.IsTrue(Has(key));

            return variables[key];
        }

        set
        {
            if(!Has(key))
            {
                variables.Add(key, value);
            }
            else
            {
                variables[key] = value;
            }
        }
    }
}