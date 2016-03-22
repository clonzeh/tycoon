using UnityEngine;
using System.Collections.Generic;
using Wintellect.PowerCollections;
using System.Threading;

public class PathFinding : MonoBehaviour
{

    public enum HeuristicFormula { Manhattan, MaxDXDY, DiagonalShortcut, Euclidean, EuclideanNoSQR };

    public byte[,] clonedGrid = null;
    public int gridX = 0;
    public int gridY = 0;
    public float nodeSize = 1f;
    public PathSettings pathSettings;
    public CGrid[,] grid;
    public LayerMask wall;
    //CGrid[,] grid;
    public float characterSize = 1f;
    public Vector2 gridBoxSize = Vector2.zero;
    Vector2 adjustedSize = Vector2.zero;
    Vector2 bottomLeft = Vector2.zero;
    public Vector3 nodeDrawSize = new Vector3(0.05f, 0.05f, 0);              //might need to be changed **************
    public bool DrawGizmos = false;
    public bool UpdateInEdit = false;
    Thread[] threads;
    public List<ManualResetEvent> doneEvents = new List<ManualResetEvent>();
    public List<PathFinder> arraytest = new List<PathFinder>();
    float threadWorkTimer;
    public int gridPowerOfSize = 5;
    float fTimerBlockedUpdate = 0;
    public float timerBlockedUpdate = 0.3f;

    public struct CGrid
    {
        public Vector2 position;              //might need to be changed **************
        public byte blocked;

        public CGrid(Vector2 pos)
        {              //might need to be changed **************
            position = pos;
            blocked = 0;
        }
    }

    public struct node
    {
        public int f;
        public int g;
        public ushort pX;
        public ushort pY;              //might need to be changed **************
        public byte status;
    }

    [System.Serializable]
    public class PathSettings
    {

        public HeuristicFormula formula = HeuristicFormula.DiagonalShortcut;
        public int maxSearch = 30000;
        public bool useDiagonals = true;
        public bool heavyDiagonals = true;
        public bool tieBreaker = true;
        public int mHEstimate = 2;
        public bool diagonalNeighbors = true;
    }

    //Called when a character is requesting a new path    
    public void AddPathRequest(Movable mov)
    {
        if (arraytest.Count < 200)
        {

            ManualResetEvent m = new ManualResetEvent(false);
            doneEvents.Add(m);

            Point st = new Point(
                Mathf.RoundToInt((mov.go.transform.position.x - bottomLeft.x) / nodeSize),
                Mathf.RoundToInt((mov.go.transform.position.y - bottomLeft.y) / nodeSize));
            Point en = new Point(
                Mathf.RoundToInt((mov.moveTarget.x - bottomLeft.x) / nodeSize),
                Mathf.RoundToInt((mov.moveTarget.y - bottomLeft.y) / nodeSize));

            PathFinder t = new PathFinder(mov, pathSettings, st, en, m);

            arraytest.Add(t);
            ThreadPool.QueueUserWorkItem(t.FindPath, clonedGrid);

        }
        else {
            UnityEngine.Debug.Log("Too many paths");
        }
    }

    void Start()
    {

        CreateGrid();
    }

