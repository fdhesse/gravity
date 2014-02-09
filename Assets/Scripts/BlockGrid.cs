using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Monobehaviour, means you can drag it into a gameObject.
/// This monobehaviour creates a grid in the attached GameObject. Many blocks put together programmatically.
/// The position of the attached GameObject determines the position of the grid.
/// </summary>
public class BlockGrid : MonoBehaviour
{
    public int rows; //number of rows of the grid - x direction
    public int cols; //number of columns of the grid - z direction
    public int beams;//number of beams of the grid - y direction

    static private int blockSize = 10; //this is the size of a block's edge

    private List<GameObject> blocks = new List<GameObject>();//list that holds all the blocks created in this grid

    private GameObject blockPrefab; //holder for the block prefab
    void Start()
    {
        blockPrefab = (GameObject)Resources.Load("Prefabs/Block", typeof(GameObject)); //get my block prefab please
        makeGridBlocks();
    }

    void Update()
    {
        //if cols or rows are changed, reinstantiate
    }


    /// <summary>
    /// Instantiates the blocks that make up the grid
    /// If the grid had already been instantiated this function destroys the gameobjects and re-instantiates them.
    /// The grid values can be thus changed on the fly (in game mode).
    /// </summary>
    private void makeGridBlocks()
    {
        transform.name = rows + "x" + cols + "x" + beams + " Grid";
        foreach (GameObject block in blocks)
        {
            Destroy(block);
        }
        for (int i = 0; i != rows; i++)
        {
            for (int j = 0; j != cols; j++)
            {
                for (int k = 0; k != beams; k++)
                {
                    Vector3 blockPosition = new Vector3(i * blockSize, k * blockSize, j * blockSize);
                    GameObject tempBlock = Instantiate(blockPrefab, blockPosition, Quaternion.identity) as GameObject;
                    tempBlock.transform.parent = transform;
                    blocks.Add(tempBlock);
                }
            }

        }

    }
}
