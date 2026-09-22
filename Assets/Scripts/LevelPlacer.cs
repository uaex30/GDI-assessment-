using UnityEngine;

public class LevelPlacer : MonoBehaviour
{
    
    public GameObject outerCorner;
    public GameObject outerWall;
    public GameObject innerCorner;
    public GameObject innerWall;
    public GameObject pellet;
    public GameObject powerPellet;
    public GameObject tJunction;
    public GameObject ghostExit;

    int[,] levelMap =
    {
        {1,2,2,2,2,2,2,2,2,2,2,2,2,7},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,4},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,4},
        {2,6,4,0,0,4,5,4,0,0,0,4,5,4},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,3},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,5},
        {2,5,3,4,4,3,5,3,3,5,3,4,4,4},
        {2,5,3,4,4,3,5,4,4,5,3,4,4,3},
        {2,5,5,5,5,5,5,4,4,5,5,5,5,4},
        {1,2,2,2,2,1,5,4,3,4,4,3,0,4},
        {0,0,0,0,0,2,5,4,3,4,4,3,0,3},
        {0,0,0,0,0,2,5,4,4,0,0,0,0,0},
        {0,0,0,0,0,2,5,4,4,0,3,4,4,8},
        {2,2,2,2,2,1,5,3,3,0,4,0,0,0},
        {0,0,0,0,0,0,5,0,0,0,4,0,0,0},
    };

    [ContextMenu("Place Tiles")]
    void PlaceTiles()
    {
        int rows = levelMap.GetLength(0);
        int cols = levelMap.GetLength(1);

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                int value = levelMap[row, col];

                GameObject prefabToPlace = null;

                switch (value)
                {
                    case 0:
                        continue;

                    case 1:
                        prefabToPlace = outerCorner;
                        break;

                    case 2:
                        prefabToPlace = outerWall;
                        break;

                    case 3:
                        prefabToPlace = innerCorner;
                        break;

                    case 4:
                        prefabToPlace = innerWall;
                        break;

                    case 5:
                        prefabToPlace = pellet;
                        break;

                    case 6:
                        prefabToPlace = powerPellet;
                        break;

                    case 7:
                        prefabToPlace = tJunction;
                        break;

                    case 8:
                        prefabToPlace = ghostExit;
                        break;
                }

                if (prefabToPlace != null)
                {
                    Vector3 position = new Vector3(col, -row, 0);

                    Instantiate(
                        prefabToPlace,
                        position,
                        Quaternion.identity,
                        transform
                    );
                }
            }
        }
    }
}