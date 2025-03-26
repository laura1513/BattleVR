using UnityEngine;

public class MissileScript : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Tile"))
        {
            Debug.Log(collision.gameObject);
            GameManager.instance.CheckHit(collision.gameObject);
            Destroy(this.gameObject);
        }
    }
}
