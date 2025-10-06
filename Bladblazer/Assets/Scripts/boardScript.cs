using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    [Header("Board Settings")]
    public int width = 12;          // Speelveld breedte
    public int height = 12;         // Speelveld hoogte
    public int bufferSize = 3;      // Hoe diep de buffer zones zijn

    public GameObject[] blockPrefabs;
    public GameObject[,] grid;      // Totale grid inclusief buffer

    public float timer = 0f;
    public bool timerActive = false;

    // Totale grid grootte (speelveld + buffer)
    private int totalWidth;
    private int totalHeight;

    // Offset voor het speelveld (buffer zit links, dus offset = bufferSize)
    private int playFieldOffsetX;
    private int playFieldOffsetY = 0; // Y blijft 0, buffer alleen boven

    public AudioClip matchSoundClip;
    public AudioClip blockFallSoundClip;

    void Start()
    {
        // Grid is groter dan speelveld voor buffer zones
        totalWidth = width + bufferSize;
        totalHeight = height + bufferSize;
        playFieldOffsetX = bufferSize; // Speelveld begint na de buffer

        grid = new GameObject[totalWidth, totalHeight];

        FillBoard();
        FillBufferZones(); // Vul de buffer zones aan de start
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
                // Na timer: vul buffer zones opnieuw
                FillBufferZones();
            }
        }
    }

    void FillBoard()
    {
        // Vul alleen het speelveld (met offset voor linkerbuffer)
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                SpawnBlock(x + playFieldOffsetX, y + playFieldOffsetY);
            }
        }
    }

    // Vult de buffer zones (links en boven)
    void FillBufferZones()
    {
        // Linkerkant buffer (x < playFieldOffsetX)
        for (int x = 0; x < playFieldOffsetX; x++)
        {
            for (int y = 0; y < totalHeight; y++)
            {
                if (grid[x, y] == null)
                {
                    SpawnBlock(x, y, isBuffer: true);
                }
            }
        }

        // Bovenkant buffer (y >= height)
        for (int y = height; y < totalHeight; y++)
        {
            for (int x = playFieldOffsetX; x < totalWidth; x++)
            {
                if (grid[x, y] == null)
                {
                    SpawnBlock(x, y, isBuffer: true);
                }
            }
        }
    }

    void SpawnBlock(int x, int y, bool isBuffer = false)
    {
        int maxAttempts = 10;
        int prefabIndex = Random.Range(0, blockPrefabs.Length);

        // Alleen match-preventie doen in het speelveld, niet in buffer
        if (!isBuffer)
        {
            for (int attempt = 0; attempt < maxAttempts; attempt++)
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
        }

        Vector2 pos = new Vector2(x, y);
        GameObject blockObj = Instantiate(blockPrefabs[prefabIndex], pos, Quaternion.identity);

        Block b = blockObj.GetComponent<Block>();
        b.x = x;
        b.y = y;
        b.colorId = prefabIndex;
        b.board = this;
        grid[x, y] = blockObj;

        // Maak buffer blokken onzichtbaar of semi-transparant
        if (isBuffer)
        {
            SpriteRenderer sr = blockObj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color;
                c.a = 0f; // Volledig transparant (of 0.3f voor semi-transparant)
                sr.color = c;
            }
        }
    }

    public bool CheckMatches()
    {
        Debug.Log("=== CheckMatches() START ===");
        bool[,] mark = new bool[totalWidth, totalHeight];

        // Check alleen binnen het speelveld (niet in buffer zones)
        // HORIZONTAAL
        for (int y = 0; y < height; y++)
        {
            int runStartX = playFieldOffsetX;
            int runColor = -999;
            int runLength = 0;

            for (int x = playFieldOffsetX; x < playFieldOffsetX + width; x++)
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
        for (int x = playFieldOffsetX; x < playFieldOffsetX + width; x++)
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
        for (int x = playFieldOffsetX; x < playFieldOffsetX + width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (mark[x, y] && grid[x, y] != null)
                {
                    removePositions.Add((x, y));
                    Debug.Log($"Match gevonden op: ({x}, {y})");
                    AudioSource.PlayClipAtPoint(matchSoundClip, transform.position);
                }
            }
        }

        Debug.Log($"Totaal matches gevonden: {removePositions.Count}");
        Debug.Log("=== CheckMatches() END ===");

        if (removePositions.Count > 0)
        {
            StartCoroutine(AnimateAndDestroyMatches(removePositions));
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

        return removePositions.Count > 0;
    }

    public IEnumerator ResolveMatches()
    {
        bool matchFound = true;

        while (matchFound)
        {
            ApplyGravity();
            FillBufferZones();
            yield return new WaitForSeconds(0.6f);

            matchFound = CheckMatches();

            // NIEUW: Wacht tot de animatie klaar is voordat we doorgaan
            if (matchFound)
            {
                yield return new WaitForSeconds(0.3f); // Zelfde tijd als in AnimateAndDestroyMatches
            }
        }
    }

    // ============ GRAVITY SYSTEEM MET BUFFER SUPPORT ============

    public void ApplyGravity()
    {
        bool anyMoved = true;

        while (anyMoved)
        {
            anyMoved = false;

            // Scan het hele speelveld (met offset)
            for (int y = 0; y < height; y++)
            {
                for (int x = playFieldOffsetX; x < playFieldOffsetX + width; x++)
                {
                    if (grid[x, y] == null)
                    {
                        // Check druk vanuit alle richtingen (inclusief buffer)
                        int leftPressure = CountDiagonalPressure(x, y, -1, 0);
                        int upPressure = CountDiagonalPressure(x, y, 0, 1);

                        if (leftPressure > 0 || upPressure > 0)
                        {
                            Debug.Log($"Leeg vakje op ({x},{y}) - Links druk: {leftPressure}, Boven druk: {upPressure}");
                        }

                        if (leftPressure > upPressure && leftPressure > 0)
                        {
                            if (SlideDiagonalLine(x, y, -1, 0))
                                anyMoved = true;
                                AudioSource.PlayClipAtPoint(blockFallSoundClip, transform.position);
                        }
                        else if (upPressure > leftPressure && upPressure > 0)
                        {
                            if (SlideDiagonalLine(x, y, 0, 1))
                                anyMoved = true;
                                AudioSource.PlayClipAtPoint(blockFallSoundClip, transform.position);
                        }
                        else if (leftPressure > 0 || upPressure > 0)
                        {
                            if (Random.value < 0.5f && leftPressure > 0)
                            {
                                if (SlideDiagonalLine(x, y, -1, 0))
                                    anyMoved = true;
                                    AudioSource.PlayClipAtPoint(blockFallSoundClip, transform.position);
                            }
                            else if (upPressure > 0)
                            {
                                if (SlideDiagonalLine(x, y, 0, 1))
                                    anyMoved = true;
                                    AudioSource.PlayClipAtPoint(blockFallSoundClip, transform.position);
                            }
                        }
                    }
                }
            }
        }
    }

    private int CountDiagonalPressure(int emptyX, int emptyY, int dirX, int dirY)
    {
        int count = 0;
        int checkX = emptyX + dirX;
        int checkY = emptyY + dirY;

        // Tel door tot BUITEN de grid (inclusief buffer zones)
        while (checkX >= 0 && checkX < totalWidth && checkY >= 0 && checkY < totalHeight)
        {
            if (grid[checkX, checkY] != null)
            {
                count++;
                checkX += dirX;
                checkY += dirY;
            }
            else
            {
                break;
            }
        }

        return count;
    }

    private bool SlideDiagonalLine(int emptyX, int emptyY, int dirX, int dirY)
    {
        List<(int x, int y)> lineBlocks = new List<(int x, int y)>();

        int checkX = emptyX + dirX;
        int checkY = emptyY + dirY;

        // Verzamel blokken tot BUITEN het speelveld (buffer zones)
        while (checkX >= 0 && checkX < totalWidth && checkY >= 0 && checkY < totalHeight)
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

        if (lineBlocks.Count == 0)
            return false;

        // Schuif alle blokken
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

    private void MoveBlockImmediate(int fromX, int fromY, int toX, int toY)
    {
        if (grid[fromX, fromY] == null) return;

        GameObject block = grid[fromX, fromY];
        Block b = block.GetComponent<Block>();

        grid[toX, toY] = block;
        grid[fromX, fromY] = null;

        b.x = toX;
        b.y = toY;

        // Als blok het speelveld binnenkomt, maak het zichtbaar
        bool wasInBuffer = (fromX < playFieldOffsetX || fromY >= height);
        bool isNowInPlayfield = (toX >= playFieldOffsetX && toX < playFieldOffsetX + width && toY < height);

        if (wasInBuffer && isNowInPlayfield)
            if (wasInBuffer && isNowInPlayfield)
            {
                SpriteRenderer sr = block.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = 1f; // Volledig zichtbaar
                    sr.color = c;
                }
            }

        b.StopAllCoroutines();
        b.StartCoroutine(b.MoveAnimation(new Vector2(toX, toY)));
    }

    private IEnumerator AnimateAndDestroyMatches(List<(int x, int y)> positions)
    {
        // Verkleur
        foreach (var pos in positions)
        {
            GameObject go = grid[pos.x, pos.y];
            if (go != null)
            {
                SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = Color.black;
                }
            }
        }

        // Pauze
        yield return new WaitForSeconds(0.3f);

        // Destroy
        foreach (var pos in positions)
        {
            GameObject go = grid[pos.x, pos.y];
            if (go != null)
            {
                grid[pos.x, pos.y] = null;
                Destroy(go);
            }
        }
    }

}