using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Order 
{
    public ThingsData thingsData;
    public int amount;

    public Order(ThingsData _thingsData, int amt)
    {
        thingsData = _thingsData;
        amount = amt;
    }
}
