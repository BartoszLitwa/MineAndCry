using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Structure
{
    public enum StructureType { None, Tree, Cactus, SmallHouse};

    public static Queue<VoxelMod> GenerateMajorFlora(StructureType type, Vector3 pos, int minTrunkheight, int maxTrunkheight)
    {
        switch (type)
        {
            case StructureType.Tree:
                return MakeTree(pos, minTrunkheight, maxTrunkheight);
            case StructureType.Cactus:
                return MakeCactus(pos, minTrunkheight, maxTrunkheight);
            case StructureType.SmallHouse:
                return MakeSmallHouse(pos);
        }

        return new Queue<VoxelMod>();
    }

    public static Queue<VoxelMod> MakeTree(Vector3 pos, int minTreeheight, int MaxTreeheight)
    {
        Queue<VoxelMod> queue = new Queue<VoxelMod>();
        int height = (int)(MaxTreeheight * PerlinNoise.Get2DPerlin(new Vector2(pos.x, pos.z), 250f, 3f));

        if (height < minTreeheight)
            height = minTreeheight;

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

        for (int i = 1; i < height; i++)
        {
            queue.Enqueue(new VoxelMod(new Vector3(pos.x, pos.y + i, pos.z), (byte)VoxelData.BlockTypes.OakWood));
        }

        return queue;
    }

    public static Queue<VoxelMod> MakeCactus(Vector3 pos, int minTreeheight, int MaxTreeheight)
    {
        Queue<VoxelMod> queue = new Queue<VoxelMod>();
        int height = (int)(MaxTreeheight * PerlinNoise.Get2DPerlin(new Vector2(pos.x, pos.z), 2534f, 2f));

        if (height < minTreeheight)
            height = minTreeheight;

        for (int i = 1; i < height; i++)
        {
            queue.Enqueue(new VoxelMod(new Vector3(pos.x, pos.y + i, pos.z), (byte)VoxelData.BlockTypes.Cactus));
        }

        return queue;
    }

    public static Queue<VoxelMod> MakeSmallHouse(Vector3 pos)
    {
        Queue<VoxelMod> queue = new Queue<VoxelMod>();

        for(int x = 0; x < 5; x++)
        {
            for(int z = 0; z < 5; z++)
            {
                queue.Enqueue(new VoxelMod(new Vector3(pos.x + x, pos.y + 1, pos.z + z), (byte)VoxelData.BlockTypes.Cobble)); //Floor
                if(x == 0 && z == 0 || x == 0 && z == 4 || x == 4 && z== 0 || x == 4 && z == 4)
                {
                    for (int y = 2; y < 6; y++)
                    {
                        queue.Enqueue(new VoxelMod(new Vector3(pos.x + x, pos.y + y, pos.z + z), (byte)VoxelData.BlockTypes.OakWood)); //Corners
                    }
                }

                if(x == 1 && z == 0 || x == 1 && z == 4 || x == 2 && z == 0 || x == 2 && z == 4 || x == 3 && z == 0 || x == 3 && z == 4 ||
                   z == 1 && x == 0 || z == 1 && x == 4 || z == 2 && x == 0 || z == 2 && x == 4 || z == 3 && x == 0 || z == 3 && x == 4)
                {
                    for (int y = 2; y < 6; y++)
                    {
                        if(y != 5)
                            queue.Enqueue(new VoxelMod(new Vector3(pos.x + x, pos.y + y, pos.z + z), (byte)VoxelData.BlockTypes.Planks)); //Walls
                        else
                            queue.Enqueue(new VoxelMod(new Vector3(pos.x + x, pos.y + y, pos.z + z), (byte)VoxelData.BlockTypes.Cobble));
                    }
                }

                if(x == 1 && z == 1 || x == 1 && z == 2 || x == 1 && z == 3 || x == 2 && z == 1 || x == 2 && z == 2 || x == 2 && z == 3 || x == 3 && z == 1 || x == 3 && z == 2 || x == 3 && z == 3)
                {
                    queue.Enqueue(new VoxelMod(new Vector3(pos.x + x, pos.y + 5, pos.z + z), (byte)VoxelData.BlockTypes.Planks)); //ceiling
                }
            }
        }

        return queue;
    }
}
