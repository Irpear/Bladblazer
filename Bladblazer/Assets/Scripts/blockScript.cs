using System.Collections;
using UnityEngine;

public class Block : MonoBehaviour
{
    public int x;
    public int y;

    public Board board;

    void Start()
    {
        board = FindFirstObjectByType<Board>();
    }

    void Update()
    {
        TryFall();
    }

    void TryFall()
    {
        // Kies hier de richting: linksonder of rechtsonder
        Vector2Int fallDirLeft = new Vector2Int(-1, -1);
        Vector2Int fallDirRight = new Vector2Int(1, -1);

        // Eerst proberen linksonder
        if (CanMoveTo(x + fallDirLeft.x, y + fallDirLeft.y))
        {
            MoveTo(x + fallDirLeft.x, y + fallDirLeft.y);
        }
        // Anders proberen rechtsonder
        else if (CanMoveTo(x + fallDirRight.x, y + fallDirRight.y))
        {
            MoveTo(x + fallDirRight.x, y + fallDirRight.y);
        }
    }

    bool CanMoveTo(int targetX, int targetY)
    {
        // Buiten het bord?
        if (targetX < 0 || targetX >= board.width || targetY < 0) return false;

        // Check of veld leeg is
        return board.grid[targetX, targetY] == null;
    }

    void MoveTo(int targetX, int targetY)
    {
        // Oude plek leegmaken
        board.grid[x, y] = null;

        // Nieuwe plek claimen
        x = targetX;
        y = targetY;
        board.grid[x, y] = gameObject;

        // Smooth bewegen
        StopAllCoroutines();
        StartCoroutine(MoveAnimation(new Vector2(x, y)));
    }

    IEnumerator MoveAnimation(Vector2 targetPos)
    {
        while ((Vector2)transform.position != targetPos)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPos, Time.deltaTime * 5f);
            yield return null;
        }
    }

    public void OnClicked()
    {
        Debug.Log($"Block clicked at {x},{y}");
        if (board != null)
        {
            board.grid[x, y] = null;
        }
        Destroy(gameObject);
    }

}