using UnityEngine;

public class ClickManager : MonoBehaviour
{
    void Update()
    {
        // Mouse input (voor in editor/PC)
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick(Input.mousePosition);
        }

        // Touch input (voor mobiel)
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                HandleClick(touch.position);
            }
        }
    }

    void HandleClick(Vector3 screenPosition)
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPosition);
        Vector2 world2D = new Vector2(worldPos.x, worldPos.y);

        RaycastHit2D hit = Physics2D.Raycast(world2D, Vector2.zero);

        if (hit.collider != null)
        {
            Block block = hit.collider.GetComponent<Block>();
            if (block != null)
            {
                block.OnClicked();
            }
        }
    }
}