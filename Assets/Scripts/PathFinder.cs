/*
using UnityEngine;
using System.Collections.Generic;
using Wintellect.PowerCollections;
using System.Diagnostics;
using System.Threading;

public class PathFinder : MonoBehaviour {

    private Grid grid;
    private byte[,] pathGrid;

    Thread[] threads;
    public List<ManualResetEvent> doneEvents = new List<ManualResetEvent>();
    public List<PathFinder> movePaths = new List<PathFinder>();
    float threadWorkTimer;
    public int gridPowerOfSize = 5;
    float fTimerBlockedUpdate = 0;
    public float timerBlockedUpdate = 0.3f;

    public enum HeuristicFormula { Manhattan, MaxDXDY, DiagonalShortcut, Euclidean, EuclideanNoSQR };

    public bool done;
    public bool pathFound = false;
    private ManualResetEvent _doneEvent;
    public List<Point> path;

    public HeuristicFormula formula = HeuristicFormula.DiagonalShortcut;
    public int maxSearch = 10000;
    public bool useDiagonals = true;
    public bool heavyDiagonals = true;
    public bool tieBreaker = true;
    public int mHEstimate = 2;
    //public byte[,] mGrid = null;

    public bool diagonalNeighbors = true;

    public byte closedNode = 2;
    public byte openNode = 1;
    public float nodeSize = 0.5f;

    int startX, startY, endX, endY;

    public PathFinder(Point Start, Point End, ManualResetEvent _m)
    {

        // mov = _mov;
        _doneEvent = _m;
        
        //formula = p.formula;
        //maxSearch = p.maxSearch;
        //useDiagonals = p.useDiagonals;
        //heavyDiagonals = p.heavyDiagonals;
        //tieBreaker = p.tieBreaker;
        //mHEstimate = p.mHEstimate;
        //diagonalNeighbors = p.diagonalNeighbors;

        startX = Start.x;
        startY = Start.y;

        endX = End.x;
        endY = End.y;

    }

    void Awake()
    {
        grid = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>().grid;
    }


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
    // arraytest = List<Pathfinder>
	void Update () {

        //makes threads do work
        threadWorkTimer += Time.deltaTime;
        if (threadWorkTimer > 0.2f)
        {
            for (int i = 0; i < movePaths.Count; i++)
            {
                if (movePaths[i].done)
                {

                    if (movePaths[i].pathFound)
                    {
                        List<Vector2> pathPos = new List<Vector2>();
                        for (int t = 0; t < movePaths[i].path.Count; t++)   // path = List<Point>
                        {                           
                            pathPos.Add(grid[movePaths[i].path[t].x, movePaths[i].path[t].y].position);  // populate list of vector2's
                        }
                                                
                        movePaths[i].mov.SetPath(pathPos);      // set moves path
                    }
                    movePaths.RemoveAt(i);
                    doneEvents.RemoveAt(i);

                    i--;
                }
            }
            threadWorkTimer = 0;
        }

        //updates the grid's blocked nodes
        fTimerBlockedUpdate += Time.deltaTime;
        if (fTimerBlockedUpdate > timerBlockedUpdate)
        {
            fTimerBlockedUpdate = 0;
            // UpdateBlocked();
        }

    }


    // called by movables to find a path they should go to
    public static void requestPath (Movable movable)
    {
        if (movePaths.Count < 200)
        {

            // creates new manual reset event and adds it to list
            ManualResetEvent m = new ManualResetEvent(false);
            doneEvents.Add(m);

            // gets the start and end points for pathfinding
            Point start = new Point(movable.x, movable.y);
            Point end = new Point(movable.moveTarget.x, movable.moveTarget.y);

            // creates a new pathfinder object
            PathFinder t = new PathFinder(mov, pathSettings, start, end, m);

            // adds to list of pathfinders
            movePaths.Add(t);

            // adds findpath 
            ThreadPool.QueueUserWorkItem(t.FindPath, clonedGrid);

        }
        else {
            UnityEngine.Debug.Log("Too many paths");
        }
    }


    // called internally to find path
    private Point[,] findPath(Vector3 start, Vector3 finish)
    {
        Point[,] points = new Point[1, 1];

        byte[,] mGrid = (byte[,])s;

        bool stop = false;
        int maxCounter = 0;

        sbyte[,] direction = null;

        if (useDiagonals)
        {
            direction = new sbyte[8, 2] { { 0, -1 }, { 1, 0 }, { 0, 1 }, { -1, 0 }, { 1, -1 }, { 1, 1 }, { -1, 1 }, { -1, -1 } };
        }
        else {
            direction = new sbyte[4, 2] { { 0, -1 }, { 1, 0 }, { 0, 1 }, { -1, 0 } };
        }


        ushort mGridX = (ushort)(mGrid.GetUpperBound(0) + 1);

        ushort mGridY = (ushort)(mGrid.GetUpperBound(1) + 1);

        ushort mGridXMinus1 = (ushort)(mGridX - 1);

        ushort mGridYLog2 = (ushort)Mathf.Log(mGridY, 2);

        ushort currentX = 0;
        ushort currentY = 0;


        node[] mCalcGrid = new node[mGridX * mGridY];
        OrderedBag<int> open = new OrderedBag<int>(new ComparePFNodeMatrix(mCalcGrid));


        int current = (startY << mGridYLog2) + startX;
        int endLocation = (endY << mGridYLog2) + endX;

        mCalcGrid[current].g = 0;
        mCalcGrid[current].f = 2;

        mCalcGrid[current].pX = (ushort)startX;
        mCalcGrid[current].pY = (ushort)startY;

        mCalcGrid[current].status = openNode;

        int newCurrent = 0;
        ushort newCurrentX = 0;
        ushort newCurrentY = 0;
        int newG = 0;
        int newH = 0;
        open.Add(current);

        while (open.Count > 0 && !stop)
        {

            current = open.GetFirst();

            open.RemoveFirst();
            if (mCalcGrid[current].status == closedNode) continue;
            currentX = (ushort)(current & mGridXMinus1);
            currentY = (ushort)(current >> mGridYLog2);

            if (maxCounter > maxSearch)
            { //Failed to find!
                stop = true; //UnityEngine.Debug.Log("stopped");
            }

            if (current == endLocation)
            { //End found!
                mCalcGrid[current].status = closedNode; //UnityEngine.Debug.Log("found");
                pathFound = true;
                break;
            }

            for (int i = 0; i < (useDiagonals ? 8 : 4); i++)
            {

                newCurrentX = (ushort)(currentX + direction[i, 0]);
                newCurrentY = (ushort)(currentY + direction[i, 1]);
                newCurrent = (newCurrentY << mGridYLog2) + newCurrentX;

                if (newCurrentX >= mGridX || newCurrentY >= mGridY) continue;

                if (mGrid[newCurrentX, newCurrentY] == 0) continue; //if node is not walkable

                if (heavyDiagonals && i > 3)
                {
                    newG = mCalcGrid[current].g + (int)(mGrid[newCurrentX, newCurrentY] * 2.41);
                }
                else {
                    newG = mCalcGrid[current].g + mGrid[newCurrentX, newCurrentY];
                }

                //is it open or closed?

                if (mCalcGrid[newCurrent].status == openNode || mCalcGrid[newCurrent].status == closedNode)
                {
                    if (mCalcGrid[newCurrent].g <= newG) continue;
                }

                mCalcGrid[newCurrent].pX = currentX;
                mCalcGrid[newCurrent].pY = currentY;
                mCalcGrid[newCurrent].g = newG;

                switch (formula)
                {
                    default:
                    case HeuristicFormula.Manhattan:
                        newH = mHEstimate * (Mathf.Abs(newCurrentX - endX) + Mathf.Abs(newCurrentY - endY));
                        break;
                    case HeuristicFormula.MaxDXDY:
                        newH = mHEstimate * (Mathf.Max(Mathf.Abs(newCurrentX - endX), Mathf.Abs(newCurrentY - endY)));
                        break;
                    case HeuristicFormula.DiagonalShortcut:
                        int h_diagonal = Mathf.Min(Mathf.Abs(newCurrentX - endX), Mathf.Abs(newCurrentY - endY));
                        int h_straight = (Mathf.Abs(newCurrentX - endX) + Mathf.Abs(newCurrentY - endY));
                        newH = (mHEstimate * 2) * h_diagonal + mHEstimate * (h_straight - 2 * h_diagonal);
                        break;
                    case HeuristicFormula.Euclidean:
                        newH = (int)(mHEstimate * Mathf.Sqrt(Mathf.Pow((newCurrentX - endX), 2) + Mathf.Pow((newCurrentY - endY), 2)));
                        break;
                    case HeuristicFormula.EuclideanNoSQR:
                        newH = (int)(mHEstimate * (Mathf.Pow((newCurrentX - endX), 2) + Mathf.Pow((newCurrentY - endY), 2)));
                        break;
                }

                if (tieBreaker)
                {
                    int dx1 = currentX - endX;
                    int dy1 = currentY - endY;
                    int dx2 = startX - endX;
                    int dy2 = startY - endY;
                    int cross = Mathf.Abs(dx1 * dy2 - dx2 * dy1);
                    newH = (int)(newH + cross * 0.001);
                }

                mCalcGrid[newCurrent].f = newG + newH;
                open.Add(newCurrent);
                mCalcGrid[newCurrent].status = openNode;
            }
            maxCounter++;
            mCalcGrid[current].status = closedNode;
        }

        if (pathFound)
        {
            //UnityEngine.Debug.Log(maxCounter);
            path = new List<Point>();
            Point pos = new Point(currentX, currentY);
            int startNode = (startY << mGridYLog2) + startX;

            int xp = currentX;
            int yp = currentY;

            int test = 0;
            while (current != startNode)
            {

                pos = new Point(xp, yp);
                // pos = (bottomLeft + Vector2.right * xp * nodeSize + Vector2.up * yp * nodeSize); wafawefawef
                //path.Add(grid[pos.x,pos.y].position);
                path.Add(pos);

                xp = mCalcGrid[current].pX;
                yp = mCalcGrid[current].pY;
                current = (mCalcGrid[current].pY << mGridYLog2) + mCalcGrid[current].pX;
                test++;
                if (test > 3000) { UnityEngine.Debug.Log("broke"); break; }
            }

            path.Reverse();
        }
        //UnityEngine.Debug.Log(path.Count + " finish");

        //sw.Stop();
        //UnityEngine.Debug.Log(sw.Elapsed.TotalMilliseconds);

        done = true;
        _doneEvent.Set();

        return points;
    }

    internal class ComparePFNodeMatrix : IComparer<int>
    {

        node[] mMatrix;

        public ComparePFNodeMatrix(node[] matrix)
        {
            mMatrix = matrix;
        }

        public int Compare(int a, int b)
        {
            if (mMatrix[a].f > mMatrix[b].f)
                return 1;
            else if (mMatrix[a].f < mMatrix[b].f)
                return -1;
            return 0;
        }
    }
}
*/
