using UnityEngine;

public class Board : MonoBehaviour
{
    public int width = 8;
    public int height = 8;
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

}
