using UnityEngine;
using System.Collections;

[System.Serializable]
public class BuyableDefinition
{
    public string Name;
    public int Cost;

    [TextArea(0, 10)]
    public string Description;

    public PurchaseType Type;
    public PurchaseCategory Category;

    public GameObject Object;
}
