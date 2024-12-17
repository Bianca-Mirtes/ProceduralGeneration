using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class PerlinNoise : MonoBehaviour
{
    [Header("Attributes")]
    public int width = 256;
    public int height = 256;
    public float scale = 20f;
    public float offsetX = 100f;
    public float offsetY = 100f;

    // Start is called before the first frame update
    void Start()
    {
        offsetX = Random.Range(0f, 9999f);
        offsetY = Random.Range(0f, 9999f);
    }
    void Update()
    {
        Renderer renderer = GetComponent<Renderer>();
        renderer.material.mainTexture = GenerateTexture();
    }

    Texture2D GenerateTexture()
    {
        Texture2D texture = new Texture2D(width, height);
        for (int ii = 0; ii < width; ii++)
            for (int jj = 0; jj < height; jj++)
            {
                UnityEngine.Color color = CalculateColor(ii, jj);
                texture.SetPixel(ii, jj, color);
            }

        texture.Apply();
        return texture;
    }

    UnityEngine.Color CalculateColor(int x, int y)
    {
        float xCoord = (float)x/width * scale +offsetX;
        float yCoord = (float)y/height * scale+ offsetY;

        float sample = Mathf.PerlinNoise(xCoord, yCoord);
        return new UnityEngine.Color(sample, sample, sample);
    }
}
