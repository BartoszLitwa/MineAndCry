using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DragAndDropHandler : MonoBehaviour
{
    [SerializeField] private UIItemSlots cursorSlot = null;
    private ItemSlot cursorItemSlot;

    [SerializeField] private GraphicRaycaster m_Raycaster = null;
    private PointerEventData m_PointerEventData;
    [SerializeField] private EventSystem m_EventSystem = null;

    World world;

    private void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
        cursorItemSlot = new ItemSlot(cursorSlot);
    }

    private void Update()
    {
        if (!world.inUI)
        {
            if(cursorSlot.itemslot.stack != null)
                cursorSlot.itemslot.TakeAll();

            return;
        }

        cursorSlot.transform.position = Input.mousePosition;

        if(Input.GetMouseButtonDown(0)) //Left mouse
        {
            HandleSlotClick(CheckForSlot());
        }
    }

    private void HandleSlotClick(UIItemSlots clickedSlot)
    {
        if (clickedSlot == null) //If not clicked any slot return
            return;

        if (!cursorSlot.HasItem && !clickedSlot.HasItem)
            return;

        if (clickedSlot.itemslot.isCreative)
        {
            cursorItemSlot.EmptySlot();
            cursorItemSlot.InsertStack(clickedSlot.itemslot.stack);
        }

        if(!cursorSlot.HasItem && clickedSlot.HasItem)
        {
            cursorItemSlot.InsertStack(clickedSlot.itemslot.TakeAll());
            return;
        }

        if (cursorSlot.HasItem && !clickedSlot.HasItem)
        {
            clickedSlot.itemslot.InsertStack(cursorItemSlot.TakeAll());
            return;
        }

        if (cursorSlot.HasItem && clickedSlot.HasItem)
        {
            if(cursorSlot.itemslot.stack.ID != clickedSlot.itemslot.stack.ID) //if both items are diffrent
            {
                ItemStack oldCursorSlot = cursorSlot.itemslot.TakeAll();
                ItemStack oldSlot = clickedSlot.itemslot.TakeAll();

                clickedSlot.itemslot.InsertStack(oldCursorSlot);
                cursorSlot.itemslot.InsertStack(oldSlot);
            }
            else if (cursorSlot.itemslot.stack.ID == clickedSlot.itemslot.stack.ID)//If both has the same items
            {
                clickedSlot.itemslot.stack.amount += cursorSlot.itemslot.stack.amount;
                if (clickedSlot.itemslot.stack.amount > 64) //If after adding items is more than 64(stack size)
                    clickedSlot.itemslot.stack.amount = 64;

                clickedSlot.UpdateSlot();
                cursorSlot.itemslot.TakeAll();
            }
        }
    }

    private UIItemSlots CheckForSlot()
    {
        m_PointerEventData = new PointerEventData(m_EventSystem);
        m_PointerEventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        m_Raycaster.Raycast(m_PointerEventData, results); //get the UI items under the mouse

        foreach(RaycastResult res in results)
        {
            if (res.gameObject.tag == "ItemSlot")
                return res.gameObject.GetComponent<UIItemSlots>();
        }

        return null;
    }
}
