using System;
using System.Collections.Generic;
using UnityEngine;

public static class ArgumentParser
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void Reset()
    {
        arguments.Clear();
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            if (arg.StartsWith("-"))
            {
                string key = arg.Substring(1);
                if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                {
                    arguments[key] = args[i + 1];
                    i++;
                }
                else
                {
                    arguments[key] = true;
                }
            }
        }
    }
    
    public static void Print()
    {
        Debug.Log("Parsed Arguments:");
        foreach (var kvp in arguments)
        {
            Debug.Log($"Key: {kvp.Key}, Value: {kvp.Value}");
        }
    }

    static Dictionary<string, object> arguments = new Dictionary<string, object>();
    static ArgumentParser()
    {
        Reset();
    }

    public static bool GetBoolean(string key, bool defaultValue = false)
    {
        if (arguments.TryGetValue(key, out object objValue))
        {
             if(bool.TryParse(objValue.ToString(), out bool boolValue))
             {
                 return boolValue;
             }
             else
            {
                return defaultValue;
            }
        }
        else
        {
            return defaultValue;
        }
    }   

    public static string GetString(string key, string defaultValue = "")
    {
        if (arguments.TryGetValue(key, out object objValue) && objValue is string strValue)
        {
            return strValue;
        }
        else
        {
            return defaultValue;   
        }
    }

    public static int GetInt(string key, int defaultValue = 0)
    {
        if (arguments.TryGetValue(key, out object objValue))
        {
            if (objValue is int intValue)
            {
                return intValue;
            }
            else if (objValue is string strValue && int.TryParse(strValue, out int parsedInt))
            {
                return parsedInt;
            }
        }
        return defaultValue;
    }
}