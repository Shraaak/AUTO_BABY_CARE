using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CustomerStateType")]
public class CustomerStateConfig : ScriptableObject
{
    public CustomerStateType stateType;
    public Sprite icon; 

    [Tooltip("状态持续时间")]
    public float duration; 

}