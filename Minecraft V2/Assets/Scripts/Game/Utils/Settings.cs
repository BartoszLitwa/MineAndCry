using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings
{
    [Header("Game Data")]
    public string version;

    [Header("Performance")]
    public int viewDistance;
    public bool enableThreading;
    public bool enableChunkLoadAnimation;

    [Header("World Generation")]
    public int seed;
    public int WorldSizeInChunks;

    [Header("Controls")]
    [Range(0.1f, 10f)]
    public float mouseSensitivity;
    [Range(30f, 90f)]
    public int PlayersFOV;
}
