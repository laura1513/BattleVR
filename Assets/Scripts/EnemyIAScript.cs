using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EnemyIAScript : MonoBehaviour
{
    char[] guessGrid;
    List<int> potentialHits;
    List<int> currentHits;
    private int guess;
    public GameObject enemyBombPrefab;
    public GameManager gameManager;
    private void Start()
    {
        potentialHits = new List<int>();
        currentHits = new List<int>();
        guessGrid = Enumerable.Repeat('o', 100).ToArray();
    }
    public List<int[]> PlaceEnemyShips()
    {
        // Lista de arrays de tama�o 5, cada array representa un barco
        List<int[]> enemyShips = new List<int[]>
        {
            new int[] { -1, -1, -1, -1, -1 },
            new int[] { -1, -1, -1, -1 },
            new int[] { -1, -1, -1 },
            new int[] { -1, -1, -1 },
            new int[] { -1, -1, }
        };
        // Lista de 100 numeros del 0 al 99
        int[] gridNumbers = Enumerable.Range(0, 100).ToArray();
        bool taken = true;
        // Loop para asignar los barcos a la lista de arrays
        foreach (int[] tileNumArray in enemyShips)
        {
            taken = true;
            while (taken)
            {
                taken = false;
                int shipNose = UnityEngine.Random.Range(0, 99);
                int rotateBool = UnityEngine.Random.Range(0, 2);
                int minusAmount = rotateBool == 0 ? 10 : 1;
                for (int i = 0; i < tileNumArray.Length; i++)
                {
                    // Verificar que el tile no est� ocupado
                    if ((shipNose - (minusAmount * i) < 0) || (gridNumbers[shipNose - i * minusAmount]) < 0)
                    {
                        taken = true;
                        break;
                    }
                    //El barco est� horizontal, verifica que el barco no se salga de los costados 0 a 10, 11 a 20
                    else if (minusAmount == 1 && shipNose / 10 != ((shipNose - i * minusAmount) - 1) / 10)
                    {
                        taken = true;
                        break;
                    }
                }
                // Si el barco no est� ocupado, asignar los tiles a la lista de arrays
                if (taken == false)
                {
                    for (int j = 0; j < tileNumArray.Length; j++)
                    {
                        tileNumArray[j] = gridNumbers[shipNose - j * minusAmount];
                        gridNumbers[shipNose - j * minusAmount] = -1;
                    }
                }
            }
        }

        return enemyShips;
    }

    public void NPCTurn()
    {
        List<int> hitIndex = new List<int>();
        // Verificar si hay un barco en la lista de arrays
        for (int i = 0; i < guessGrid.Length; i++)
        {
            //h == hit, m == miss, o == open
            if (guessGrid[i] == 'h')
            {
                hitIndex.Add(i);
            }
        }
        // Si hay m�s de un acierto, buscar el siguiente tile
        if (hitIndex.Count > 1)
        {
            int diff = hitIndex[1] - hitIndex[0];
            int posNeg = Random.Range(0, 2) * 2 - 1;
            int nextIndex = hitIndex[0] + diff;
            // Verificar que el siguiente tile no est� ocupado
            while (guessGrid[nextIndex] != 'o')
            {
                if (guessGrid[nextIndex] == 'm' || nextIndex > 100 || nextIndex < 0)
                {
                    diff *= -1;
                }
                nextIndex += diff;
            }
            guess = nextIndex;
        }
        else if (hitIndex.Count == 1)
        {
            List<int> closeTiles = new List<int>();
            //Para ir a norte, sur, este y oeste ya que hay 10 tiles en cada fila
            closeTiles.Add(-1);
            closeTiles.Add(1);
            closeTiles.Add(-10);
            closeTiles.Add(10);

            int index = Random.Range(0, closeTiles.Count);
            Debug.Log("Index: " + index);
            int possibleGuess = hitIndex[0] + closeTiles[index];
            //Verificar que el posible tiro no se salga del tablero
            bool onGrid = possibleGuess > -1 && possibleGuess < 100;

            while ((!onGrid || guessGrid[possibleGuess] != 'o') && closeTiles.Count > 0)
            {
                closeTiles.RemoveAt(index);
                index = Random.Range(0, closeTiles.Count);
                possibleGuess = hitIndex[0] + closeTiles[index];
                onGrid = possibleGuess > -1 && possibleGuess < 100;
            }
            guess = possibleGuess;
        }
        // Si no hay barcos, buscar un tile aleatorio
        else
        {
            int nextIndex = Random.Range(0, 100);
            while (guessGrid[nextIndex] != 'o') nextIndex = Random.Range(0, 100);
            nextIndex = GuessAgainCheck(nextIndex);
            nextIndex = GuessAgainCheck(nextIndex);
            guess = nextIndex;
        }
        //Busca la casilla en el tablero
        GameObject tile = GameObject.Find("WaterCell (" + (guess + 1) + ")");
        guessGrid[guess] = 'm';
        Vector3 vec = tile.transform.position;
        //Crea la bomba en la casilla
        GameObject missile = Instantiate(enemyBombPrefab, vec, enemyBombPrefab.transform.rotation);
        missile.GetComponent<EnemyMissileScript>().SetTarget(guess);
        missile.GetComponent<EnemyMissileScript>().targetTileLocation = tile.transform.position;
    }
    private int GuessAgainCheck(int nextIndex)
    {
        string str = "nx: " + nextIndex;
        int newGuess = nextIndex;
        bool edgeCase = nextIndex < 10 || nextIndex > 89 || nextIndex % 10 == 0 || nextIndex % 10 == 9;
        bool nearGuess = false;
        if (nextIndex + 1 < 100) {
            nearGuess = guessGrid[nextIndex + 1] != 'o';
        }
        if (!nearGuess && nextIndex - 1 > 0){
            nearGuess = guessGrid[nextIndex - 1] != 'o';
        }
        if (!nearGuess && nextIndex + 10 < 100) {
            nearGuess = guessGrid[nextIndex + 10] != 'o';
        }
        if (!nearGuess && nextIndex - 10 > 0){
            nearGuess = guessGrid[nextIndex - 10] != 'o';
        }
        if (edgeCase || nearGuess) {
            newGuess = Random.Range(0, 100);
        }
        while (guessGrid[newGuess] != 'o') newGuess = Random.Range(0, 100);
        return newGuess;
    }
    public void MissileHit(int hit)
    {
        guessGrid[guess] = 'h';
        Invoke("EndTurn", 2f);
    }

    //Se ejecuta una vez que el jugador hunde un barco
    public void JugadorHundido()
    {
        for (int i = 0; i < guessGrid.Length; i++)
        {
            if (guessGrid[i] == 'h')
            {
                //Si el barco est� hundido, marcar los tiles adyacentes como agua
                guessGrid[i] = 'x';
            }
        }
    }

    private void EndTurn()
    {
        gameManager.GetComponent<GameManager>().EndEnemyTurn();
    }

    public void PauseAndEnd(int miss)
    {
        if (currentHits.Count > 0 && currentHits[0] > miss)
        {
            foreach(int potential in potentialHits)
            {
                //Si el potencial de los impactos es menor que el de los fallos, eliminarlo
                if (currentHits[0] > miss)
                {
                    if (potential < miss)
                    {
                        potentialHits.Remove(potential);
                    }
                }
                else
                {
                    if (potential > miss)
                    {
                        potentialHits.Remove(potential);
                    }
                }
            }
        }
        Invoke("EndTurn", 2f);
    }
}
