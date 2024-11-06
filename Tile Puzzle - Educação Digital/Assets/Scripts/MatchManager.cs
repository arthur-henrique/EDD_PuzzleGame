using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MatchManager : MonoBehaviour
{
    // Reference to the parent object that will contain all game pieces
    [SerializeField] private Transform gameTransform;
    // Prefab for each individual piece in the game
    [SerializeField] private Transform piecePrefabTransform;
    // List to hold all pieces on the board
    private List<Transform> pieces;
    [SerializeField]
    private GameObject winningCanvas;
    [SerializeField]
    private GameObject losingCanvas;

    // Index of the empty tile position (last tile in the grid)
    private int emptyTile;
    // Size of the grid (e.g., 3 for a 3x3 grid)
    public int gameSize;
    public int maxMoves = 500;
    public int currentMoves;
    // Flag to check if shuffling is in progress
    private bool shuffling = false;

    private bool canPlay = true;

    public TMP_Text text;
    public TMP_Dropdown sizeDD;
    public TMP_Dropdown movesDD;

    // Function to create game pieces and arrange them on the board
    private void CreateGamePieces(float gapThickness)
    {
        // Calculate the width of each piece based on the game size
        float width = 1 / (float)gameSize;
        for (int row = 0; row < gameSize; row++)
        {
            for (int col = 0; col < gameSize; col++)
            {
                // Instantiate a new piece and add it to the pieces list
                Transform piece = Instantiate(piecePrefabTransform, gameTransform);
                pieces.Add(piece);

                // Set the local position of the piece based on its row and column
                piece.localPosition = new Vector3(-1 + (2 * width * col) + width,
                    +1 - (2 * width * row) - width, 0);

                // Set the scale of the piece to account for the gap between pieces
                piece.localScale = ((2 * width) - gapThickness) * Vector3.one;
                piece.name = $"{(row * gameSize) + col}";  // Name piece by its position in the grid

                // Create an empty tile by hiding the last piece and storing its index
                if ((row == gameSize - 1) && (col == gameSize - 1))
                {
                    emptyTile = (gameSize * gameSize) - 1;
                    piece.gameObject.SetActive(false);
                }
                else
                {
                    // Set the UV coordinates to map texture for each piece
                    float gap = gapThickness / 2;
                    Mesh mesh = piece.GetComponent<MeshFilter>().mesh;
                    Vector2[] uv = new Vector2[4];

                    // UV coordinates for the texture
                    uv[0] = new Vector2((width * col) + gap, 1 - ((width * (row + 1)) - gap));
                    uv[1] = new Vector2((width * (col + 1)) - gap, 1 - ((width * (row + 1)) - gap));
                    uv[2] = new Vector2((width * col) + gap, 1 - ((width * row) + gap));
                    uv[3] = new Vector2((width * (col + 1)) - gap, 1 - ((width * row) + gap));
                    mesh.uv = uv;
                }
            }
        }
    }

    // Initializes the pieces list and calls CreateGamePieces
    private void Start()
    {
        pieces = new List<Transform>();
        CreateGamePieces(0.01f);
        text.text = "Moves: " + currentMoves + "/" + maxMoves;
    }

    // Handles shuffling and player input for swapping tiles
    private void Update()
    {
        // Shuffle the board at the start
        if (!shuffling)
        {
            shuffling = true;
            StartCoroutine(WaitForShuffle(0.5f));
        }
        // Handle tile movement on mouse click
        if (Input.GetMouseButtonDown(0))
        {
            if(!canPlay)
            { return; }

            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit)
            {
                // Find the clicked piece and attempt to swap with the empty tile if valid
                for (int i = 0; i < pieces.Count; i++)
                {
                    if (pieces[i] == hit.transform)
                    {
                        if (SwapIfValid(i, -gameSize, gameSize)) { break; }
                        if (SwapIfValid(i, +gameSize, gameSize)) { break; }
                        if (SwapIfValid(i, -1, 0)) { break; }
                        if (SwapIfValid(i, +1, gameSize - 1)) { break; }
                    }
                }
                
                if (CheckCompletion())// Check if the puzzle is complete
                {
                    winningCanvas.SetActive(true);
                }
                
                if(CheckLost())
                {
                    losingCanvas.SetActive(true);
                }
            }
        }
    }

    // Swaps a piece with the empty tile if the move is valid
    // colCheck prevents horizontal moves from wrapping around the grid
    private bool SwapIfValid(int i, int offset, int colCheck)
    {
        if (((i % gameSize) != colCheck) && ((i + offset) == emptyTile))
        {
            // Swap positions in the list and on the grid
            (pieces[i], pieces[i + offset]) = (pieces[i + offset], pieces[i]);
            (pieces[i].localPosition, pieces[i + offset].localPosition) = (pieces[i + offset].localPosition, pieces[i].localPosition);
            emptyTile = i;  // Update the empty tile index
            
            return true;
        }
        return false;
    }

    // Shuffles the pieces randomly on the grid
    private void Shuffle()
    {
        int count = 0;
        int last = 0;
        // Shuffle until enough moves have been made
        while (count < (gameSize * gameSize * gameSize))
        {
            int rnd = Random.Range(0, gameSize * gameSize);
            if (rnd == last) { continue; }
            last = emptyTile;

            // Try each direction and count if a swap was made
            if (SwapIfValid(rnd, -gameSize, gameSize)) { count++; }
            else if (SwapIfValid(rnd, +gameSize, gameSize)) { count++; }
            else if (SwapIfValid(rnd, -1, 0)) { count++; }
            else if (SwapIfValid(rnd, +1, gameSize - 1)) { count++; }
        }
    }

    // Checks if all pieces are in the correct order
    private bool CheckCompletion()
    {
        for (int i = 0; i < pieces.Count; i++)
        {
            // Return false if any piece is out of place
            if (pieces[i].name != $"{i}")
            {
                currentMoves++;
                return false;
            }
        }
        canPlay = false;
        return true;  // Puzzle is complete
    }

    private bool CheckLost()
    {
        if (currentMoves >= maxMoves)
        {
            canPlay = false;
            return true;
        }
        else
        {
            text.text = "Moves: " + currentMoves + "/" + maxMoves;
            return false;
        }
        
    }

    // Waits for a given duration before shuffling the pieces
    private IEnumerator WaitForShuffle(float durationToWait)
    {
        yield return new WaitForSeconds(durationToWait);
        Shuffle();
    }

    public void DifficultySelector()
    {
        if(sizeDD.value == 0)
        {
            gameSize = 3;
        }
        else if (sizeDD.value == 1)
        {
            gameSize = 4;
        }
        else if(sizeDD.value == 2)
        {
            gameSize = 5;
        }
    }
    public void MovesSelector()
    {
        if (movesDD.value == 0)
        {
            maxMoves = 500;
        }
        else if (movesDD.value == 1)
        {
            maxMoves = 200;
        }
        else if (movesDD.value == 2)
        {
            maxMoves = 100;
        }
    }
}
