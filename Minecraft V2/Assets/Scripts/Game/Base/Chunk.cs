using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public ChunkCord coord;

    GameObject chunkObject;
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;

    int VertexIndex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> transparentTriangles = new List<int>();
    List<Color> colors = new List<Color>();
    List<Vector3> normals = new List<Vector3>();

    Material[] materials = new Material[2];
    public VoxelState[,,] voxelMap = new VoxelState[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];
    public Queue<VoxelMod> modifications = new Queue<VoxelMod>();
    World world;

    public Vector3 Position;

    private bool _isActive;
    private bool isVoxelMapPopulated = false;

    public Chunk(ChunkCord _coord, World _world)
    {
        coord = _coord;
        world = _world;
        IsActive = true;
    }

    public void Init()
    {
        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        materials[0] = world.material; //standard
        materials[1] = world.transparentMaterial;//transparent
        meshRenderer.materials = materials;

        chunkObject.transform.SetParent(world.transform);
        chunkObject.transform.position = new Vector3(coord.x * VoxelData.ChunkWidth, 0f, coord.z * VoxelData.ChunkWidth);
        chunkObject.name = "Chunk" + coord.x + "," + coord.z;
        Position = chunkObject.transform.position;

        PopulateVoxelMap();
    }

    public  void UpdateChunk()
    {
        while(modifications.Count > 0)
        {
            VoxelMod v = modifications.Dequeue(); //removes last voxelmod form queue
            Vector3 pos = v.position -= Position;
            voxelMap[(int)pos.x, (int)pos.y, (int)pos.z].id = v.ID;
        }

        ClearMeshdata();
        CalculateLight();

        for (int y = 0; y < VoxelData.ChunkHeight; y++) //Height loop 
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++) //Square loop x 
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++) //Square loop y
                {
                    if(world.blocktypes[voxelMap[x,y,z].id].IsSolid)
                        UpdateMeshdata(new Vector3(x, y, z));
                }
            }
        }

        world.ChunksToDraw.Enqueue(this);
    }

    void CalculateLight()
    {
        Queue<Vector3Int> litVoxels = new Queue<Vector3Int>();

        for (int x = 0; x < VoxelData.ChunkWidth; x++) //Square loop x 
        {
            for (int z = 0; z < VoxelData.ChunkWidth; z++) //Square loop y
            {
                float lightRay = 1f;
                for(int y = VoxelData.ChunkHeight - 1; y >= 0; y--) //Lopp down(from top to bootom)
                {
                    VoxelState thisvoxel = voxelMap[x, y, z];

                    if(thisvoxel.id > 0)
                    {
                        lightRay *= world.blocktypes[thisvoxel.id].transparency;
                    }
                    thisvoxel.globalLightPercent = lightRay;
                    voxelMap[x, y, z] = thisvoxel;

                    if (lightRay > VoxelData.lightFallOff)
                        litVoxels.Enqueue(new Vector3Int(x,y,z));
                }
            }
        }

        while (litVoxels.Count > 0)
        { 
            Vector3Int v = litVoxels.Dequeue(); //Removes top one
            for (int p = 0; p < 6; p++)
            {
                Vector3 currentVoxel = v + VoxelData.faceCheks[p];
                Vector3Int neighbour = new Vector3Int((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z);

                if(IsVoxelInChunk(neighbour.x, neighbour.y, neighbour.z))
                {
                    if(voxelMap[neighbour.x, neighbour.y, neighbour.z].globalLightPercent < voxelMap[v.x, v.y, v.z].globalLightPercent - VoxelData.lightFallOff)
                    {
                        voxelMap[neighbour.x, neighbour.y, neighbour.z].globalLightPercent = voxelMap[v.x, v.y, v.z].globalLightPercent - VoxelData.lightFallOff;
                        if (voxelMap[neighbour.x, neighbour.y, neighbour.z].globalLightPercent > VoxelData.lightFallOff)
                            litVoxels.Enqueue(neighbour);
                    }
                }
            }
        }
    }

    void ClearMeshdata()
    {
        VertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        colors.Clear();
        transparentTriangles.Clear();
        uvs.Clear();
        normals.Clear();
    }

    public bool IsActive
    {
        get { return _isActive; }
        set {
            _isActive = value;
            if (chunkObject != null)
                chunkObject.SetActive(value);
        }
    }

    public bool isEditable
    {
        get
        {
            if (!isVoxelMapPopulated)
                return false;
            else
                return true;
        }
    }

    bool IsVoxelInChunk(int x, int y, int z)
    {
        if (x < 0 || x > VoxelData.ChunkWidth - 1 || y < 0 || y > VoxelData.ChunkHeight - 1 || z < 0 || z > VoxelData.ChunkWidth - 1) //If is out of index in Array return false
            return false;
        else
            return true;
    }

    public void EditVoxel(Vector3 pos, byte newId)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z); 

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        voxelMap[xCheck, yCheck, zCheck].id = newId;

        lock (world.ChunkUpdateThreadLock)
        {
            world.chunksToUpdate.Insert(0, this); //Put this chunk at the top of the list
            UpdateSurroundingChunks(xCheck, yCheck, zCheck);
        }
    }

    void UpdateSurroundingChunks(int x, int y, int z)
    {
        Vector3 thisVoxel = new Vector3(x, y, z);

        for(int f = 0; f < 6; f++)
        {
            Vector3 currentVoxel = thisVoxel + VoxelData.faceCheks[f];

            if(!IsVoxelInChunk((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z))
            {
                world.chunksToUpdate.Insert(0, world.getChunkFromvector3(thisVoxel + Position + VoxelData.faceCheks[f]));
            }
        }
    }

    VoxelState CheckVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z); //Round down

        if (!IsVoxelInChunk(x,y,z)) 
            return world.GetVoxelState(pos + Position);

        return voxelMap[x,y,z];
    }

    public VoxelState GetVoxelFromGlobalVector3(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z); //Round down

        xCheck -= Mathf.FloorToInt(Position.x);
        zCheck -= Mathf.FloorToInt(Position.z);

        return voxelMap[xCheck, yCheck, zCheck];
    }

    void PopulateVoxelMap()
    {
        for (int y = 0; y < VoxelData.ChunkHeight; y++) //Height loop 
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++) //Square loop x 
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++) //Square loop y
                {
                    voxelMap[x, y, z] = new VoxelState(world.GetVoxel(new Vector3(x, y, z) + Position));
                }
            }
        }

        isVoxelMapPopulated = true;

        lock (world.ChunkUpdateThreadLock)
        {
            world.chunksToUpdate.Add(this);
        }

        if(world.settings.enableChunkLoadAnimation)
            chunkObject.AddComponent<ChunkLoadAnimation>();
    }

    void UpdateMeshdata(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z); //Round down
        byte blockID = voxelMap[xCheck, yCheck, zCheck].id;
        //bool isTransparent = world.blocktypes[blockID].renderNeighbourFaces;

        for (int f = 0; f < 6; f++) //face index
        {
            VoxelState neighbour = CheckVoxel(pos + VoxelData.faceCheks[f]);

            if (neighbour != null && world.blocktypes[neighbour.id].renderNeighbourFaces) //Check if there isnt any face next to
            {
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[f, 0]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[f, 1]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[f, 2]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[f, 3]]);

                for (int i = 0; i < 4; i++)
                    normals.Add(VoxelData.faceCheks[f]);

                AddTexture(world.blocktypes[blockID].GetTextureID(f));

                float lightlevel = neighbour.globalLightPercent;

                colors.Add(new Color(0, 0, 0, lightlevel));
                colors.Add(new Color(0, 0, 0, lightlevel));
                colors.Add(new Color(0, 0, 0, lightlevel));
                colors.Add(new Color(0, 0, 0, lightlevel));

                if (!world.blocktypes[neighbour.id].renderNeighbourFaces)
                {
                    triangles.Add(VertexIndex);
                    triangles.Add(VertexIndex + 1);
                    triangles.Add(VertexIndex + 2);
                    triangles.Add(VertexIndex + 2);
                    triangles.Add(VertexIndex + 1);
                    triangles.Add(VertexIndex + 3);
                }
                else
                {
                    transparentTriangles.Add(VertexIndex);
                    transparentTriangles.Add(VertexIndex + 1);
                    transparentTriangles.Add(VertexIndex + 2);
                    transparentTriangles.Add(VertexIndex + 2);
                    transparentTriangles.Add(VertexIndex + 1);
                    transparentTriangles.Add(VertexIndex + 3);
                }
                VertexIndex += 4;
            }
        }
    }

    void AddTexture(int TextureID)
    {
        float y = TextureID / VoxelData.TextureAtlasSizeInBlocks;
        float x = TextureID - (y * VoxelData.TextureAtlasSizeInBlocks);

        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;

        y = 1f - y - VoxelData.NormalizedBlockTextureSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));
    }

    public void CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();

        mesh.subMeshCount = 2;
        mesh.SetTriangles(triangles.ToArray(), 0);
        mesh.SetTriangles(transparentTriangles.ToArray(), 1);
        //mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.colors = colors.ToArray();
        mesh.normals = normals.ToArray();
        //mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }
}

public class ChunkCord //Position of chunk in chunk map. Not the relative pos in world
{
    public int x;
    public int z;

    public ChunkCord()
    {
        x = 0; z = 0;
    }

    public ChunkCord(int _x, int _z)
    {
        x = _x;
        z = _z;
    }

    public ChunkCord(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int zCheck = Mathf.FloorToInt(pos.z);

        x = xCheck / VoxelData.ChunkWidth;
        z = zCheck / VoxelData.ChunkWidth;
    }

    public bool Equals(ChunkCord other)
    {
        if (other == null)
            return false;
        else if (other.x == x && other.z == z)
            return true;
        else
            return false;
    }
}

public class VoxelState
{
    public byte id;
    public float globalLightPercent;
    public VoxelData.Biomes Biome;

    public VoxelState()
    {
        id = 0;
        globalLightPercent = 0f; //Dark block
        Biome = VoxelData.Biomes.GrassLands;
    }

    public VoxelState(byte _id)
    {
        id = _id;
        globalLightPercent = 0f; //Dark block
    }

    public VoxelState(byte _id, VoxelData.Biomes _biome)
    {
        id = _id;
        globalLightPercent = 0f; //Dark block
        Biome = _biome;
    }
}
