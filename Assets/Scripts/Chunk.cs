using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class Chunk : MonoBehaviour
{
    [System.NonSerialized]
    public int chunkSize = 16;
    public int chunkX = 0, chunkY = 0, chunkZ = 0;

    private MeshCollider meshCollider;
    private MeshFilter meshFilter;
    private Mesh mesh;
    void Start()
    {
        Init();

        Thread thread = new Thread(() =>
        {
            MeshData meshData = CalculateDensities();
            AsyncCallback.Instance.Invoke(() => UpdateMesh(meshData));
        })
        {
            IsBackground = true,
            Priority = System.Threading.ThreadPriority.BelowNormal,
            Name = string.Format("Chunk_{0}_{1}_{2}_Init", chunkX, chunkY, chunkZ)
        };
        thread.Start();
    }

    public void Init()
    {
        if (!mesh)
        {
            mesh = new Mesh();
            mesh.name = "ChunkMesh";
        }

        meshCollider = GetComponent<MeshCollider>();
        meshFilter = GetComponent<MeshFilter>();

        //Target is the value that represents the surface of mesh
        //For example the perlin noise has a range of -1 to 1 so the mid point is were we want the surface to cut through
        //The target value does not have to be the mid point it can be any value with in the range
        MarchingCubes.SetTarget(0.0f);
        MarchingCubes.SetWindingOrder(2, 1, 0);
        if (World.Instance.marchingTechahedral)
            MarchingCubes.SetModeToTetrahedrons();
    }

    float frames = 20;

    private float[,,] Voxels;

    public MeshData CalculateDensities()
    {
        //The size of voxel array. Be carefull not to make it to large as a mesh in unity can only be made up of 65000 verts
        int Width = chunkSize;
        int Height = chunkSize;
        int Depth = chunkSize;

        Voxels = new float[Width, Height, Depth];

        //Fill voxels with values. I'm using perlin noise but any method to create voxels will work
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int z = 0; z < Depth; z++)
                {
                    Voxels[x, y, z] = World.Instance.Density(x + chunkX, y + chunkY, z + chunkZ);
                }
            }
        }

        MeshData meshData = MarchingCubes.BuildMesh(Voxels);

        return meshData;
    }

    public void UpdateMesh(MeshData Def)
    {
        if (Def == null) return;
        mesh.vertices = Def.Vertices;
        mesh.triangles = Def.Triangles;
        //The diffuse shader wants uvs so just fill with a empty array, they're not actually used
        mesh.uv = new Vector2[mesh.vertices.Length];
        ;
        mesh.RecalculateNormals();
        transform.localPosition = new Vector3(chunkX, chunkY, chunkZ);
        meshFilter.mesh = mesh;

        UnityEngine.Profiling.Profiler.EndSample();

        meshCollider.sharedMesh = mesh;
    }

}