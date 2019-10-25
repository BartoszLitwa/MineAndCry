using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

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
    Material[] materials = new Material[2];
    public byte[,,] voxelMap = new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];
    public Queue<VoxelMod> modifications = new Queue<VoxelMod>();
    World world;

    public Vector3 Position;

    private bool _isActive;
    private bool isVoxelMapPopulated = false;
    private bool threadLocked = false;

    public Chunk(ChunkCord _coord, World _world, bool generateOnLoad)
    {
        coord = _coord;
        world = _world;
        IsActive = true;

        if (generateOnLoad)
            Init();
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

        Thread myThread = new Thread(new ThreadStart(PopulateVoxelMap));
        myThread.Start();
    }

    public void UpdateChunk()
    {
        Thread mythread = new Thread(new ThreadStart(_updateChunk));
        mythread.Start(); //Start new thread
    }

    private void _updateChunk()
    {
        threadLocked = true;

        while(modifications.Count > 0)
        {
            VoxelMod v = modifications.Dequeue(); //removes last voxelmod form queue
            Vector3 pos = v.position -= Position;
            voxelMap[(int)pos.x, (int)pos.y, (int)pos.z] = v.ID;
        }
        ClearMeshdata();

        for (int y = 0; y < VoxelData.ChunkHeight; y++) //Height loop 
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++) //Square loop x 
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++) //Square loop y
                {
                    if(world.blocktypes[voxelMap[x,y,z]].IsSolid)
                        UpdateMeshdata(new Vector3(x, y, z));
                }
            }
        }

        lock (world.ChunksToDraw) //As soon it get there it locks it and unlock it after
        {
            world.ChunksToDraw.Enqueue(this);
        }

        threadLocked = false;
    }

    void ClearMeshdata()
    {
        VertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        transparentTriangles.Clear();
        uvs.Clear();
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
            if (!isVoxelMapPopulated || threadLocked)
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

        voxelMap[xCheck, yCheck, zCheck] = newId;

        UpdateSurroundingChunks(xCheck, yCheck, zCheck);

        _updateChunk();
    }

    void UpdateSurroundingChunks(int x, int y, int z)
    {
        Vector3 thisVoxel = new Vector3(x, y, z);

        for(int f = 0; f < 6; f++)
        {
            Vector3 currentVoxel = thisVoxel + VoxelData.faceCheks[f];

            if(!IsVoxelInChunk((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z))
            {
                world.getChunkFromvector3(thisVoxel + Position)._updateChunk();
            }
        }
    }

    bool CheckVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z); //Round down

        if (!IsVoxelInChunk(x,y,z)) 
            return world.CheckifVoxelTransparent(pos + Position);

        return world.blocktypes[voxelMap[x,y,z]].IsTransparent;
    }

    public byte GetVoxelFromGlobalVector3(Vector3 pos)
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
                    voxelMap[x, y, z] = world.GetVoxel(new Vector3(x, y, z) + Position);
                }
            }
        }

        _updateChunk();
        isVoxelMapPopulated = true;
    }

    void UpdateMeshdata(Vector3 pos)
    {
        byte blockID = voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];
        bool isTransparent = world.blocktypes[blockID].IsTransparent;

        for (int f = 0; f < 6; f++) //face index
        {
            if (CheckVoxel(pos + VoxelData.faceCheks[f])) //Check if there isnt any face next to
            {
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[f, 0]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[f, 1]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[f, 2]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[f, 3]]);

                AddTexture(world.blocktypes[blockID].GetTextureID(f));

                if (!isTransparent)
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

        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();

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
