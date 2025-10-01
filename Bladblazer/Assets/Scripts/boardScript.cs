using UnityEngine;

public class Board : MonoBehaviour
{
    public int width = 12;
    public int height = 12;
    public GameObject[] blockPrefabs;

    public GameObject[,] grid;

    public float timer = 0f;
    public bool timerActive = false;

    void Start()
    {
        grid = new GameObject[width, height];
        FillBoard();
    }

    private void Update()
    {

        if (timerActive)
        {
            Debug.Log($"Timer: {timer}");
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                timerActive = false;
                SpawnNewBlock();
            }
        }
    }

    void FillBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                SpawnBlock(x, y);
            }
        }
    }

    void SpawnBlock(int x, int y)
    {
        // Zorgt ervoor dat er niet dezelfde kleur blokjes naast elkaar komen te liggen
        int maxAttemps = 10;
        int prefabIndex = Random.Range(0, blockPrefabs.Length);

        for (int attempt = 0; attempt < maxAttemps; attempt++)
        {
            bool hasSameNeighbor = false;

            // Check het linker blokje
            if (x > 0 && grid[x - 1, y] != null)
            {
                if (grid[x - 1, y].name.Contains(blockPrefabs[prefabIndex].name))
                {
                    hasSameNeighbor = true;
                }
                    
            }

            // Check het blokje eronder
            if (y > 0 && grid[x, y - 1] != null)
            {
                if (grid[x, y - 1].name.Contains(blockPrefabs[prefabIndex].name))
                { 
                    hasSameNeighbor = true;
                }
            }

            if (!hasSameNeighbor)
            {
                break;
            }

            prefabIndex = Random.Range(0, blockPrefabs.Length);
        }

        Vector2 pos = new Vector2(x, y);
        GameObject blockObj = Instantiate(blockPrefabs[prefabIndex], pos, Quaternion.identity);

        Block b = blockObj.GetComponent<Block>();
        b.x = x;
        b.y = y;
        b.board = this; // belangrijk
        grid[x, y] = blockObj;
    }

    public int CountBlocksBelow(int startX, int startY, int dirX)
{
    int count = 0;
    int x = startX + dirX;
    int y = startY - 1;

    while (x >= 0 && x < width && y >= 0)
    {
        if (grid[x, y] != null) count++;
        y--;
        x += dirX;
    }

    return count;
}

    public void SpawnNewBlock()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid.GetValue(x, y) == null)
                {
                    SpawnBlock(x, y);
                    Debug.Log($"Spawned new block at {x},{y}");
                }
            }
        }


    }
}