    void Update()
    {

        //makes threads do work
        threadWorkTimer += Time.deltaTime;
        if (threadWorkTimer > 0.2f)
        {
            for (int i = 0; i < arraytest.Count; i++)
            {
                if (arraytest[i].done)
                {

                    if (arraytest[i].pathFound)
                    {
                        List<Vector2> pathPos = new List<Vector2>();
                        for (int t = 0; t < arraytest[i].path.Count; t++)
                        {
                            pathPos.Add(grid[arraytest[i].path[t].x, arraytest[i].path[t].y].position);
                        }

                        arraytest[i].mov.SetPath(pathPos);
                    }
                    arraytest.RemoveAt(i);
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
            UpdateBlocked();
        }

    }

    public byte DetectCollisions(Vector2 position)
    {

        float _charSize = characterSize * 0.5f;
        Vector2 bottomLeftCorner = -Vector2.right * _charSize - Vector2.up * _charSize;              //might need to be changed **************
        Vector2 topRightCorner = Vector2.right * _charSize + Vector2.up * _charSize;             //might need to be changed **************
        return Physics2D.OverlapArea(position + bottomLeftCorner, position + topRightCorner, wall) ? (byte)0 : (byte)1; //if overlap .. blocked == 0               //might need to be changed **************
    }

    void CreateGrid()
    {

        int[] sizes = new int[] { 2, 4, 16, 32, 64, 128, 256 };
        if (!(gridBoxSize.x < 0 || gridBoxSize.y < 0))
        {              //might need to be changed **************

            gridX = sizes[gridPowerOfSize];
            gridY = sizes[gridPowerOfSize];              //might need to be changed **************
            adjustedSize.x = gridX * nodeSize;
            adjustedSize.y = gridY * nodeSize;              //might need to be changed **************

            grid = new CGrid[gridX, gridY];
            clonedGrid = new byte[gridX, gridY];

            //might need to be changed **************
            bottomLeft = (Vector2)transform.position - Vector2.right * (adjustedSize.x * 0.5f - nodeSize * 0.5f) - Vector2.up * (adjustedSize.y * 0.5f - nodeSize * 0.5f);

            for (int x = 0; x < gridX; x++)
            {
                for (int y = 0; y < gridY; y++)
                {
                    grid[x, y] = new CGrid(bottomLeft + Vector2.right * x * nodeSize + Vector2.up * y * nodeSize);
                    grid[x, y].blocked = DetectCollisions(grid[x, y].position);
                    clonedGrid[x, y] = grid[x, y].blocked;
                }
            }
        }
    }

    void UpdateBlocked()
    {
        for (int x = 0; x < gridX; x++)
        {
            for (int y = 0; y < gridY; y++)
            {
                //grid[x, y] = new CGrid(bottomLeft + Vector2.right * x * nodeSize + Vector2.up * y * nodeSize);
                grid[x, y].blocked = DetectCollisions(grid[x, y].position);
                clonedGrid[x, y] = grid[x, y].blocked;
            }
        }
    }

    void OnDrawGizmos()
    {

        Gizmos.DrawWireCube(transform.position, (Vector3)adjustedSize);
        if (DrawGizmos)
        {

            if (grid != null)
            {
                foreach (CGrid node in grid)
                {

                    Gizmos.color = node.blocked == 0 ? Color.red : Color.blue;

                    Gizmos.DrawCube(node.position, nodeDrawSize);
                }
            }
        }
    }

    public struct Point { public int x; public int y; public Point(int _x, int _y) { x = _x; y = _y; } }

    public class PathFinder
    {

        public Movable mov;
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

        public PathFinder(Movable _mov, PathSettings p, Point Start, Point End, ManualResetEvent _m)
        {

            mov = _mov;
            _doneEvent = _m;


            formula = p.formula;
            maxSearch = p.maxSearch;
            useDiagonals = p.useDiagonals;
            heavyDiagonals = p.heavyDiagonals;
            tieBreaker = p.tieBreaker;
            mHEstimate = p.mHEstimate;

            diagonalNeighbors = p.diagonalNeighbors;


            startX = Start.x;
            startY = Start.y;

            endX = End.x;
            endY = End.y;

        }

        public void FindPath(object s)
        {

            // Stopwatch sw = new Stopwatch();
            //sw.Start();

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
                            // type could be Y?
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
                    if (test > 3000) { 
                        //UnityEngine.Debug.Log("broke");
                        break; }
                }

                path.Reverse();
            }
            //UnityEngine.Debug.Log(path.Count + " finish");

            //sw.Stop();
            //UnityEngine.Debug.Log(sw.Elapsed.TotalMilliseconds);

            done = true;
            _doneEvent.Set();
        }
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