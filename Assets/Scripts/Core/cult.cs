using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Cult
{
    public God god;
    public Church church;
    public List<Follower> followers = new();
    
    [Header("Resources")]
    public float money = 0f;

    public void AddMoney(float value)
    {
        money += Mathf.Max(0f, value);
    }

    public bool SpendMoney(float value)
    {
        if (value <= money)
        {
            money -= value;
            return true;
        }
        return false;
    }
}

