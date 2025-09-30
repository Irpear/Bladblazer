using UnityEngine;

public class ClickManager : MonoBehaviour
{

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mouse2D = new Vector2(mousePos.x, mousePos.y);

            // Physics2D raycast
            RaycastHit2D hit = Physics2D.Raycast(mouse2D, Vector2.zero);
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
}