using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemStack
{
    public byte ID;
    public int amount;

    public ItemStack(byte _id, int _amount)
    {
        ID = _id;
        amount = _amount;
    }
}
