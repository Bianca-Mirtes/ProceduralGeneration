using UnityEngine;
using UnityEditor;

public class Texture2DArrayCreator : MonoBehaviour
{
    [MenuItem("Tools/Create Texture2DArray")]
    static void CreateTexture2DArray()
    {
        // Caminho das texturas que você quer usar
        string[] texturePaths = new string[] {
            "Assets/Textures/Grass.png",
            "Assets/Textures/Rocks 1.png",
            "Assets/Textures/Rocks 2.png",
            "Assets/Textures/Sandy grass.png",
            "Assets/Textures/Snow.png",
            "Assets/Textures/Stony ground.png",
            "Assets/Textures/Water.png"
        };

        // Carregar as texturas
        Texture2D[] textures = new Texture2D[texturePaths.Length];
        for (int i = 0; i < texturePaths.Length; i++)
        {
            textures[i] = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePaths[i]);
            if (textures[i] == null)
            {
                Debug.LogError($"Erro ao carregar textura: {texturePaths[i]}");
                return;
            }
        }

        // Verificar se todas as texturas têm o mesmo tamanho e formato
        int width = textures[0].width;
        int height = textures[0].height;
        TextureFormat format = textures[0].format;
        foreach (var tex in textures)
        {
            if (tex.width != width || tex.height != height || tex.format != format)
            {
                Debug.LogError("Todas as texturas precisam ter o mesmo tamanho e formato!");
                return;
            }
        }

        // Criar o Texture2DArray
        Texture2DArray textureArray = new Texture2DArray(width, height, textures.Length, format, false);

        // Copiar os dados de cada textura para o Texture2DArray
        for (int i = 0; i < textures.Length; i++)
        {
            for (int mip = 0; mip < textures[i].mipmapCount; mip++)
            {
                Graphics.CopyTexture(textures[i], 0, mip, textureArray, i, mip);
            }
        }

        // Salvar como um asset no projeto
        string assetPath = "Assets/Textures/MyTexture2DArray.asset";
        AssetDatabase.CreateAsset(textureArray, assetPath);
        AssetDatabase.SaveAssets();

        Debug.Log($"Texture2DArray criado e salvo em {assetPath}");
    }
}