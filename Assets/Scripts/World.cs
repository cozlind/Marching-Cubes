using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class World : MonoBehaviour
{
    public int seed;
    public bool marchingTechahedral;

    public int chunkSize = 16;
    public int chunksPerFrame = 500;
    public float sightDistance = 128;
    [System.NonSerialized]
    public int displayChunkCount;

    internal Dictionary<string, GameObject> chunkDict;
    public PerlinNoise noise;
    public GameObject chunkPfb;
    public GameObject focus;

    private static World instance;
    public static World Instance
    {
        get
        {
            if (!instance)
            {
                instance = new World();
            }
            return instance;
        }
        private set { instance = value; }
    }
    World()
    {
        Instance = this;
    }

    void Start()
    {
        Init();
    }


   
    private long FrameCount;
    void Update()
    {
        FrameCount ++;
        if (FrameCount % 10 == 0)
        {
            GenerateChunks();
        }
    }

    void CleanupWorld()
    {
        //Clean up any previously instantiated Chunks
        List<GameObject> destoryList = GameObject.FindGameObjectsWithTag("WorldChunk").ToList();
#if UNITY_EDITOR
        destoryList.ForEach(x => DestroyImmediate(x));
#else
		destoryList.ForEach(x => Destroy(x));
#endif
    }

    void Init(bool ForceChunksToInit = false)
    {
        if (noise == null)
        {
            System.Random rand = new System.Random();
            if (seed == 0)
                seed = (int)(rand.NextDouble() * int.MaxValue);
            noise = new PerlinNoise(seed);
        }
        chunkDict = new Dictionary<string, GameObject>();

        CleanupWorld();
        GenerateChunks(ForceChunksToInit);
    }

    void GenerateChunks(bool ForceChunksToInit = false)
    {
        int minX = Mathf.RoundToInt((focus.transform.position.x - sightDistance) / chunkSize);
        int maxX = Mathf.RoundToInt((focus.transform.position.x + sightDistance) / chunkSize);
        int minY = Math.Max(Mathf.RoundToInt((focus.transform.position.y - sightDistance)), -3);
        int maxY = Math.Min(Mathf.RoundToInt((focus.transform.position.y + sightDistance)), 3);
        int minZ = Mathf.RoundToInt((focus.transform.position.z - sightDistance) / chunkSize);
        int maxZ = Mathf.RoundToInt((focus.transform.position.z + sightDistance) / chunkSize);


        int maxPerFrame = chunksPerFrame;// ForceChunksToInit ? 1000 : 500;
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y < maxY; y++)
            {
                for (int z = minZ; z < maxZ; z++)
                {
                    string key = string.Format("{0},{1},{2}", x, y, z);
                    if (!chunkDict.ContainsKey(key))
                    {
                        maxPerFrame--;

                        Vector3 pos = new Vector3(x * chunkSize, y * chunkSize, z * chunkSize);
                        if (Vector3.Distance(pos, focus.transform.position) < sightDistance)
                        {
                            GameObject chunk = Instantiate(chunkPfb, pos, Quaternion.identity) as GameObject;

                            displayChunkCount++;
                            chunkDict.Add(key, chunk);

                            Chunk chunkScript = chunk.GetComponent<Chunk>();
                            chunkScript.chunkSize = chunkSize + 1;
                            chunkScript.chunkX = x * chunkSize;
                            chunkScript.chunkY = y * chunkSize;
                            chunkScript.chunkZ = z * chunkSize;
                            chunkScript.transform.parent = gameObject.transform;
                            chunkScript.gameObject.name = string.Format("Chunk_{0}_{1}_{2}", x, y, z);
                            chunkScript.tag = "WorldChunk";


                            if (ForceChunksToInit)
                            {
                                UnityEngine.Profiling.Profiler.BeginSample("Forced Init");
                                chunkScript.Init();
                                chunkScript.UpdateMesh(chunkScript.CalculateDensities());
                                UnityEngine.Profiling.Profiler.EndSample();
                            }
                            UnityEngine.Profiling.Profiler.EndSample();
                        }
                    }
                    if (maxPerFrame < 0) return;
                }
            }
        }
    }

    public float Density(float x, float y, float z)
    {
        z *= -1;
        float ret = y < 0 ? y * 1.5f : y;
        //ret += noise.FractalNoise2D(x, z, 1, 40f, 15f);
        //ret += noise.FractalNoise3D(x, y, z, 2, 20f, 20f);
        //ret += noise.FractalNoise3D(x, y, z, 3, 40f, 15f);
        //ret += noise.FractalNoise3D(x, y, z, 4, 60f, 15f);
        //ret += noise.FractalNoise3D(x, y, z, 5, 80f, 10f);
        ret += noise.FractalNoise3D(x, y, z, 6, 100f, 50f);

        //if (y < 0) {ret *= .5f;}

        var roadCP = Mathf.Abs(x + 25 * Mathf.Sin(z / 50));//Mathf.Pow(Mathf.Pow(x, 2) + Mathf.Pow(z, 2), .5f);
        if (roadCP < 18)
        {
            var target = y;// y > 0 ? 1 : 0;
            if (roadCP < 3)
            {
                ret = target;
            }
            else
            {
                ret = Mathf.Lerp(target, ret, Mathf.Cos((roadCP - 3) / 15 * Mathf.PI) / -2 + .5f);
            }
        }
        return ret;
    }
}
public class MeshData
{
    public int[] Triangles;
    public Vector3[] Vertices;
}
