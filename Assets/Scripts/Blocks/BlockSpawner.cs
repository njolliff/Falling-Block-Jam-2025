using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
    // PUBLIC
    public float spawnInterval;
    [Range(0, 100)] public float hazardousBlockChance;
    public Vector2 blockFallSpeedRange = new(1, 1);

    // PRIVATE
    private GameObject[] _blocks;
    private GameObject[] _hazardousBlocks;
    private float spawnTimer = 0;

    void Awake()
    {
        _blocks = Resources.LoadAll<GameObject>("Prefabs/Blocks");
        _hazardousBlocks = Resources.LoadAll<GameObject>("Prefabs/Hazardous Blocks");
    }

    // Update is called once per frame
    void Update()
    {
        CheckSpawn();
    }

    private void CheckSpawn()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            SpawnBlock();
            spawnTimer = 0;
        }
    }
    private void SpawnBlock()
    {
        // Determine if spawning a normal or hazardous block
        float randVal = Random.Range(0, 100);

        // Pick a random spawn point and fall speed
        Vector2 randomSpawnPoint = Camera.main.ViewportToWorldPoint(new Vector3(Random.Range(0.1f, 0.9f), 1f, Camera.main.nearClipPlane)) + Vector3.up * 5; // Spawn blocks on a random point on the x axis 5 units above screen height
        float randomFallSpeed = Random.Range(blockFallSpeedRange.x, blockFallSpeedRange.y);

        // Normal
        if (randVal >= hazardousBlockChance)
        {
            if (_blocks == null || _blocks.Length < 1) return; // Do nothing if blocks container is null or empty

            // Pick a random normal block
            GameObject randomBlock = _blocks[Random.Range(0, _blocks.Length)];

            // Instantiate the random block and set its fall speed to the random fall speed
            GameObject block = Instantiate(randomBlock, randomSpawnPoint, Quaternion.identity);
            block.GetComponent<FallingBlock>().fallSpeed = randomFallSpeed;
        }

        // Hazardous
        else
        {
            if (_hazardousBlocks == null || _hazardousBlocks.Length < 1) return; // Do nothing if hazardous blocks container is null or empty

            // Pick a random hazardous block
            GameObject randomBlock = _hazardousBlocks[Random.Range(0, _hazardousBlocks.Length)];

            // Instantiate the random block and set its fall speed to the random fall speed
            GameObject block = Instantiate(randomBlock, randomSpawnPoint, Quaternion.identity);
            block.GetComponent<FallingBlock>().fallSpeed = randomFallSpeed;
        }
    }
}