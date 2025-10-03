using System.Collections;
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
        StartCoroutine(ResolveMatches());
    }

    private void Update()
    {
        if (timerActive)
        {
            
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
        int maxAttemps = 10;
        int prefabIndex = Random.Range(0, blockPrefabs.Length);

        for (int attempt = 0; attempt < maxAttemps; attempt++)
        {
            bool hasSameNeighbor = false;

            if (x > 0 && grid[x - 1, y] != null)
            {
                if (grid[x - 1, y].name.Contains(blockPrefabs[prefabIndex].name))
                {
                    hasSameNeighbor = true;
                }
            }

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
        b.board = this;
        grid[x, y] = blockObj;
    }

    public bool CheckMatches()
    {
        Debug.Log("=== CheckMatches() START ===");
        bool[,] mark = new bool[width, height];

        // HORIZONTAAL
        for (int y = 0; y < height; y++)
        {
            int runStartX = 0;
            int runColor = -999;
            int runLength = 0;

            for (int x = 0; x < width; x++)
            {
                if (grid[x, y] == null)
                {
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
                    if (runLength >= 3)
                    {
                        for (int k = runStartX; k < runStartX + runLength; k++) mark[k, y] = true;
                    }
                    runStartX = x;
                    runLength = 1;
                    runColor = c;
                }
            }

            if (runLength >= 3)
            {
                for (int k = runStartX; k < runStartX + runLength; k++) mark[k, y] = true;
            }
        }

        // VERTICAAL
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

        // Verwijder de matches
        foreach (var pos in removePositions)
        {
            GameObject go = grid[pos.x, pos.y];
            if (go != null)
            {
                grid[pos.x, pos.y] = null;
                Destroy(go);
            }
        }

        // Als er matches waren, geef bonus move
        if (removePositions.Count >= 3)
        {
            MoveManager moveManager = FindFirstObjectByType<MoveManager>();
            if (moveManager != null && !moveManager.gameIsOver)
            {
                moveManager.AddExtraMove();
                Debug.Log("Bonus move!");
            }
        }

        // Return true als er überhaupt matches waren (ook als het minder dan 3 zijn)
        return removePositions.Count > 0;
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

    public IEnumerator ResolveMatches()
    {
        bool matchFound = true;

        while (matchFound)
        {
            ApplyGravity();
            yield return new WaitForSeconds(0.6f);
            matchFound = CheckMatches();
        }
    }

    // ============ NIEUWE GRAVITY SYSTEEM ============

    public void ApplyGravity()
    {
        bool anyMoved = true;

        // Blijf herhalen totdat niks meer beweegt
        while (anyMoved)
        {
            anyMoved = false;

            // Scan van linksonder naar rechtsboven (belangrijk voor correcte volgorde)
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (grid[x, y] == null) // Leeg vakje gevonden
                    {
                        // Check welke diagonale rij de meeste druk heeft
                        int leftPressure = CountDiagonalPressure(x, y, -1, 0); // Links (x-1, y richting)
                        int upPressure = CountDiagonalPressure(x, y, 0, 1);    // Boven (x, y+1 richting)

                        if (leftPressure > upPressure && leftPressure > 0)
                        {
                            // Schuif hele linker diagonale rij
                            if (SlideDiagonalLine(x, y, -1, 0))
                                anyMoved = true;
                        }
                        else if (upPressure > leftPressure && upPressure > 0)
                        {
                            // Schuif hele boven diagonale rij
                            if (SlideDiagonalLine(x, y, 0, 1))
                                anyMoved = true;
                        }
                        else if (leftPressure > 0 || upPressure > 0)
                        {
                            // Gelijke druk: kies random
                            if (Random.value < 0.5f && leftPressure > 0)
                            {
                                if (SlideDiagonalLine(x, y, -1, 0))
                                    anyMoved = true;
                            }
                            else if (upPressure > 0)
                            {
                                if (SlideDiagonalLine(x, y, 0, 1))
                                    anyMoved = true;
                            }
                        }
                    }
                }
            }
        }
    }

    // Telt hoeveel blokken er in een diagonale lijn zitten
    private int CountDiagonalPressure(int emptyX, int emptyY, int dirX, int dirY)
    {
        int count = 0;
        int checkX = emptyX + dirX;
        int checkY = emptyY + dirY;

        // Loop door de diagonale lijn
        while (checkX >= 0 && checkX < width && checkY >= 0 && checkY < height)
        {
            if (grid[checkX, checkY] != null)
            {
                count++;
                checkX += dirX;
                checkY += dirY;
            }
            else
            {
                break; // Stop bij eerste gat
            }
        }

        return count;
    }

    // Schuift een hele diagonale rij naar beneden (als trein over rails)
    private bool SlideDiagonalLine(int emptyX, int emptyY, int dirX, int dirY)
    {
        // Vind alle blokken in deze diagonale lijn
        List<(int x, int y)> lineBlocks = new List<(int x, int y)>();

        int checkX = emptyX + dirX;
        int checkY = emptyY + dirY;

        // Verzamel alle aaneengesloten blokken in de lijn
        while (checkX >= 0 && checkX < width && checkY >= 0 && checkY < height)
        {
            if (grid[checkX, checkY] != null)
            {
                lineBlocks.Add((checkX, checkY));
                checkX += dirX;
                checkY += dirY;
            }
            else
            {
                break;
            }
        }

        // Als er geen blokken zijn om te schuiven
        if (lineBlocks.Count == 0)
            return false;

        // Schuif alle blokken in de lijn één positie naar beneden
        // Start bij het eerste blok (dichtstbij de lege plek)
        for (int i = 0; i < lineBlocks.Count; i++)
        {
            int fromX = lineBlocks[i].x;
            int fromY = lineBlocks[i].y;
            int toX = (i == 0) ? emptyX : lineBlocks[i - 1].x;
            int toY = (i == 0) ? emptyY : lineBlocks[i - 1].y;

            MoveBlockImmediate(fromX, fromY, toX, toY);
        }

        return true;
    }

    // Verplaatst een blok direct (voor gravity systeem)
    private void MoveBlockImmediate(int fromX, int fromY, int toX, int toY)
    {
        if (grid[fromX, fromY] == null) return;

        GameObject block = grid[fromX, fromY];
        Block b = block.GetComponent<Block>();

        grid[toX, toY] = block;
        grid[fromX, fromY] = null;

        b.x = toX;
        b.y = toY;

        b.StopAllCoroutines();
        b.StartCoroutine(b.MoveAnimation(new Vector2(toX, toY)));
    }
}