using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Structure
{
    public static Queue<VoxelMod> MakeTree(Vector3 pos, int minTreeheight, int MaxTrunkheight)
    {
        Queue<VoxelMod> queue = new Queue<VoxelMod>();
        int height = (int)(MaxTrunkheight * PerlinNoise.Get2DPerlin(new Vector2(pos.x, pos.z), 250f, 3f));

        if (height < minTreeheight)
            height = minTreeheight;

        for(int i = 0; i < height; i++)
        {
            queue.Enqueue(new VoxelMod(new Vector3(pos.x, pos.y + i, pos.z), (byte)VoxelData.BlockTypes.OakWood));
        }

        queue.Enqueue(new VoxelMod(new Vector3(pos.x, pos.y + height + 1, pos.z), (byte)VoxelData.BlockTypes.Leaves));

        for(int x = -1; x < 2; x++)
        {
            for (int z = -1; z < 2; z++)
            {
                queue.Enqueue(new VoxelMod(new Vector3(pos.x + x, pos.y + height, pos.z + z), (byte)VoxelData.BlockTypes.Leaves));
            }
        }

        for(int x = -2; x < 3; x++)
        {
            for (int y = -2; y < 0; y++)
            {
                for (int z = -2; z < 3; z++)
                {
                    queue.Enqueue(new VoxelMod(new Vector3(pos.x + x, pos.y + height + y, pos.z + z), (byte)VoxelData.BlockTypes.Leaves));
                }
            }
        }

        queue.Enqueue(new VoxelMod(new Vector3(pos.x, pos.y + height - 1, pos.z), (byte)VoxelData.BlockTypes.OakWood));
        queue.Enqueue(new VoxelMod(new Vector3(pos.x, pos.y + height - 2, pos.z), (byte)VoxelData.BlockTypes.OakWood));

        return queue;
    }
}
