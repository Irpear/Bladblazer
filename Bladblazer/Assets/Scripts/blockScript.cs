using System.Collections;
using UnityEngine;

public class Block : MonoBehaviour
{
    public int x;
    public int y;

    public Board board;
    public MoveManager moveManager;


    void Start()
    {
        board = FindFirstObjectByType<Board>();
        moveManager = FindFirstObjectByType<MoveManager>();
    }

    void Update()
    {
        TryFall();
        moveManager = FindFirstObjectByType<MoveManager>();

    }

    void TryFall()
    {
        // Kies hier de richting: linksonder of rechtsonder
        Vector2Int fallDirLeft = new Vector2Int(0, -1);
        Vector2Int fallDirRight = new Vector2Int(1, 0);

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


        // Inspawnen van een nieuw blok werkt nu
        // Nu er voor zorgen dat een ander blok pas weggehaalt kan worden als een nieuw blok is gespawned

        // Ik heb het een beetje geprobeerd door een !board.timerActive toe te voegen aan de if statement
        // Maar dat werkt niet helemaal, je kan blokken dan niet meer weghalen zolang de timer actief is
        // Maar als je klikt gaan je moves nog steeds wel naar beneden, en de timer wordt dan weer gereset (ook al gaan er geen blokken weg)

        if (!board.timerActive)
        {

            Debug.Log($"Block clicked at {x},{y}");
            if (board != null && !moveManager.gameIsOver && !board.timerActive)
            {
                board.grid[x, y] = null;
                Destroy(gameObject);
                Debug.Log(moveManager.gameIsOver);
            }

            board.timer = 0.5f;
            board.timerActive = true;

            moveManager.UseMove();
        }

    }

}