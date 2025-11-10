using NUnit.Framework.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Board : MonoBehaviour
{
    [SerializeField] private GameObject scorePopupPrefab;
    [SerializeField] private Canvas canvas;

    [Header("Board Settings")]
    public int width = 12;          // Speelveld breedte
    public int height = 12;         // Speelveld hoogte
    public int bufferSize = 3;      // Hoe diep de buffer zones zijn

    public GameObject[] blockPrefabs;
    public GameObject[,] grid;      // Totale grid inclusief buffer

    int difficulty;

    public static bool fallLeft = true; // true = links, false = rechts
    public UnityEngine.UI.Image leftButtonImage;
    public UnityEngine.UI.Image rightButtonImage;
    public Color activeColor = new Color(1f, 1f, 1f, 0f);
    public Color inactiveColor = new Color(21f / 255f, 25f / 255f, 30f / 255f, 1f);

    public float timer = 0f;
    public bool timerActive = false;

    public bool isResolvingMatches = false;

    // Totale grid grootte (speelveld + buffer)
    private int totalWidth;
    private int totalHeight;

    // Offset voor het speelveld (buffer zit links, dus offset = bufferSize)
    private int playFieldOffsetX;
    private int playFieldOffsetY = 0; // Y blijft 0, buffer alleen boven

    public AudioClip matchSoundClip;
    public AudioClip blockFallSoundClip;

    public Block blockScript;
    public ScoreManager scoreManager;

    void Start()
    {

        difficulty = GameSettings.Difficulty;
        Debug.Log($"Difficulty loaded from GameSettings: {difficulty}");

        // Grid is groter dan speelveld voor buffer zones
        totalWidth = width + bufferSize;
        totalHeight = height + bufferSize;
        playFieldOffsetX = bufferSize; // Speelveld begint na de buffer

        grid = new GameObject[totalWidth, totalHeight];

        blockScript = FindFirstObjectByType<Block>();
        scoreManager = FindFirstObjectByType<ScoreManager>();

        FillBoard();
        FillBufferZones(); // Vul de buffer zones aan de start
        StartCoroutine(ResolveMatches());
        UpdateButtonVisuals();
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
        int prefabIndex;

        // EERST: Check of het een disco blok wordt (1 op 100)
        int disco = Random.Range(0, 100);
        if (disco == 42) 
        {
            prefabIndex = 9; // Disco block
        }
        else
        {
            prefabIndex = Random.Range(0, (7 + difficulty)); // bij easy is diff 0, dus dan gebruikt hij de prefabs 0 t/m 6. bij normal 1 dus t/m 7. bij hard 2 dus t/m 8.
        }

        if (!isBuffer && prefabIndex != 9)
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

                prefabIndex = Random.Range(0, (7 + difficulty));
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

    public (List<int> matchLengths, List<bool> isDiscoMatch, List<Vector3> matchCenters) CheckMatches()
    {
        Debug.Log("=== CheckMatches() START ===");
        List<int> matchLengths = new List<int>();
        List<bool> isDiscoMatch = new List<bool>();
        List<Vector3> matchCenters = new List<Vector3>();
        bool[,] mark = new bool[totalWidth, totalHeight];

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
                        Debug.Log($"Runlength: {runLength}");
                        matchLengths.Add(runLength);

                        // Bereken het middelpunt van de match
                        float centerX = runStartX + (runLength -1 ) / 2.0f;
                        float centerY = y;
                        Vector3 centerPos = new Vector3(centerX, centerY, 0);
                        matchCenters.Add(centerPos);

                        isDiscoMatch.Add(runColor == 9);
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
                        Debug.Log($"Runlength: {runLength}");
                        matchLengths.Add(runLength);

                        // Bereken het middelpunt van de match
                        float centerX = runStartX + (runLength - 1) / 2.0f;
                        float centerY = y;
                        Vector3 centerPos = new Vector3(centerX, centerY, 0);
                        matchCenters.Add(centerPos);


                        isDiscoMatch.Add(runColor == 9);
                    }
                    runStartX = x;
                    runLength = 1;
                    runColor = c;
                }
            }

            if (runLength >= 3)
            {
                for (int k = runStartX; k < runStartX + runLength; k++) mark[k, y] = true;
                matchLengths.Add(runLength);

                // Bereken het middelpunt van de match
                float centerX = runStartX + (runLength - 1) / 2.0f;
                float centerY = y;
                Vector3 centerPos = new Vector3(centerX, centerY, 0);
                matchCenters.Add(centerPos);


                isDiscoMatch.Add(runColor == 9);
            }
        }

        // VERTICAAL
        for (int x = playFieldOffsetX; x < playFieldOffsetX + width; x++)
        {
            int runStartY = playFieldOffsetY;
            int runColor = -999;
            int runLength = 0;

            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == null)
                {
                    if (runLength >= 3)
                    {
                        Debug.Log($"Vertical match: x={x}, runStartY={runStartY}, runLength={runLength}, calculated centerY={runStartY + (runLength - 1) / 2.0f}");

                        for (int k = runStartY; k < runStartY + runLength; k++) mark[x, k] = true;
                        //Debug.Log($"Marking vertical match at grid position ({x}, {k})");
                        Debug.Log($"Runlength: {runLength}");
                        matchLengths.Add(runLength);

                        // Bereken het middelpunt van de match
                        float centerX = x;
                        float centerY = runStartY + (runLength - 1) / 2.0f;
                        Vector3 centerPos = new Vector3(centerX, centerY, 0);
                        matchCenters.Add(centerPos);

                        isDiscoMatch.Add(runColor == 9);
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
                        Debug.Log($"Vertical match: x={x}, runStartY={runStartY}, runLength={runLength}, calculated centerY={runStartY + (runLength - 1) / 2.0f}");

                        for (int k = runStartY; k < runStartY + runLength; k++) mark[x, k] = true;
                        Debug.Log($"Runlength: {runLength}");
                        matchLengths.Add(runLength);

                        // Bereken het middelpunt van de match
                        float centerX = x;
                        float centerY = runStartY + (runLength - 1) / 2.0f;
                        Vector3 centerPos = new Vector3(centerX, centerY, 0);
                        matchCenters.Add(centerPos);

                        isDiscoMatch.Add(runColor == 9);
                    }
                    runStartY = y;
                    runLength = 1;
                    runColor = c;
                }
            }

            if (runLength >= 3)
            {
                for (int k = runStartY; k < runStartY + runLength; k++) mark[x, k] = true;
                matchLengths.Add(runLength);

                // Bereken het middelpunt van de match
                float centerX = x;
                float centerY = runStartY + (runLength - 1) / 2.0f;
                Vector3 centerPos = new Vector3(centerX, centerY, 0);
                matchCenters.Add(centerPos);

                isDiscoMatch.Add(runColor == 9);
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
                    timer += 0.5f;
                }
            }
        }

        Debug.Log($"Totaal matches gevonden: {removePositions.Count}");
        Debug.Log("=== CheckMatches() END ===");

        if (removePositions.Count > 0)
        {
            StartCoroutine(AnimateAndDestroyMatches(removePositions));
        }

        if (removePositions.Count >= 3)
        {
            MoveManager moveManager = FindFirstObjectByType<MoveManager>();
            if (moveManager != null && !moveManager.gameIsOver)
            {
                moveManager.AddExtraMove();
                Debug.Log("Bonus move!");
            }
        }

        return (matchLengths, isDiscoMatch, matchCenters);
    }

    public IEnumerator ResolveMatches()
    {
        isResolvingMatches = true;
        yield return new WaitForSeconds(0.25f);

        ApplyGravity();
        FillBufferZones();
        yield return new WaitForSeconds(0.6f);

        var (matchLengths, isDiscoMatch, matchCenters) = CheckMatches();

        if (matchLengths.Count > 0)
        {
            float currentMultiplier = ScoreManager.Instance.GetMultiplier();
            ScoreManager.Instance.SetMultiplier(currentMultiplier + 0.1f);
            Debug.Log("Multiplier set to: " + ScoreManager.Instance.GetMultiplier());

            for (int i = 0; i < matchLengths.Count; i++)
            {
                int runLength = matchLengths[i];
                bool disco = isDiscoMatch[i];
                Vector3 centerPos = matchCenters[i];

                int finalScore = runLength * scoreManager.pointsPerBlock;
                if (disco)
                {
                    finalScore *= 10;
                    Debug.Log("DISCO MATCH BONUS!");
                }

                finalScore = Mathf.RoundToInt(finalScore * ScoreManager.Instance.GetMultiplier());

                GameEvents.OnBlocksRemoved.Invoke(finalScore);

                SpawnScorePopup(finalScore, centerPos);
            }
            yield return new WaitForSeconds(0.3f);

            while (matchLengths.Count > 0)
            {
                ApplyGravity();
                FillBufferZones();
                yield return new WaitForSeconds(0.6f);
                (matchLengths, isDiscoMatch, matchCenters) = CheckMatches();

                if (matchLengths.Count > 0)
                {
                    currentMultiplier = ScoreManager.Instance.GetMultiplier();
                    ScoreManager.Instance.SetMultiplier(currentMultiplier + 0.1f);
                    Debug.Log("Multiplier set to: " + ScoreManager.Instance.GetMultiplier());

                    for (int i = 0; i < matchLengths.Count; i++)
                    {
                        int runLength = matchLengths[i];
                        bool disco = isDiscoMatch[i];
                        Vector3 centerPos = matchCenters[i];

                        int finalScore = runLength * scoreManager.pointsPerBlock;
                        if (disco)
                        {
                            finalScore *= 10;
                            Debug.Log("DISCO MATCH BONUS!");
                        }

                        finalScore = Mathf.RoundToInt(finalScore * ScoreManager.Instance.GetMultiplier());

                        GameEvents.OnBlocksRemoved.Invoke(finalScore);

                        SpawnScorePopup(finalScore, centerPos);
                    }
                    yield return new WaitForSeconds(0.3f);
                }
            }
        }
        else
        {
            ScoreManager.Instance.SetMultiplier(1.0f);
            Debug.Log("Multiplier reset to 1.0");
        }

        isResolvingMatches = false;
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
                            if (fallLeft && leftPressure > 0)
                            {
                                if (SlideDiagonalLine(x, y, -1, 0))
                                    anyMoved = true;
                                AudioSource.PlayClipAtPoint(blockFallSoundClip, transform.position);
                            }
                            else if (!fallLeft && upPressure > 0)
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

    private void SpawnScorePopup(int scoreAmount, Vector3 centerPos)
    {
        GameObject popupObj = Instantiate(scorePopupPrefab, canvas.transform);
        Debug.Log($"Popup instantiated: {popupObj != null}");

        ScorePopup popup = popupObj.GetComponent<ScorePopup>();
        Debug.Log($"ScorePopup component found: {popup != null}");

        popup.Initialize(scoreAmount, centerPos);
        Debug.Log("Initialize called");

    }

    public void SetFallDirectionLeft()
    {
        fallLeft = true;
        UpdateButtonVisuals();
        Debug.Log("Fall direction set to LEFT");
    }

    public void SetFallDirectionRight()
    {
        fallLeft = false;
        UpdateButtonVisuals();
        Debug.Log("Fall direction set to RIGHT");
    }

    private void UpdateButtonVisuals()
    {
        if (leftButtonImage != null)
            leftButtonImage.color = fallLeft ? activeColor : inactiveColor;

        if (rightButtonImage != null)
            rightButtonImage.color = fallLeft ? inactiveColor : activeColor;
    }

}