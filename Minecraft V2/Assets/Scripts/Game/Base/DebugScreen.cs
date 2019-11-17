using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugScreen : MonoBehaviour
{
    World world;
    Player player;
    Text text;

    float frameRate;
    float timer;

    int halfWorldSizeInChunks;
    int halfWorldSizeInVoxels;

    void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
        player = GameObject.Find("Player").GetComponent<Player>();
        text = GetComponent<Text>();

        halfWorldSizeInChunks = VoxelData.WorldSizeInChunks / 2;
        halfWorldSizeInVoxels = VoxelData.WorldSizeInVoxels / 2;
    }

    void Update()
    {
        string debugtext = "FPS: " + frameRate + "\n";
        debugtext += "X/Y/Z: " + (Mathf.FloorToInt(world.player.transform.position.x) - halfWorldSizeInVoxels) + "/" + Mathf.FloorToInt(world.player.transform.position.y)
                                + "/" + (Mathf.FloorToInt(world.player.transform.position.z) - halfWorldSizeInVoxels) + "\n";
        debugtext += "Chunk: " + (world.playerChunkCoord.x - halfWorldSizeInChunks) + "/" + (world.playerChunkCoord.z - halfWorldSizeInChunks) + "\n";
        debugtext += "Health: " + player.health;

        text.text = debugtext;

        if (timer > 1f)
        {
            frameRate = (int)(1f / Time.unscaledDeltaTime);
            timer = 0;
        }
        else
            timer += Time.deltaTime;

    }
}
