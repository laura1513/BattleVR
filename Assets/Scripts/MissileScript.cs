using UnityEngine;

public class MissileScript : MonoBehaviour
{
    private GameManager gameManager;
    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Tile"))
        {
            gameManager.CheckHit(collision.gameObject);
            Destroy(gameObject);
        }
    }
}
