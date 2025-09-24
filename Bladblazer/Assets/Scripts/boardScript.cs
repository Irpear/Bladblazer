using UnityEngine;

public class Board : MonoBehaviour
{
    public int width = 12;
    public int height = 12;
    public GameObject blockPrefab;

    public GameObject[,] grid;

    void Start()
    {
        grid = new GameObject[width, height];
        FillBoard();
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
        Vector2 pos = new Vector2(x, y);

        GameObject blockObj = Instantiate(blockPrefab, pos, Quaternion.identity);

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
}

