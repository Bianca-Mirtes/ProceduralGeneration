using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace PerlinNoiseGenerator
{
    public class DynamicBlockWorldGenerator : MonoBehaviour
    {
        [SerializeField] private NoiseSettings noiseSettings;  // Configuracoes de ruido
        [SerializeField] private Vector2 sampleCentre;         // Centro inicial do noise
        [SerializeField] private int chunkSize = 8;            // Tamanho do chunk (8x8 blocos visiveis)
        [SerializeField] private int maxTerrainHeight = 20;    // Altura maxima do terreno

        [SerializeField] private GameObject dirtBlock;         // Prefab do bloco de terra
        [SerializeField] private GameObject grassBlock;        // Prefab do bloco de grama
        [SerializeField] private GameObject stoneBlock;        // Prefab do bloco de pedra
        [SerializeField] private GameObject waterBlock;

        [SerializeField] private Transform player;             // Referencia ao jogador
        [SerializeField] private int blocksGeneratedByFrame;   // Qtdd de blocos gerados a cada frame para evitar gargalos

        private Dictionary<Vector2Int, GameObject> activeChunks = new Dictionary<Vector2Int, GameObject>();
        private Vector2Int currentChunk;     // Chunk em que o player esta
        private Vector3 originalGravity;

        void Start()
        {
            originalGravity = Physics.gravity;
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
            int seaLevel = 1;
            float[,] noiseMap = PerlinNoiseGenerator.Noise.GenerateNoiseMap(chunkSize, chunkSize, noiseSettings, chunkOrigin);
            //float[,] noiseMap = new float[3, 3];

            for (int i = 0; i < chunkSize * chunkSize; i++)
            {
                int x = i % chunkSize;
                int z = i / chunkSize;

                // Calcula a posicao global do bloco
                int worldX = chunkOrigin.x + x;
                int worldZ = chunkOrigin.y - z;

                float height = noiseMap[x, z] * maxTerrainHeight;
                int terrainHeight = Mathf.FloorToInt(height);

                GenerateVerticalBlocks(chunkParent.transform, worldX, worldZ, terrainHeight, seaLevel);

                if (i % blocksGeneratedByFrame == 0) // A cada X blocos gerados, espera um frame
                {
                    FindFirstObjectByType<PrefabGenerator>().GeneratePrefabs(noiseMap[x, z], x, z);
                    yield return null;
                }
            }

            activeChunks.Add(chunkCoord, chunkParent);
            //CombineChildrenAndAddCollider(chunkParent);
        }

        void GenerateVerticalBlocks(Transform parent, int worldX, int worldZ, int terrainHeight, int seaLevel)
        {
            for (int y = 0; y <= Mathf.Max(terrainHeight, seaLevel); y++)
            {
                GameObject blockToSpawn;

                // Define o tipo de bloco
                if (y > terrainHeight && y <= seaLevel)
                {
                    blockToSpawn = waterBlock; // Bloco de agua
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
                Instantiate(blockToSpawn, blockPosition, Quaternion.identity, parent);
            }
        }

        void CombineChildrenAndAddCollider(GameObject parent)
        {
            // Agrupar todos os filhos
            foreach (Transform child in parent.transform)
            {
                child.SetParent(null);  // Desvincula o filho do parent
            }

            // Adiciona um colisor ao GameObject pai
            BoxCollider collider = parent.AddComponent<BoxCollider>(); // Pode ser BoxCollider, SphereCollider, etc.

            // Calcula os bounds do objeto pai baseado nos filhos
            Bounds bounds = new Bounds(parent.transform.position, Vector3.zero);

            // Calcula a área de cobertura combinando as posições de todos os filhos
            foreach (Transform child in parent.transform)
            {
                bounds.Encapsulate(child.GetComponent<Renderer>().bounds);
            }

            // Ajusta o tamanho e a posição do colisor com base nos bounds calculados
            collider.center = bounds.center - parent.transform.position;
            collider.size = bounds.size;

            // Reativa o parent para os filhos ficarem dentro dele
            foreach (Transform child in parent.transform)
            {
                child.SetParent(parent.transform);  // Reconecta os filhos ao GameObject pai
            }
        }
    }
}
