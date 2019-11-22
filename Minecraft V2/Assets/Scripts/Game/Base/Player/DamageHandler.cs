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

    public List<GameObject> Hearts = new List<GameObject>();

    bool lastIsGrounded;
    bool IsInAir;
    Vector3 LastGroundedpos;

    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
        world = GameObject.Find("World").GetComponent<World>();

        for(int i = 0; i < player.health / 2; i++)
        {
            GameObject heart = Instantiate(HealthPrefab, new Vector3(Healthbar.transform.position.x - 400 + i * 40, Healthbar.transform.position.y - 10, Healthbar.transform.position.z), Quaternion.identity, Healthbar.transform);
            Hearts.Add(heart);
        }
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
                int dmg = Mathf.FloorToInt((heightDiff - 3) * multiplayerDamage); ;
                player.health -= dmg;
                Debug.Log($"Taking {dmg} damage");
                UpdateUIHeartsState();
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

    void UpdateUIHeartsState()
    {
        int PlayerHealth = player.health;
        bool hasHalHeart = false;
        if (PlayerHealth % 2 != 0)
        {
            --PlayerHealth;
            hasHalHeart = true;
        }
        for(int i = 0; i < PlayerHealth / 2; i++)
        {
            HeartItem temp = Hearts[i].GetComponent<HeartItem>();
            temp.HeartStateChanged(true, true);
        }
        for (int i = PlayerHealth / 2; i < 10; i++)
        {
            HeartItem temp = Hearts[i].GetComponent<HeartItem>();
            temp.HeartStateChanged(true, false);
        }
        if (hasHalHeart)
        {
            HeartItem temp = Hearts[PlayerHealth / 2 + 2].GetComponent<HeartItem>();
            temp.HeartStateChanged(false, true);
        }
    }
}
