using UnityEngine;
using System.Collections;

[System.Serializable]
public enum PurchaseCategory
{
    Weapons,
    Support,
}

[System.Serializable]
public enum PurchaseType
{
    Item,
    Weapon,
    Vehicle,
}

[System.Serializable]
public class PurchasableDefinition : ScriptableObject
{
    public PurchaseType PurchaseType = PurchaseType.Item;
    public PurchaseCategory Category = PurchaseCategory.Support;

    public GameObject Object;

    public string PurchaseName;
    public int Cost;

    [TextArea(0,10)]
    public string PurchaseDescription;
}
