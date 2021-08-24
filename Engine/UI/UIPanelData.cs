using System.Collections;
using UnityEngine;

public abstract class UIPanelData<T>
{
    public abstract bool IsSameData(T other);    
}