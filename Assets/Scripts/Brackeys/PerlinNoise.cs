using UnityEngine;

public class PerlinNoise : MonoBehaviour
{
    private int width = 256;
    private int height = 256;

    public float scale = 20f;

    public float offsetX = 100f;
    public float offsetY = 100f;

    private float lastScale;
    private float lastOffsetX;
    private float lastOffsetY;

    void Start(){
        lastScale = scale;
        offsetX = Random.Range(0, 99999f);
        offsetY = Random.Range(0, 99999f);
        lastOffsetX = offsetX;
        lastOffsetY = offsetY;
        GenerateNoise();
    }

    void Update(){
        if (lastScale != scale || lastOffsetX != offsetX || lastOffsetY != offsetY)
        {
            GenerateNoise();
            lastScale = scale;
            lastOffsetX = offsetX;
            lastOffsetY = offsetY;
        }
    }

    void GenerateNoise(){
        Renderer renderer = GetComponent<Renderer>();
        renderer.material.mainTexture = GenerateTexture();
    }

    Texture2D GenerateTexture(){
        Texture2D texture = new Texture2D(width, height);
        for (int x = 0; x < width; x++){
            for (int y = 0; y < height; y++){
                Color color = CalculateColor(x,y); // Calcula a cor com base no resultado do perlin noise
                texture.SetPixel(x,y, color);
            }
        }

        texture.Apply();
        return texture;
    }

    Color CalculateColor(int x, int y){
        // Convertendo coordenadas de pixel para coordenadas de perlin
        float xCoord = (float)x/width * scale + offsetX;
        float yCoord = (float)y/height * scale + offsetY;

        float sample = Mathf.PerlinNoise(xCoord,yCoord);
        return new Color(sample, sample, sample);
    }
}
