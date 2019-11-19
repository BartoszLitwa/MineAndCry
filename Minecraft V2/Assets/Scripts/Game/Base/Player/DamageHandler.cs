using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageHandler : MonoBehaviour
{
    [SerializeField] int minHeightToGetDamage = 4;
    [SerializeField] float multiplayerDamage = 0.5f;

    [SerializeField] GameObject Healthbar;
    [SerializeField] GameObject HealthPrefab;

    [SerializeField] GameObject Hungerbar;
    [SerializeField] GameObject HungerPrefab;

    [SerializeField] GameObject DeadScreenPanel;
    [SerializeField] Text DeadScreentText;

    [SerializeField] Player player;
    [SerializeField] World world;

    bool lastIsGrounded;
    bool IsInAir;
    Vector3 LastGroundedpos;

    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
        world = GameObject.Find("World").GetComponent<World>();
    }
    
    void Update()
    {
        if (!world.WorldLoaded)
        {
            LastGroundedpos = Camera.main.transform.position;
            return;
        }

        if (!IsInAir && player.isGrounded != lastIsGrounded && !player.isGrounded)
        {
            Debug.Log(Camera.main.transform.position + " InAir");
            IsInAir = true;

        }
        if(IsInAir && !player.isGrounded != lastIsGrounded && player.isGrounded)
        {
            Debug.Log(Camera.main.transform.position + " !InAir");
            int heightDiff = (int)Mathf.Abs(LastGroundedpos.y - Camera.main.transform.position.y) + 1;
            Debug.Log("Height diffrence: " + heightDiff);
            if (heightDiff >= minHeightToGetDamage)
            {
                int dmg = Mathf.FloorToInt(heightDiff * multiplayerDamage); ;
                player.health -= dmg;
                Debug.Log($"Taking {dmg} damage");
                if (player.health < 0)
                {
                    player.isDead = true;
                }
            }

            LastGroundedpos = Camera.main.transform.position;
            IsInAir = false;
        }

        lastIsGrounded = player.isGrounded;
    }
}
