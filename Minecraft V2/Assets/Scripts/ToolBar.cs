using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolBar : MonoBehaviour
{
    public UIItemSlots[] slots;
    public RectTransform highlight;
    public Player player;
    public int slotIndex = 0;

    private void Start()
    {
        byte index = 1;
        foreach(UIItemSlots s in slots)
        {
            ItemStack stack = new ItemStack(index, Random.Range(2,65)); //Top int is exclusive
            ItemSlot slot = new ItemSlot(slots[index - 1], stack);
            index++;
        }
    }

    private void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if(scroll != 0)
        {
            if (scroll < 0)
                slotIndex++;
            else
                slotIndex--;

            if (slotIndex > slots.Length - 1)
                slotIndex = 0;
            if (slotIndex < 0)
                slotIndex = slots.Length - 1;

            highlight.position = slots[slotIndex].slotIcon.transform.position;
        }
    }

}
