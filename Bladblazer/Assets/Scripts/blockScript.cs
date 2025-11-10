using System.Collections;
using UnityEngine;

public class Block : MonoBehaviour
{
    public int x;
    public int y;

    public Board board;
    public MoveManager moveManager;
    public ScoreManager scoreManager;
    public int colorId;
    public float animDelay = 0.25f;

    public AudioClip blockRemoveSoundClip;

    [SerializeField] private Animator animator;


    void Start()
    {
        board = FindFirstObjectByType<Board>();
        moveManager = FindFirstObjectByType<MoveManager>();
        scoreManager = FindFirstObjectByType<ScoreManager>();
    }

    void Update()
    {
        //TryFall();
        moveManager = FindFirstObjectByType<MoveManager>();

    }

    //void TryFall()
    //{
    //    // Kies hier de richting: linksonder of rechtsonder
    //    Vector2Int fallDirLeft = new Vector2Int(0, -1);
    //    Vector2Int fallDirRight = new Vector2Int(1, 0);

    //    // Eerst proberen linksonder
    //    if (CanMoveTo(x + fallDirLeft.x, y + fallDirLeft.y))
    //    {
    //        MoveTo(x + fallDirLeft.x, y + fallDirLeft.y);
    //    }
    //    // Anders proberen rechtsonder
    //    else if (CanMoveTo(x + fallDirRight.x, y + fallDirRight.y))
    //    {
    //        MoveTo(x + fallDirRight.x, y + fallDirRight.y);
    //    }
    //}

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

    public IEnumerator MoveAnimation(Vector2 targetPos)
    {
        while ((Vector2)transform.position != targetPos)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPos, Time.deltaTime * 5f);
            yield return null;
        }
    }

    public void OnClicked()
    {

        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            // Klik gebeurde over een UI-element, negeer
            return;
        }

        // Inspawnen van een nieuw blok werkt nu
        // Nu er voor zorgen dat een ander blok pas weggehaalt kan worden als een nieuw blok is gespawned

        // Ik heb het een beetje geprobeerd door een !board.timerActive toe te voegen aan de if statement
        // Maar dat werkt niet helemaal, je kan blokken dan niet meer weghalen zolang de timer actief is
        // Maar als je klikt gaan je moves nog steeds wel naar beneden, en de timer wordt dan weer gereset (ook al gaan er geen blokken weg)

        if (!board.timerActive && !board.isResolvingMatches)
        {

            Debug.Log($"Block clicked at {x},{y}");
            if (board != null && !moveManager.gameIsOver && !board.timerActive)
            {
                animator.SetBool("isRemoved", true);
                board.grid[x, y] = null;
                int BlockClickScore = Mathf.RoundToInt(1 * scoreManager.pointsPerBlock * scoreManager.currentMultiplier);
                scoreManager.AddScore(BlockClickScore);
                AudioSource.PlayClipAtPoint(blockRemoveSoundClip, transform.position);
                Debug.Log(moveManager.gameIsOver);

                // DELAY VOOR HET LATEN ZIEN VAN DE ANIMATIE
                StartCoroutine(DestroyAfterDelay(0.25f));
            }

            board.timer = 0.5f;
            board.timerActive = true;

            moveManager.UseMove();
        }

    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);

        board.StartCoroutine(board.ResolveMatches());
    }

}