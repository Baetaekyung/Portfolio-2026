using System;
using UnityEngine;

[Serializable]
public class ResourceHaveData
{
    public int resourceIndex;
    [HideInInspector] public int haveAmount;
    public int maxHaveAmount = -1;
}

public partial class MainUI
{
    [SerializeField] private ResourceHaveData[] resourceHaveData = new ResourceHaveData[3];
    [SerializeField] private Transform resourceHaveFieldTrm;

    
}
