using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipScript : MonoBehaviour
{

    public float xOffset = 0;
    public float zOffset = 0;
    private float nextYrotation = 90f;
    private GameObject clickedTile;
    public GameObject rotar;
    int hitCount = 0;
    public int shipSize;

    private Material[] allMaterials;

    public List<GameObject> touchTiles = new List<GameObject>();
    List<Color> allColors = new List<Color>();

    private void Start()
    {
        // Guardar los colores de los materiales del barco
        allMaterials = GetComponent<Renderer>().materials;
        for (int i = 0; i < allMaterials.Length; i++)
        {
            allColors.Add(allMaterials[i].color);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Verificar si el objeto colisionado es un tile
        if (collision.gameObject.CompareTag("Tile"))
        {
            touchTiles.Add(collision.gameObject);
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        // Verificar si el objeto colisionado es un tile
        if (collision.gameObject.CompareTag("Tile"))
        {
            touchTiles.Remove(collision.gameObject);
        }
    }
    public void ClearTileList()
    {
        // Limpiar la lista de tiles
        touchTiles.Clear();
    }
    public Vector3 GetOffsetVec(Vector3 tilePos)
    {
        // Devolver un vector con la posici�n del tile
        return new Vector3(tilePos.x + xOffset, 2, tilePos.z + zOffset);
    }
    //Rotaci�n del barco
    public void RotateShip()
    {
        if (clickedTile == null)
        {
            return;
        }
        touchTiles.Clear();
        transform.localEulerAngles += new Vector3(0, nextYrotation, 0);
        nextYrotation *= -1;
        float temp = xOffset;
        xOffset = zOffset;
        zOffset = temp;
        SetPosition(clickedTile.transform.position);
    }
    //Colocar el barco en la posici�n del tile
    public void SetPosition(Vector3 pos)
    {
        ClearTileList();
        transform.position = new Vector3(pos.x + xOffset, 2, pos.z + zOffset);
    }
    //A�adir un tile a la lista de tiles
    public void SetClickedTile(GameObject tile)
    {
        clickedTile = tile;
    }

    public bool OnGameBoard()
    {
        // Verificar si el barco est� en el tablero
        if (touchTiles.Count == shipSize)
        {
            rotar.SetActive(false);
        }
        return touchTiles.Count == shipSize;
    }
    // Verificar si el barco est� hundido
    public bool ComprobarHundido()
    {
        hitCount++;
        return shipSize <= hitCount;
    }
}