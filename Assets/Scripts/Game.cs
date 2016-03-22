using UnityEngine;
using System.Collections.Generic;

public class Game : MonoBehaviour {

    public Grid grid;
    public GameObject tile;
    public MouseInput mouse;
    public Movable movable;

    void Awake()
    { //Awake happens before Start() and can avoid errors with getcomponents -- if this component is disabled,  Awake will run, Start will Not.

        //instantiate things... like GetComponent caches and what not... any time you can store a GetComponent into a variable that you'l use often, you should.
        mouse = GameObject.FindGameObjectWithTag("mouse").GetComponent<MouseInput>();
    }

    void Start()
    {
        grid.CreateGrid();
        movable = grid.node[0, 0].movable;
    }

    // Update is called once per frame
    void Update ()
    {

    }

    void FixedUpdate()
    {
        if (movable.moving)
        {
            movable.moveObject();
        }
    }

    // sphere randomizer
    public void sphereShower()
    {    
        int r = Random.Range(1, 100);
        if (r == 1)
        {
            GameObject sphere = (GameObject)Resources.Load("sphere");
            Vector3 v = new Vector3(0, 10, 0);
            Instantiate(sphere, v, sphere.transform.rotation);
        }

    }

    public void rightClick()
    {
        // movable.startMoving();
        movable.RequestPath();
    }

}

//add new class to project for Grid (helps with organization) 
//classes that don't inherit from MonoBehaviour don't have standard functions Awake() Start() Update() FixedUpdate(), doesn't make sense to have it if you're not using it

[System.Serializable] //make the class have editable properties in the inspector, constructors get complicated with this though, so best to leave them out?
public class Grid
{
    public int size = 0;    // should be even

    //grid info
    private Point gridSize = new Point(32, 32); //defaults are 32
    public Node[,] node;

    public void CreateGrid(int _sizeX = 32, int _sizeY = 32)
    {

        if (size == 0) { gridSize.x = _sizeX; } else { gridSize.x = size; }
        if (size == 0) { gridSize.y = _sizeY; } else { gridSize.y = size; }

        node = new Node[gridSize.x, gridSize.y];        

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                int xPos = x - (gridSize.x / 2);
                int yPos = y - (gridSize.y / 2);
                Vector3 v3 = new Vector3(xPos, 0, yPos);
                GameObject tile = (GameObject)Resources.Load("rocktile");
                GameObject go = (GameObject)Object.Instantiate(tile, v3, Quaternion.FromToRotation(tile.transform.forward, tile.transform.up));
                Node n = new Node(x, y);
                n.setGameObject(go);
                if (x == 0 && y == 0) { n.movable = new Movable(0, 0);  }
                node[x, y] = n;
            }
        }
    }

    public Node ReturnPosition(int x, int y)
    {
        if (node != null && x >= 0 && x < node.GetLength(0) && y >= 0 && y < node.GetLength(1))
        {
            return node[x, y];
        }
        else {

            Node fake = new Node(-1, -1);
            string s = string.Format("Error. invalid grid position return of {0} {1}", x, y);
            Debug.Log(s);
            Debug.Break();
            return fake;
        }
    }

    public void moveObjectToTile(GameObject go, int x, int y)
    {

    }
} 


public class Node
{

    public int x;
    public int y;
    public GameObject go;
    public Game game;
    public Movable movable;

    public Node(int _x, int _y)
    {
        x = _x;
        y = _y;
        game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
    }

    public void setGameObject(GameObject _go)
    {
        go = _go;
    }
}

[System.Serializable]
public struct Point
{
    public int x;
    public int y;
    public Point(int _x, int _y)
    {
        x = _x;
        y = _y;
    }
}