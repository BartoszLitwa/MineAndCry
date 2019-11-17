using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageHandler : MonoBehaviour
{
    [SerializeField] int minHeightToGetDamage = 4;
    [SerializeField] float multiplayerDamage = 1f;

    [SerializeField] GameObject Healthbar;
    [SerializeField] GameObject HealthPrefab;

    [SerializeField] GameObject Hungerbar;
    [SerializeField] GameObject HungerPrefab;

    [SerializeField] Player player;
    [SerializeField] World world;

    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
        world = GameObject.Find("World").GetComponent<World>();
    }

    bool lastIsGrounded;
    bool CheckedFallDamage;
    int LastCheckedHeight;
    void Update()
    {
        if(player.isGrounded != lastIsGrounded)
        {
            CheckedFallDamage = false;
        }

        if (!CheckedFallDamage && !player.isGrounded)
        {
            Vector3 pos = Camera.main.transform.position;
            Chunk thisChunk = world?.getChunkFromVector3(pos);

            bool FoundGround = false;
            int height = 0;
            for (int y = Mathf.FloorToInt(pos.y - player.playerHeight); !FoundGround; y--)
            {
                VoxelState state = thisChunk?.GetVoxelFromGlobalVector3(new Vector3(pos.x, y, pos.z));
                if (state == null) return;

                if (state.id != (byte)VoxelData.BlockTypes.Air)
                {
                    height = y;
                    FoundGround = true;
                }
            }

            LastCheckedHeight = height;
            CheckedFallDamage = true;
        }

        if(player.isGrounded && CheckedFallDamage && LastCheckedHeight >= minHeightToGetDamage)
        {
            player.health -= Mathf.FloorToInt(LastCheckedHeight * multiplayerDamage);
            if (player.health < 0)
                player.health = 0;
        }

        lastIsGrounded = player.isGrounded;
    }
}
