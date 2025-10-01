using System.Collections.Generic;
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
        CheckMatches();
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

    public void CheckMatches()
    {
        bool[,] mark = new bool[width, height];

        // HORIZONTAAL: loop elke rij en zoek runs (aaneengesloten gelijke kleuren)
        for (int y = 0; y < height; y++)
        {
            int runStartX = 0;
            int runColor = -999;
            int runLength = 0;

            for (int x = 0; x < width; x++)
            {
                if (grid[x, y] == null)
                {
                    // vakje leeg => run afsluiten
                    if (runLength >= 3)
                    {
                        for (int k = runStartX; k < runStartX + runLength; k++) mark[k, y] = true;
                    }
                    runLength = 0;
                    runColor = -999;
                    runStartX = x + 1;
                    continue;
                }

                Block b = grid[x, y].GetComponent<Block>();
                int c = b.colorId;

                if (runLength == 0)
                {
                    // start nieuwe run
                    runStartX = x;
                    runLength = 1;
                    runColor = c;
                }
                else if (c == runColor)
                {
                    runLength++;
                }
                else
                {
                    // run afgesloten door andere kleur
                    if (runLength >= 3)
                    {
                        for (int k = runStartX; k < runStartX + runLength; k++) mark[k, y] = true;
                    }
                    // start nieuwe run
                    runStartX = x;
                    runLength = 1;
                    runColor = c;
                }
            }

            // aan einde van rij: check trailing run
            if (runLength >= 3)
            {
                for (int k = runStartX; k < runStartX + runLength; k++) mark[k, y] = true;
            }
        }

        // VERTICAAL: idem, maar kolom-voor-kolom
        for (int x = 0; x < width; x++)
        {
            int runStartY = 0;
            int runColor = -999;
            int runLength = 0;

            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == null)
                {
                    if (runLength >= 3)
                    {
                        for (int k = runStartY; k < runStartY + runLength; k++) mark[x, k] = true;
                    }
                    runLength = 0;
                    runColor = -999;
                    runStartY = y + 1;
                    continue;
                }

                Block b = grid[x, y].GetComponent<Block>();
                int c = b.colorId;

                if (runLength == 0)
                {
                    runStartY = y;
                    runLength = 1;
                    runColor = c;
                }
                else if (c == runColor)
                {
                    runLength++;
                }
                else
                {
                    if (runLength >= 3)
                    {
                        for (int k = runStartY; k < runStartY + runLength; k++) mark[x, k] = true;
                    }
                    runStartY = y;
                    runLength = 1;
                    runColor = c;
                }
            }

            if (runLength >= 3)
            {
                for (int k = runStartY; k < runStartY + runLength; k++) mark[x, k] = true;
            }
        }

        // Bouw lijst van te verwijderen posities (en debug print)
        List<(int x, int y)> removePositions = new List<(int x, int y)>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (mark[x, y] && grid[x, y] != null)
                {
                    removePositions.Add((x, y));
                }
            }
        }

        Debug.Log("Matches gevonden (count coords): " + removePositions.Count);

        // Verwijder nu veilig op basis van coördinaten uit de grid
        foreach (var pos in removePositions)
        {
            GameObject go = grid[pos.x, pos.y];
            if (go != null)
            {
                grid[pos.x, pos.y] = null;
                Destroy(go);
            }
        }

        if (removePositions.Count >= 3)
        {
            MoveManager moveManager = FindFirstObjectByType<MoveManager>();
            if (moveManager != null && !moveManager.gameIsOver)
            {
                moveManager.AddExtraMove();
                Debug.Log("Bonus move! Total moves: " + moveManager.totalMoves);
            }
        }
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

        CheckMatches();

    }
}


