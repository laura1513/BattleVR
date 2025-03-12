using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Ships")]
    public GameObject[] ships;
    public EnemyIAScript enemyIAScript;
    public List<TileScript> allTileScript;
    public ShipScript shipScript;
    private List<int[]> enemyShips;
    private int enemyShipCount = 5;
    private int playerShipCount = 5;
    private int shipIndex = 0;

    [Header("HUD")]
    public Button nextBtn;
    public Button reanudarBtn;
    public Text topText;
    public Text playerShipText;
    public Text enemyShipText;

    [Header("Objects")]
    public GameObject missilePrefab;
    public GameObject enemyMissilePrefab;
    public GameObject puerto;
    public GameObject firePrefab;

    private bool setupComplete = false;
    private bool playerTurn = true;

    [Header("GameObjects")]
    private List<GameObject> playerFires = new List<GameObject>();
    private List<GameObject> enemyFires = new List<GameObject>();

    private List<int> numeros = Enumerable.Range(0, 100).ToList();
    
    // Start is called before the first frame update
    void Start()
    {
        shipScript = ships[shipIndex].GetComponent<ShipScript>();
        nextBtn.onClick.AddListener(() => NextShipClicked());
        reanudarBtn.onClick.AddListener(() => Reanudar());
        enemyShips = enemyIAScript.PlaceEnemyShips();
    }
    //Boton para cambiar de barco
    public void NextShipClicked()
    {
        if (!shipScript.OnGameBoard())
        {
        }
        else
        {
            if (shipIndex <= ships.Length - 2)
            {
                shipIndex++;
                shipScript = ships[shipIndex].GetComponent<ShipScript>();
            }
            else
            {
                nextBtn.gameObject.SetActive(false);
                puerto.SetActive(false);
                topText.text = "Lanza la bomba";
                setupComplete = true;
                for (int i = 0; i < ships.Length; i++) ships[i].SetActive(false);
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        //rotate ship
        /*if (Input.GetKeyDown(KeyCode.R))
        {
            
        }*/
    }

    //Funcion para cuando se hace click en una casilla
    public void TileClicked(GameObject tile)
    {
        if (setupComplete && playerTurn)
        {
            // se lanza un misil
            Vector3 tilePos = tile.transform.position;
            tilePos.y += 200;
            playerTurn = false;
            Instantiate(missilePrefab, tilePos, missilePrefab.transform.rotation);
        }
        else if (!setupComplete)
        {
            // se coloca un barco
            PlaceShip(tile);
            shipScript.SetClickedTile(tile);
        }
    }
    //Funcion para colocar un barco
    private void PlaceShip(GameObject tile)
    {
        // se coloca un barco en la casilla

        shipScript = ships[shipIndex].GetComponent<ShipScript>();
        shipScript.ClearTileList();
        Vector3 newVec = shipScript.GetOffsetVec(tile.transform.position);
        Debug.Log(newVec);
        ships[shipIndex].transform.position = newVec;
    }
    //Funcion para lanzar un misil
    public void CheckHit(GameObject tile)
    {
        // Obtener el n�mero del nombre del objeto
        Match match = Regex.Match(tile.name, @"\d+");

        if (!match.Success)
        {
            Debug.LogError($"Error: El nombre del tile '{tile.name}' no contiene un n�mero v�lido.");
            return;
        }

        if (!int.TryParse(match.Value, out int tileNum))
        {
            Debug.LogError($"Error: No se pudo convertir '{match.Value}' a un n�mero entero.");
            return;
        }

        int hitCount = 0;

        foreach (int[] tileNumArray in enemyShips)
        {
            if (tileNumArray.Contains(tileNum))
            {
                for (int i = 0; i < tileNumArray.Length; i++)
                {
                    if (tileNumArray[i] == tileNum)
                    {
                        tileNumArray[i] = -5;  // Marcar como golpeado
                        hitCount++;
                    }
                    else if (tileNumArray[i] == -5)
                    {
                        hitCount++;
                    }
                }

                // Verificar si el barco est� hundido
                if (hitCount == tileNumArray.Length)
                {
                    enemyShipCount--;
                    topText.text = "Hundido";
                    enemyFires.Add(Instantiate(firePrefab, tile.transform.position, Quaternion.identity));
                    tile.GetComponent<TileScript>().SetTileColor(1, new Color32(68, 0, 0, 225));
                    tile.GetComponent<TileScript>().SwitchColors(1);
                }
                else
                {
                    topText.text = "Tocado";
                    enemyFires.Add(Instantiate(firePrefab, tile.transform.position, Quaternion.identity));
                    tile.GetComponent<TileScript>().SetTileColor(1, new Color32(225, 0, 0, 225));
                    tile.GetComponent<TileScript>().SwitchColors(1);
                }
                //return; // Salir del m�todo, ya que se encontr� un impacto
            }
        }

        // Si no hubo impactos
        if (hitCount == 0)
        {
            topText.text = "Agua";
            tile.GetComponent<TileScript>().SetTileColor(1, new Color32(255, 255, 0, 0));
            tile.GetComponent<TileScript>().SwitchColors(1);
        }
        Invoke("EndPlayerTurn", 2f);
    }

    //Funcion para cuando un misil enemigo impacta en un barco del jugador
    public void EnemyHitPlayer(Vector3 tile, int tileNum, GameObject hitObj)
    {
        enemyIAScript.MissileHit(tileNum);
        tile.y += 0.2f;
        playerFires.Add(Instantiate(firePrefab, tile, Quaternion.identity));
        if (hitObj.GetComponent<ShipScript>().ComprobarHundido())
        {
            playerShipCount--;
            playerShipText.text = playerShipCount.ToString();
            enemyIAScript.JugadorHundido();
        }
        Invoke("EndEnemyTurn", 2f);
    }
    private void EndPlayerTurn()
    {
        for (int i = 0; i < ships.Length; i++)
        {
            ships[i].SetActive(true);
        }
        foreach (GameObject fire in playerFires)
        {
            fire.SetActive(true);
        }
        foreach (GameObject fire in enemyFires)
        {
            fire.SetActive(false);
        }
        enemyShipText.text = enemyShipCount.ToString();
        topText.text = "Turno del enemigo";
        enemyIAScript.NPCTurn();
        CollorAllTiles(0);
        if (playerShipCount < 1)
        {

            topText.text = "Perdiste";
        }
    }

    public void EndEnemyTurn()
    {
        for (int i = 0; i < ships.Length; i++)
        {
            ships[i].SetActive(false);
        }
        foreach (GameObject fire in playerFires)
        {
            fire.SetActive(false);
        }
        foreach (GameObject fire in enemyFires)
        {
            fire.SetActive(true);
        }
        playerShipText.text = playerShipCount.ToString();
        topText.text = "Lanza el misil";
        playerTurn = true;
        CollorAllTiles(1);
        if (enemyShipCount < 1)
        {
            topText.text = "Ganaste";
        }
    }

    private void CollorAllTiles(int colorIndex)
    {
        foreach(TileScript tile in allTileScript)
        {
            tile.SwitchColors(colorIndex);
        }

    }
    void GameOver(string message)
    {
        topText.text = message;
        reanudarBtn.gameObject.SetActive(true);
    }

    void Reanudar()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void Rotar()
    {
        shipScript.RotateShip();
    }
}