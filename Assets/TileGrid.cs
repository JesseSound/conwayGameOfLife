using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGrid : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject tilePrefab;
    List<List<GameObject>> tiles = new List<List<GameObject>>();
    //I want the sim to be slower okAYYY
    public float delayBetweenIterations = 0.1f;
    private float timer = 0f;
    public Gradient colGrad;

    int[,] types =
    {
        { 1, 0, 1, 0, 1, 0, 1, 1, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0,1 },
        { 0, 1, 1, 0, 0, 1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 0, 0, 0,1 },
        { 0, 0, 1, 1, 0, 1, 0, 1, 0, 1, 1, 0, 1, 0, 0, 1, 0, 0, 0, 0,1 },
        { 0, 0, 1, 0, 1, 0, 0, 0, 1, 1, 0, 1, 0, 1, 1, 1, 1, 0, 0, 0,1 },
        { 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 1, 1, 0, 1, 1, 0, 0,0 },
        { 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 1, 1, 0, 0, 0,1 },
        { 0, 0, 1, 1, 1, 1, 1, 0, 1, 0, 1, 0, 1, 0, 0, 1, 1, 0, 0, 0,1 },
        { 1, 1, 1, 0, 1, 0, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0,1 },
        { 0, 0, 1, 0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 1, 0, 1, 1, 1, 1, 0,1 },
        { 0, 0, 1, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 1, 1,0 }
    };

    void Start()
    {
        int rows = types.GetLength(0);
        int cols = types.GetLength(1);
        float x = 0.5f;
        float y = 0.5f + rows - 1;
        for (int row = 0; row < rows; row++)
        {
            List<GameObject> columnTiles = new List<GameObject>();
            for (int col = 0; col < cols; col++)
            {
                GameObject tile = Instantiate(tilePrefab);
                tile.transform.position = new Vector3(x, y);
                columnTiles.Add(tile);

                x += 1.0f;
            }
            tiles.Add(columnTiles);
            x = 0.5f;
            y -= 1.0f;
        }


    }

    void FixedUpdate()
    {
        Vector2 screenCenter = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width / 2f, Screen.height / 2f));

        int rows = types.GetLength(0);
        int cols = types.GetLength(1);

        int[,] nextGeneration = new int[rows, cols];
        timer += Time.fixedDeltaTime;

        if (timer >= delayBetweenIterations)
        {
            timer -= delayBetweenIterations;
            // Loop through every cell 
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    int aliveNeighbours = 0;

                    // Get alive neighbours
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            if (i == 0 && j == 0)
                                continue;

                            int neighborRow = row + i;
                            int neighborCol = col + j;

                            // Check if the neighbor is within bounds
                            if (neighborRow >= 0 && neighborRow < rows &&
                                neighborCol >= 0 && neighborCol < cols &&
                                types[neighborRow, neighborCol] == 1)
                            {
                                aliveNeighbours++;
                            }
                        }
                    }

                    // Apply the rules of the game
                    if (types[row, col] == 1 && (aliveNeighbours < 2 || aliveNeighbours > 3))
                    {
                        // Cell dies due to underpopulation or social anxiety
                        nextGeneration[row, col] = 0;
                    }
                    else if (types[row, col] == 0 && aliveNeighbours == 3)
                    {
                        // Cell is born due to other cells bangin'
                        nextGeneration[row, col] = 1;
                    }
                    else
                    {
                        // Cell remains the same
                        nextGeneration[row, col] = types[row, col];
                    }
                }
            }

            // Update the current generation with the next generation
            types = nextGeneration;

            // Update tile colors
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {

                    // Calculate the position of the current cell
                    Vector2 cellPosition = new Vector2(col + 0.5f, rows - row - 0.5f); // Adjust as needed based on your grid setup

                    // Calculate the distance between the cell and the center of the screen
                    float distanceToCenter = Vector2.Distance(screenCenter, cellPosition);

                    // Map the distance to a color using the gradient
                    Color color = types[row, col] == 1 ? colGrad.Evaluate(distanceToCenter) : Color.black;

                    // Apply the color to the tile
                    tiles[row][col].GetComponent<SpriteRenderer>().color = color;

                }
            }
        }



        bool reset = CheckTiles();
        Debug.Log(reset);
        if (reset)
        {
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    types[row, col] = Random.Range(0, 2);
                   
                }
            }
        }
    }

    // reset the game if everything is black
    bool CheckTiles()
    {
        // Iterate through each cell in the types array
        int rows = types.GetLength(0);
        int cols = types.GetLength(1);
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                // If any tile is not black
                if (types[row,col] == 1)
                {
                    return false;
                }
            }
        }

        // If all tiles black
        return true;
    }


}
