using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CreateThingsData")]
public class ThingsData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public int price;
    public float weight;

}
