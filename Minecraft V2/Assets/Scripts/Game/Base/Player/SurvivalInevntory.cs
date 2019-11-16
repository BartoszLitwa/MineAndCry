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
    [SerializeField] private Player player;
    [SerializeField] private ToolBar playerToolbar;

    void Start()
    {
        for(int i = 0; i < 27; i++) //3 rows 9 columns = 27
        {
            GameObject slot = Instantiate(slotPrefab, transform);

            UIItemSlots Uislot = slot.GetComponent<UIItemSlots>();
            ItemSlot itemSlot = new ItemSlot(Uislot);
            itemSlot.isCreative = false;
            Helpers.itemslots.Add(Uislot);
        }
    }

    void Update()
    {
        
    }
}
