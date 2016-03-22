using UnityEngine;
using System.Collections.Generic;

public class Movable {

    public int x;
    public int y;
    public GameObject go;
    public Node node;
    public Point moveTarget;

    // pathfinding
    public static PathFinding pathfinding;
    private List<Vector2> path = null;

    // moving
    public float speed = 5;
    public bool moving = false;
    private Vector3 targetPos;

    public Movable(int _x, int _y)
    {
        go = GameObject.FindGameObjectWithTag("movable");
        x = _x;
        y = _y;
    }

    void Awake()
    {
        pathfinding = GameObject.FindGameObjectWithTag("pathfinding").transform.GetComponent<PathFinding>();
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    // starts an object in motion
    public void startMoving()
    {

        RaycastHit hit = MouseInput.getMouseRaycastHit();

        if (hit.collider)
        {
            moving = true;

            GameObject targetTile = hit.collider.gameObject;
            targetPos = targetTile.transform.position;
            targetPos.y = 1.5f;
        }
    }

    //
    public void startMoving2()
    {

    }


    // moves an object towards its target
    public bool moveObject()
    {
        RaycastHit hit = MouseInput.getMouseRaycastHit();
        float t = Time.deltaTime;
        Vector3 targetDir;

        targetDir = targetPos - go.transform.position;
        float distance = speed * t;
        float distanceToTarget = targetDir.sqrMagnitude;
        targetDir.Normalize();
        if (distanceToTarget < distance * distance)
        {
            go.GetComponent<Rigidbody>().MovePosition(targetPos);
            moving = false;
            return true;
        }
        else
        {
            go.GetComponent<Rigidbody>().MovePosition(go.transform.position + speed * (targetDir * Time.deltaTime));
            return false;
        }
    }


    public void moveObject2()
    {

    }


    //
    public void RequestPath()
    {
        RaycastHit hit = MouseInput.getMouseRaycastHit();

        if (hit.collider)
        {
            moving = true;

            GameObject targetTile = hit.collider.gameObject;
            targetPos = targetTile.transform.position;
            targetPos.y = 1.5f;
        }
        // updatePath = 0;
        pathfinding.AddPathRequest(this);
    }


    // 
    public void SetPath(List<Vector2> p)
    {
        // currentOrder = p.Count > 1 ? 1 : 0;
        path = p;
    }
}
