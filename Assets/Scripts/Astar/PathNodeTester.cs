using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class PathNodeTester : MonoBehaviour
{
    public List<PathNode> sources;
    public GameObject start;
    public GameObject end;
    public Color nodeColor = new Color(0.05f, 0.3f, 0.05f, 0.1f);
    public Color pulseColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);
    public Color pathColor = new Color(0.0f, 1.0f, 0.0f, 1.0f);
    public bool reset;

    public bool gridCreated;
    int startIndex;
    int endIndex;

    int lastEndIndex;
    int lastStartIndex;

    bool donePath;
    public List<PathNode> solvedPath = new List<PathNode>();



    public void Awake()
    {

        if (gridCreated)
            return;
        sources = PathNode.CreateGrid(Vector3.zero, Vector3.one * 10.0f, new int[] { 6, 1, 6 }, 0.0f, gameObject);
        gridCreated = true;

    }

    public void PulsePoint(int index)
    {
        if (AStarHelper.Invalid(sources[index]))
            return;
        DrawHelper.DrawCube(sources[index].Position, Vector3.one * 2.0f, pulseColor);
    }


    public void Draw(int startPoint, int endPoint, Color inColor)
    {
        Debug.DrawLine(sources[startPoint].Position, sources[endPoint].Position, inColor);
    }

    static int Closest(List<PathNode> inNodes, Vector3 toPoint)
    {
        int closestIndex = 0;
        float minDist = float.MaxValue;
        for (int i = 0; i < inNodes.Count; i++)
        {
            if (AStarHelper.Invalid(inNodes[i]))
                continue;
            float thisDist = Vector3.Distance(toPoint, inNodes[i].Position);
            if (thisDist > minDist)
                continue;

            minDist = thisDist;
            closestIndex = i;
        }

        return closestIndex;
    }


    public void Update()
    {
        if (reset)
        {
            donePath = false;
            //ArrayFunc.Clear(ref solvedPath);
            reset = false;
        }

        if (start == null || end == null)
        {
            Debug.LogWarning("Need 'start' and or 'end' defined!");
            enabled = false;
            return;
        }

        startIndex = Closest(sources, start.transform.position);

        endIndex = Closest(sources, end.transform.position);


        if (startIndex != lastStartIndex || endIndex != lastEndIndex)
        {
            reset = true;
            lastStartIndex = startIndex;
            lastEndIndex = endIndex;
            return;
        }

        for (int i = 0; i < sources.Count; i++)
        {
            if (AStarHelper.Invalid(sources[i]))
                continue;
            sources[i].nodeColor = nodeColor;
        }

        PulsePoint(lastStartIndex);
        PulsePoint(lastEndIndex);


        if (!donePath)
        {

            solvedPath = AStarHelper.Calculate(sources[lastStartIndex], sources[lastEndIndex]);

            donePath = true;
        }

        // Invalid path
        if (solvedPath == null || solvedPath.Count < 1)
        {
            Debug.LogWarning("Invalid path!");
            reset = true;
            enabled = false;
            return;
        }


        //Draw path	
        for (int i = 0; i < solvedPath.Count - 1; i++)
        {
            if (AStarHelper.Invalid(solvedPath[i]) || AStarHelper.Invalid(solvedPath[i + 1]))
            {
                reset = true;

                return;
            }
            Debug.DrawLine(solvedPath[i].Position, solvedPath[i + 1].Position, Color.cyan * new Color(1.0f, 1.0f, 1.0f, 0.5f));
        }



    }

}