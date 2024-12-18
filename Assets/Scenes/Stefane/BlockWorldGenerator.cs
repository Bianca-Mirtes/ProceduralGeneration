using UnityEngine;
using System.Collections.Generic;


namespace PerlinNoiseGenerator
{
    public class DynamicBlockWorldGenerator : MonoBehaviour
    {
        public NoiseSettings noiseSettings;  // Configura��es de ru�do
        public Vector2 sampleCentre;         // Centro inicial do noise
        public int chunkSize = 8;            // Tamanho do chunk (8x8 blocos vis�veis)
        public int maxTerrainHeight = 20;    // Altura m�xima do terreno

        public GameObject dirtBlock;         // Prefab do bloco de terra
        public GameObject grassBlock;        // Prefab do bloco de grama
        public GameObject stoneBlock;        // Prefab do bloco de pedra
        public GameObject waterBlock;

        public Transform player;             // Refer�ncia ao jogador

        private Dictionary<Vector2Int, GameObject> activeChunks = new Dictionary<Vector2Int, GameObject>();
        private Vector2Int currentChunk;     // Chunk em que o player est�

        void Start()
        {
            UpdateVisibleChunks();
        }

        void Update()
        {
 
            Vector2Int newChunk = GetPlayerChunk();
            Debug.Log(newChunk.x+" - "+newChunk.y);
            // Apenas atualiza os chunks se o jogador mudou de chunk
            if (newChunk != currentChunk)
            {
                currentChunk = newChunk;
                UpdateVisibleChunks();
            }
        }


        Vector2Int GetPlayerChunk()
        {
            // Calcula o chunk em que o jogador est� com base na posi��o
            int chunkX = Mathf.FloorToInt(player.position.x / chunkSize);
            int chunkZ = Mathf.FloorToInt(player.position.z / chunkSize);
            return new Vector2Int(chunkX, chunkZ);
        }

        void UpdateVisibleChunks()
        {
            // Calcula os chunks vis�veis ao redor do player
            HashSet<Vector2Int> newVisibleChunks = new HashSet<Vector2Int>();

            for (int xOffset = -1; xOffset <= 1; xOffset++)
            {
                for (int zOffset = -1; zOffset <= 1; zOffset++)
                {
                    Vector2Int chunkCoord = currentChunk + new Vector2Int(xOffset, zOffset);
                    newVisibleChunks.Add(chunkCoord);

                    if (!activeChunks.ContainsKey(chunkCoord))
                    {
                        GenerateChunk(chunkCoord);
                    }
                }
            }

            // Desativa chunks que n�o est�o mais vis�veis
            List<Vector2Int> chunksToRemove = new List<Vector2Int>();
            foreach (var chunk in activeChunks.Keys)
            {
                if (!newVisibleChunks.Contains(chunk))
                {
                    activeChunks[chunk].SetActive(false);
                    chunksToRemove.Add(chunk);
                }
            }

            foreach (var chunk in chunksToRemove)
            {
                activeChunks.Remove(chunk);
            }
        }

        


        void GenerateChunk(Vector2Int chunkCoord)
        {
            GameObject chunkParent = new GameObject($"Chunk_{chunkCoord.x}_{chunkCoord.y}");
            chunkParent.transform.parent = transform;

            Vector2Int chunkOrigin = chunkCoord * chunkSize;

            // Define o n�vel do mar
            int seaLevel = 5;

            // Gera o mapa de ru�do para o chunk
            float[,] noiseMap = PerlinNoiseGenerator.Noise.GenerateNoiseMap(chunkSize, chunkSize, noiseSettings, sampleCentre + chunkOrigin);

            for (int z = 0; z < chunkSize; z++)
            {
                for (int x = 0; x < chunkSize; x++)
                {
                    // Calcula a posi��o global do bloco
                    int worldX = chunkOrigin.x + x;
                    int worldZ = chunkOrigin.y + z;

                    float height = noiseMap[x, z] * maxTerrainHeight;
                    int terrainHeight = Mathf.FloorToInt(height);

                    // Instancia os blocos do terreno
                    for (int y = 0; y <= Mathf.Max(terrainHeight, seaLevel); y++)
                    {
                        GameObject blockToSpawn;

                        // Define o tipo de bloco
                        if (y > terrainHeight && y <= seaLevel)
                        {
                            blockToSpawn = waterBlock; // Bloco de �gua
                        }
                        else if (y == terrainHeight)
                        {
                            blockToSpawn = grassBlock;
                        }
                        else if (y > terrainHeight - 3)
                        {
                            blockToSpawn = dirtBlock;
                        }
                        else
                        {
                            blockToSpawn = stoneBlock;
                        }

                        Vector3 blockPosition = new Vector3(worldX, y, worldZ);
                        Instantiate(blockToSpawn, blockPosition, Quaternion.identity, chunkParent.transform);
                    }
                }
            }

            activeChunks.Add(chunkCoord, chunkParent);
        }

    }
}

