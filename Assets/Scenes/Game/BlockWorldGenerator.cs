using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace PerlinNoiseGenerator
{
    public class DynamicBlockWorldGenerator : MonoBehaviour
    {
        [SerializeField] private NoiseSettings noiseSettings;  // Configuracoes de ruido
        [SerializeField] private NoiseSettings noiseTree;      // Configuracoes de ruido das arve
        [SerializeField] private NoiseSettings noiseCave;      // Configuracoes de ruido das cavernas
        [SerializeField] private Vector2 sampleCentre;         // Centro inicial do noise
        [SerializeField] private int chunkSize = 8;            // Tamanho do chunk (8x8 blocos visiveis)
        [SerializeField] private int maxTerrainHeight = 20;    // Altura maxima do terreno
        [SerializeField] private int seaLevel = 5;

        [SerializeField] private GameObject dirtBlock;         // Prefab do bloco de terra
        [SerializeField] private GameObject grassBlock;        // Prefab do bloco de grama
        [SerializeField] private GameObject stoneBlock;        // Prefab do bloco de pedra
        [SerializeField] private GameObject waterBlock;
        [SerializeField] private GameObject sandBlock;
        [SerializeField] private GameObject tree;
        [SerializeField] private float treeThreshold = .7f;
        [SerializeField] private GameObject cave;
        [SerializeField] private float caveThreshold = .7f;

        [SerializeField] private Transform player;             // Referencia ao jogador
        [SerializeField] private int blocksGeneratedByFrame;   // Qtdd de blocos gerados a cada frame para evitar gargalos

        private Dictionary<Vector2Int, GameObject> activeChunks = new Dictionary<Vector2Int, GameObject>();
        private Vector2Int currentChunk;     // Chunk em que o player esta
        private bool isTerrain = false;

        void Start()
        {
            UpdateVisibleChunks();
        }

        void Update()
        {
            Vector2Int newChunk = GetPlayerChunk();
            Debug.Log("[" + newChunk.x + ", " + newChunk.y + "]");
            // Apenas atualiza os chunks se o jogador mudou de chunk
            if (newChunk != currentChunk)
            {
                currentChunk = newChunk;
                UpdateVisibleChunks();
            }
        }

        Vector2Int GetPlayerChunk()
        {
            // Calcula o chunk em que o jogador esta com base na posicao
            int chunkX = Mathf.FloorToInt(player.position.x / chunkSize);
            int chunkZ = Mathf.FloorToInt(player.position.z / chunkSize);
            return new Vector2Int(chunkX, chunkZ);
        }

        void UpdateVisibleChunks()
        {
            // Calcula os chunks visiveis ao redor do player
            HashSet<Vector2Int> newVisibleChunks = new HashSet<Vector2Int>();

            for (int xOffset = -1; xOffset <= 1; xOffset++)
            {
                for (int zOffset = 0; zOffset <= 2; zOffset++)
                {
                    Vector2Int chunkCoord = currentChunk + new Vector2Int(xOffset, zOffset);
                    newVisibleChunks.Add(chunkCoord);

                    if (!activeChunks.ContainsKey(chunkCoord))
                    {
                        StartCoroutine(GenerateChunks(chunkCoord));
                        //GenerateChunk(chunkCoord);
                    }
                }
            }

            // Desativa chunks que nao estao mais visiveis
            List<Vector2Int> chunksToRemove = new List<Vector2Int>();
            foreach (var chunk in activeChunks.Keys)
            {
                if (!newVisibleChunks.Contains(chunk))
                {
                    //activeChunks[chunk].SetActive(false);
                    Destroy(activeChunks[chunk]);
                    chunksToRemove.Add(chunk);
                }
            }

            foreach (var chunk in chunksToRemove)
            {
                activeChunks.Remove(chunk);
            }
        }

        IEnumerator GenerateChunks(Vector2Int chunkCoord) //[2, 2]
        {
            GameObject chunkParent = new GameObject($"Chunk_{chunkCoord.x}_{chunkCoord.y}");
            chunkParent.transform.parent = transform;

            Vector2Int chunkOrigin = chunkCoord * chunkSize;
            float[,] noiseMap = PerlinNoiseGenerator.Noise.GenerateNoiseMap(chunkSize, chunkSize, noiseSettings, chunkOrigin);
            float[,] noiseTreeMap = PerlinNoiseGenerator.Noise.GenerateNoiseMap(chunkSize, chunkSize, noiseTree, chunkOrigin);
            float[,] noiseCaveMap = PerlinNoiseGenerator.Noise.GenerateNoiseMap(chunkSize, chunkSize, noiseCave, chunkOrigin);

            System.Random rand = new System.Random(chunkCoord.GetHashCode());

            for (int i = 0; i < chunkSize * chunkSize; i++)
            {
                int x = i % chunkSize;
                int z = i / chunkSize;

                // Calcula a posicao global do bloco
                int worldX = chunkOrigin.x + x;
                int worldZ = chunkOrigin.y - z;

                //terrain
                float height = noiseMap[x, z] * maxTerrainHeight;
                int terrainHeight = Mathf.FloorToInt(height);

                //tree
                float treeChance = Mathf.Round(noiseTreeMap[x, z] * noiseMap[x, z] * 10f) / 10f;

                //cave
                float caveChance = Mathf.Round(noiseCaveMap[x, z] * (1-noiseMap[x, z]) * 10f) / 10f;

                GenerateVerticalBlocks(chunkParent.transform, worldX, worldZ, terrainHeight, seaLevel, treeChance, caveChance);

                if (i % blocksGeneratedByFrame == 0) // A cada X blocos gerados, espera um frame
                {
                    yield return null;
                }
            }

            activeChunks.Add(chunkCoord, chunkParent);
            isTerrain = true;
        }

        void GenerateVerticalBlocks(Transform parent, int worldX, int worldZ, int terrainHeight, int seaLevel, float treeChance, float caveChance)
        {
            for (int y = 0; y <= Mathf.Max(terrainHeight, seaLevel); y++)
            {
                GameObject blockToSpawn;

                // Define o tipo de bloco
                if(y > terrainHeight && y <= seaLevel)
                {
                    blockToSpawn = waterBlock; // Bloco de agua
                }
                else if (y == terrainHeight)
                {
                    if(y >= seaLevel)
                        blockToSpawn = grassBlock;
                    else
                        blockToSpawn = sandBlock;
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
                if(!(blockToSpawn == waterBlock && y < seaLevel)) //NAO RENDERIZA BLOCOS DE AGUA DENTRO DAGUA
                    Instantiate(blockToSpawn, blockPosition, Quaternion.identity, parent);

                if (y > seaLevel)
                {
                    if (treeChance >= treeThreshold && (worldX % 5 == 0 || worldX % 11 == 0) && (worldZ % 5 == 0 || worldZ % 11 == 0))
                    {
                        Vector3 treePosition = new Vector3(worldX, terrainHeight, worldZ);
                        Instantiate(tree, treePosition, Quaternion.identity, parent);
                    }

                    if (caveChance >= caveThreshold && (worldX % 25 == 0 && worldZ % 25 == 0) && terrainHeight > 1)
                    {
                        Vector3 cavePosition = new Vector3(worldX, terrainHeight, worldZ);
                        Instantiate(cave, cavePosition, Quaternion.identity, parent);
                    }
                }
            }
        }

        public bool isTerrainGenerated() //checa se existe terreno abaixo do player
        {
            print("Terrain under player: "+isTerrain);
            return isTerrain;
        }

        public bool isUnderWater()
        {
            return player.localPosition.y < -2;
        }
    }
}
