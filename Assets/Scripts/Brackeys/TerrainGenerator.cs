using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    private int width = 256; //x-axis of the terrain
    private int height = 256; //z-axis

    public int depth = 20; //y-axis

    public float scale = 20f;

    public float offsetX = 100f;
    public float offsetY = 100f;

    private float lastScale;
    private float lastOffsetX;
    private float lastOffsetY;
    private float lastDepth;

    private void Start()
    {
        lastDepth = depth;
        lastScale = scale;
        offsetX = Random.Range(0f, 9999f);
        offsetY = Random.Range(0f, 9999f);
        lastOffsetX = offsetX;
        lastOffsetY = offsetY;
        UpdateTerrain();
    }

    private void Update()
    {
        if (lastScale != scale || lastDepth != depth || lastOffsetX != offsetX || lastOffsetY != offsetY)
        {
            UpdateTerrain();
            lastScale = scale;
            lastDepth = depth;
            lastOffsetX = offsetX;
            lastOffsetY = offsetY;
        }
    }

    void UpdateTerrain(){
        Terrain terrain = GetComponent<Terrain>();
        terrain.terrainData = GenerateTerrain(terrain.terrainData);
    }

    TerrainData GenerateTerrain (TerrainData terrainData)
    {
        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width, depth, height);

        terrainData.SetHeights(0, 0, GenerateHeights());
        return terrainData;
    }

    float[,] GenerateHeights()
    {
        float[,] heights = new float[width, height];
        for(int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                heights[x, y] = CalculateHeight(x, y);
            }
        }

        return heights;
    }

    float CalculateHeight (int x, int y)
    {
        float xCoord = (float)x / width * scale + offsetX;
        float yCoord = (float)y / height * scale + offsetY;

        return Mathf.PerlinNoise(xCoord, yCoord);
    }
}