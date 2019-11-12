using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SurvivalInevntory : MonoBehaviour
{
    [SerializeField] private GameObject ToolbarSurvivalInventory;
    [SerializeField] private GameObject Toolbar;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private World world;

    public List<UIItemSlots> itemslots = new List<UIItemSlots>();

    void Start()
    {
        for(int i = 0; i < 27; i++) //3 rows 9 columns = 27
        {
            GameObject slot = Instantiate(slotPrefab, transform);

            UIItemSlots Uislot = slot.GetComponent<UIItemSlots>();
            //ItemStack stack = new ItemStack(0, 0);
            ItemSlot itemSlot = new ItemSlot(Uislot);
            itemSlot.isCreative = false;
            itemslots.Add(Uislot);
        }

        UIItemSlots[] UISlots = Toolbar.GetComponents<UIItemSlots>();
        for (int i = 0; i < 9; i++)
        {
            GameObject slot = Instantiate(slotPrefab, ToolbarSurvivalInventory.transform);

            UIItemSlots UI = slot.GetComponent<UIItemSlots>();
            UI = UISlots[i];
        }
    }

    void Update()
    {
        
    }
}
