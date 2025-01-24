using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace PerlinNoiseGenerator
{
    public class DynamicBlockWorldGenerator : MonoBehaviour
    {
        [Header("Noises")]
        [SerializeField] private NoiseSettings noiseSettings;  // Configuracoes de ruido
        [SerializeField] private NoiseSettings noiseTree;      // Configuracoes de ruido das arve
        [SerializeField] private NoiseSettings noiseCaveEntries;      // Configuracoes de ruido das cavernas
        [SerializeField] private NoiseSettings noiseCave;
        [SerializeField] private NoiseSettings noiseOres;
        [SerializeField] private NoiseSettings noiseCloud;

        [SerializeField] private Vector2 sampleCentre;         // Centro inicial do noise
        [SerializeField] private int chunkSize = 8;            // Tamanho do chunk (8x8 blocos visiveis)
        [SerializeField] private int maxTerrainHeight = 20;    // Altura maxima do terreno
        [SerializeField] private int seaLevel = 5;
        [SerializeField] private int maxCaveHeight = 11;
        //[SerializeField] private int maxTerrainHeightCave

        [Header("Blocks")]
        [SerializeField] private GameObject dirtBlock;         // Prefab do bloco de terra
        [SerializeField] private GameObject grassBlock;        // Prefab do bloco de grama
        [SerializeField] private GameObject stoneBlock;        // Prefab do bloco de pedra
        [SerializeField] private GameObject waterBlock;
        [SerializeField] private GameObject sandBlock;
        [SerializeField] private GameObject tree;
        [SerializeField] private GameObject caveEntry;
        [SerializeField] private GameObject cloud;
        [SerializeField] private GameObject ores;

        [Header("Variables")]
        [SerializeField] private float treeThreshold = .7f;
        [SerializeField] private float caveThreshold = .7f;
        [SerializeField] private float cloudThreshold = .75f;

        [SerializeField] private Transform player;             // Referencia ao jogador
        [SerializeField] private int blocksGeneratedByFrame;   // Qtdd de blocos gerados a cada frame para evitar gargalos
        [SerializeField] private int cloudHeigh = 20;

        [SerializeField] private bool inCave = false, closeToCave = false;
        //private bool oldCloseToCave = false;

        private Dictionary<Vector2Int, GameObject> activeChunks = new Dictionary<Vector2Int, GameObject>();
        private Vector2Int currentChunk;     // Chunk em que o player esta
        private bool isTerrain = false;

        [SerializeField] private TittleController tittleController;
        [SerializeField] private LightingController lightingController;

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

            if(player.localPosition.y < -6)
            {
                inCave = true;
            }else
            {
                inCave = false;
            }
            lightingController.SetLighting(inCave);
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
                    chunksToRemove.Add(chunk);
                    Destroy(activeChunks[chunk]);
                }
            }

            foreach (var chunk in chunksToRemove)
            {
                activeChunks.Remove(chunk);
            }
        }

        IEnumerator GenerateChunks(Vector2Int chunkCoord)
        {
            GameObject chunkParent = new GameObject($"Chunk_{chunkCoord.x}_{chunkCoord.y}");
            chunkParent.transform.parent = transform;

            Vector2Int chunkOrigin = chunkCoord * chunkSize;
            float[,] noiseMap = PerlinNoiseGenerator.Noise.GenerateNoiseMap(chunkSize, chunkSize, noiseSettings, chunkOrigin);
            float[,] noiseCaveMap = PerlinNoiseGenerator.Noise.GenerateNoiseMap(chunkSize, chunkSize, noiseCave, chunkOrigin);
            float[,] noiseTreeMap = PerlinNoiseGenerator.Noise.GenerateNoiseMap(chunkSize, chunkSize, noiseTree, chunkOrigin);
            float[,] noiseCaveEntriesMap = PerlinNoiseGenerator.Noise.GenerateNoiseMap(chunkSize, chunkSize, noiseCaveEntries, chunkOrigin);
            float[,] noiseCloudMap = PerlinNoiseGenerator.Noise.GenerateNoiseMap(chunkSize, chunkSize, noiseCloud, chunkOrigin);
            float[,] noiseOresMap = PerlinNoiseGenerator.Noise.GenerateNoiseMap(chunkSize, chunkSize, noiseOres, chunkOrigin);

            for (int i = 0; i < chunkSize * chunkSize; i++)
            {
                int x = i % chunkSize;
                int z = i / chunkSize;

                // Calcula a posicao global do bloco
                int worldX = chunkOrigin.x + x;
                int worldZ = chunkOrigin.y - z;

                int heightColumn = Mathf.FloorToInt(noiseCaveMap[x, z] * maxCaveHeight);
 
                //terrain
                float height = noiseMap[x, z] * maxTerrainHeight;
                int terrainHeight = Mathf.FloorToInt(height);

                //trees
                float treeChance = noiseTreeMap[x, z];

                //caveEntries
                float caveChance = noiseCaveEntriesMap[x, z];

                GenerateCaveBlocks(heightColumn, worldX, worldZ, chunkParent.transform, stoneBlock);
                if (noiseOresMap[x, z] > .8f)
                {
                    GenerateCaveBlocks(heightColumn + 1, worldX, worldZ, chunkParent.transform, ores);
                }

                if (i % (blocksGeneratedByFrame) == 0) // A cada X blocos gerados, espera um frame
                    yield return null;

                if (closeToCave)
                    continue;

                GenerateVerticalBlocks(chunkParent.transform, worldX, worldZ, terrainHeight, seaLevel, treeChance, caveChance);

                float hasCloud = noiseCloudMap[x, z];
                if (hasCloud > cloudThreshold)
                {
                    Vector3 cloudPosition = new Vector3(worldX, cloudHeigh, worldZ);
                    Instantiate(cloud, cloudPosition, Quaternion.identity, chunkParent.transform);
                }
                    
                if (i % blocksGeneratedByFrame == 0) // A cada X blocos gerados, espera um frame
                    yield return null;

            }
            closeToCave = false;

            activeChunks.Add(chunkCoord, chunkParent);
            if (!isTerrain)
                tittleController.fadeOutTitle();
            isTerrain = true;
        }

        void GenerateVerticalBlocks(Transform parent, int worldX, int worldZ, int terrainHeight, int seaLevel, float treeChance, float caveChance)
        {
            for (int y = 0; y <= Mathf.Max(terrainHeight, seaLevel); y++)
            {
                bool hasCave = renderElements(treeChance, caveChance, terrainHeight, y, worldX, worldZ, parent);

                if (!hasCave)
                    renderBlocks(terrainHeight, y, worldX, worldZ, parent);
                
            }
        }

        private bool renderElements(float treeChance, float caveChance, int terrainHeight, int y, int worldX, int worldZ, Transform parent)
        {
            if (y > seaLevel)
            {
                if (caveChance >= caveThreshold && terrainHeight == y)
                {
                    Vector3 cavePosition = new Vector3(worldX, terrainHeight, worldZ);
                    Instantiate(caveEntry, cavePosition, Quaternion.identity, parent);
                    return true;
                }

                if (treeChance >= treeThreshold && terrainHeight == y)
                {
                    Vector3 treePosition = new Vector3(worldX, terrainHeight, worldZ);
                    Instantiate(tree, treePosition, Quaternion.identity, parent);
                }
            }
            return false;
        }

        private void renderBlocks(int terrainHeight, int y, int worldX, int worldZ, Transform parent)
        {
            GameObject blockToSpawn;
            //bool renderBlock = true;

            // Define o tipo de bloco
            if (y > terrainHeight && y <= seaLevel)
            {
                blockToSpawn = waterBlock;
            }
            else if (y == terrainHeight)
            {
                if (y >= seaLevel)
                    blockToSpawn = grassBlock;
                else
                    blockToSpawn = sandBlock;
            }
            else
            {
                return;
            }

            Vector3 blockPosition = new Vector3(worldX, y, worldZ);
            if (!(blockToSpawn == waterBlock && y < seaLevel)) //NAO RENDERIZA BLOCOS DE AGUA DENTRO DAGUA
                Instantiate(blockToSpawn, blockPosition, Quaternion.identity, parent);
        }

        private void GenerateCaveBlocks(int heightColumn, int worldX, int worldZ, Transform parent, GameObject blockToSpawn)
        {
            Vector3 blockPosition = new Vector3(worldX, heightColumn - maxCaveHeight, worldZ);
            Instantiate(blockToSpawn, blockPosition, Quaternion.identity, parent);
        }

        public bool isTerrainGenerated()
        {
            print("Terrain under player: "+isTerrain);
            return isTerrain;
        }

        public bool isUnderWater()
        {
            return player.localPosition.y < -2 && player.localPosition.y > -6 && !inCave;
        }
        
        public void SetIsCloseToCave(bool closeToCave)
        {
            this.closeToCave = closeToCave;
        }
    }
}
