using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIItemSlots : MonoBehaviour
{
    public bool isLinked = false;
    public ItemSlot itemslot;
    public Image slotImage;
    public Image slotIcon;
    public Text slotAmount;
    World world;

    private void Awake()
    {
        world = GameObject.Find("World").GetComponent<World>();
    }

    public bool HasItem
    {
        get {
            if (itemslot == null)
                return false;
            else
                return itemslot.HasItem;
        }
    }

    public void Link(ItemSlot _itemslot)
    {
        itemslot = _itemslot;
        isLinked = true;
        itemslot.LinkUISlot(this);
        UpdateSlot();
    }

    public void UpdateSlot()
    {
        if (itemslot != null && itemslot.HasItem)
        {
            slotIcon.sprite = world.blocktypes[itemslot.stack.ID].icon;
            slotAmount.text = itemslot.stack.amount.ToString();
            slotIcon.enabled = true;
            slotAmount.enabled = true;
        }
        else
        {
            Clear();
        }
    }

    public void Clear()
    {
        slotIcon.sprite = null;
        slotAmount.text = "";
        slotIcon.enabled = false;
        slotAmount.enabled = false;
    }

    private void OnDestroy()
    {
        if (itemslot != null) //isLinked
        {
            itemslot.UnLinkUISlot();
        }
    }

    public void UnLink()
    {
        itemslot.UnLinkUISlot();
        itemslot = null;
        UpdateSlot();
    }
}

public class ItemSlot
{
    public ItemStack stack = null;
    private UIItemSlots uiItemSlot = null;
    public bool isCreative;

    public ItemSlot(UIItemSlots _uiitemslot)
    {
        stack = null;
        uiItemSlot = _uiitemslot;
        uiItemSlot.Link(this);
    }

    public ItemSlot(UIItemSlots _uiitemslot, ItemStack _stack)
    {
        stack = _stack;
        uiItemSlot = _uiitemslot;
        uiItemSlot.Link(this);
    }

    public void LinkUISlot(UIItemSlots uiSlot)
    {
        uiItemSlot = uiSlot;
    }

    public void UnLinkUISlot()
    {
        uiItemSlot = null;
    }

    public void EmptySlot()
    {
        stack = null;
        if(uiItemSlot != null)
        {
            uiItemSlot.UpdateSlot();
        }
    }

    public int Take(int amt)
    {
        if (amt > stack.amount)
        {
            int _amt = stack.amount;
            EmptySlot();
            return _amt;
        }
        else if(amt < stack.amount)
        {
            stack.amount -= amt;
            uiItemSlot.UpdateSlot();
            return  amt;
        }
        else
        {
            EmptySlot();
            return amt;
        }
    }

    public ItemStack TakeAll()
    {
        ItemStack handover = new ItemStack(stack.ID, stack.amount);
        EmptySlot();
        return handover;
    }

    public void InsertStack(ItemStack _itemstack)
    {
        stack = _itemstack;
        uiItemSlot.UpdateSlot();
    }

    public bool HasItem
    {
        get {
            if (stack != null)
                return true;
            else
                return false;
        }
    }
}
