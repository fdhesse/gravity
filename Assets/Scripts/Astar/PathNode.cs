using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathNode : MonoBehaviour, IPathNode<PathNode>
{
    public List<PathNode> connections;

    public static int pnIndex;

    public Color nodeColor = new Color(0.05f, 0.3f, 0.05f, 0.1f);

    public bool Invalid
    {
        get { return (this == null); }
    }

    public List<PathNode> Connections
    {
        get { return connections; }
    }

    public Vector3 Position
    {
        get
        {

            return transform.position;
        }
    }

    public void Update()
    {
        DrawHelper.DrawCube(transform.position, Vector3.one, nodeColor);
        if (connections == null)
            return;
        for (int i = 0; i < connections.Count; i++)
        {
            if (connections[i] == null)
                continue;
            Debug.DrawLine(transform.position, connections[i].Position, nodeColor);
        }
    }

    public void Awake()
    {
        if (connections == null)
            connections = new List<PathNode>();

    }

    public static PathNode Spawn(Vector3 inPosition)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = "pn_" + pnIndex;
        obj.transform.position = inPosition;
        obj.transform.localScale = Vector3.one * 10.0f;
        obj.GetComponent<Renderer>().enabled = false;
        pnIndex++;

        PathNode newNode = obj.AddComponent<PathNode>();
        return newNode;
    }

    public static List<PathNode> CreateGrid(Vector3 center, Vector3 spacing, int[] dim, float randomSpace,GameObject parent)
    {
        GameObject groupObject = new GameObject("grid");
        Random.seed = 1337;
        int xCount = dim[0];
        int yCount = dim[1];
        int zCount = dim[2];
        float xWidth = spacing.x * xCount;
        float zWidth = spacing.z * yCount;

        float xStart = center.x - (xWidth / 2.0f) + (spacing.x / 2.0f);
        float zStart = center.z - (zWidth / 2.0f) + (spacing.z / 2.0f);

        List<PathNode> result = new List<PathNode>();

        for (int x = 0; x < xCount; x++)
        {
            float xPos = (x * spacing.x) + xStart;

            for (int y = 0; y < zCount; y++)
            {
                if (randomSpace > 0.0f)
                {
                    if (Random.value <= randomSpace)
                    {
                        result.Add(null);
                        continue;

                    }
                }
                float yPos = (y * spacing.z) + zStart;
                PathNode newNode = Spawn(new Vector3(xPos, 0.0f, yPos));

                result.Add(newNode);
                newNode.transform.parent = groupObject.transform;

            }
        }
        groupObject.transform.parent = parent.transform;
        groupObject.transform.position = new Vector3(parent.transform.position.x + xWidth/2, parent.transform.position.y, parent.transform.position.z + zWidth/2);

        for (int x = 0; x < xCount; x++)
        {


            for (int z = 0; z < zCount; z++)
            {
                int thisIndex = (x * zCount) + z;
                List<int> connectedIndicies = new List<int>();
                PathNode thisNode = result[thisIndex];
                if (AStarHelper.Invalid(thisNode)) continue;
                if (x != 0)
                    connectedIndicies.Add(((x - 1) * yCount) + z);
                if (x != xCount - 1)
                    connectedIndicies.Add(((x + 1) * yCount) + z);
                if (z != 0)
                    connectedIndicies.Add((x * yCount) + (z - 1));
                if (z != yCount - 1)
                    connectedIndicies.Add((x * yCount) + (z + 1));

                if (x != 0 && z != 0)
                    connectedIndicies.Add(((x - 1) * yCount) + (z - 1));

                if (x != xCount - 1 && z != yCount - 1)
                    connectedIndicies.Add(((x + 1) * yCount) + (z + 1));

                if (x != 0 && z != yCount - 1)
                    connectedIndicies.Add(((x - 1) * yCount) + (z + 1));

                if (x != xCount - 1 && z != 0)
                    connectedIndicies.Add(((x + 1) * yCount) + (z - 1));

                for (int i = 0; i < connectedIndicies.Count; i++)
                {
                    PathNode thisConnection = result[connectedIndicies[i]];
                    if (AStarHelper.Invalid(thisConnection))
                        continue;
                    thisNode.Connections.Add(thisConnection);
                }

            }
        }

        return result;

    }
}