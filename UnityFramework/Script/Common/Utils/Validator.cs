using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Validator
{
    public static bool IsValidReferences(out string invalidRefName, params Object[] objs)
    {
        invalidRefName = "";

        foreach (var obj in objs)
        {
            if (!obj)
            {
                invalidRefName = obj.name;
                return false;
            }
        }

        return true;
    }

    public static bool IsValidReferences(out string invalidRefName, params ScriptableObject[] scriptables)
    {
        invalidRefName = "";

        foreach (var scriptable in scriptables)
        {
            if (!scriptable)
            {
                invalidRefName = scriptable.name;
                return false;
            }
        }
 
        return true;
    }

    public static bool IsValidCollection<T>(IEnumerable<T> collection)
    {
        if (collection == null || collection.Count() == 0)
        {
            return false;
        }
        return true;
    }
}
