using UnityEngine;
using System.Collections.Generic;

public class Movement : MonoBehaviour {

    //debug
    public bool playerInput = false;
    public float redrawTimer = 4;
    //
    [Range(0, 0.35f)]
    public float speed = 0.2f;
    public float fixedDiagnolSpeed = 0.7071f;

    public bool angleMovement = true;
    public float turnRate = 1f;
    public float angle = 0;

    public Vector2 direction;
    public Vector2 velocity = Vector2.zero;
    public Vector2 Collision = Vector2.zero;
    public LayerMask Wall;

    BoxCollider2D box;

    public int rays = 2;
    public bool followPath = false;
    public float pathRequestLimit = 0.2f;
    float pathTimer = 0;
    public float mustUpdatePath = 0.6f;
    float updatePath = 0;

    public static PathFinding pathfinding;
    public Vector2 targetPosition = Vector2.zero;
    List<Vector2> path = new List<Vector2>();

    public bool following = false;
    public GameObject followCharacter = null;

    public bool RTSMovement = false;
    public bool drawGizmos = false;

    int currentOrder = 0;
    int pathMode = 0;

    void Awake() {
    

    }

    public class test {

        public int a = 0;
    }

    void Start () {

 
        if (pathfinding == null) {
            pathfinding = GameObject.FindGameObjectWithTag("pathfinding").transform.GetComponent<PathFinding>();
        }
        box = GetComponent<BoxCollider2D>(); //Debug.Log((box.size.x * transform.localScale.x));
	}
	
    void Update() {


        if (RTSMovement && Input.GetMouseButtonDown(1)) {
            Vector3 v3 = Input.mousePosition;
            targetPosition = Camera.main.ScreenToWorldPoint(v3); targetPosition.x = Mathf.Round(targetPosition.x); targetPosition.y = Mathf.Round(targetPosition.y);

            RequestPath(); updatePath = 0; pathTimer = 0;
            //transform.position = targetPosition;
        }

        pathTimer += Time.deltaTime;
        updatePath += Time.deltaTime;

       
        if (pathTimer >= pathRequestLimit) {

            if (!RTSMovement) {
                if (followCharacter != null) {
                    targetPosition = followCharacter.transform.position;
                    
                }
            }
             

            if (followPath && (path == null || (path != null && path.Count > 0 && Vector2.Distance( path[path.Count - 1], targetPosition ) > 1f)) ) {
                RequestPath(); //Debug.Log("requestpath");
            }
            
            pathTimer = 0;
        } 
        if (updatePath >= mustUpdatePath) {
            if (followPath) {
                RequestPath(); //Debug.Log("requestpath1");
            }
        }
        
    }

    public bool ArrowKeysMovement = false;
	void FixedUpdate () {

        direction = Vector2.zero;
        if (playerInput) {
            float h = 0;
            float v = 0;
            if (ArrowKeysMovement) {
                
                if (Input.GetKey(KeyCode.LeftArrow)) {
                    h = -1f;
                }
                if (Input.GetKey(KeyCode.RightArrow)) {
                    if (h == -1f) { h = 0; } else {
                        h = 1f;
                    }
                }
                if (Input.GetKey(KeyCode.DownArrow)) {
                    v = -1f;
                }
                if (Input.GetKey(KeyCode.UpArrow)) {
                    if (v == -1f) { v = 0; } else {
                        v = 1f;
                    }
                }
            } else {
                if (Input.GetKey(KeyCode.A)) {
                    h = -1f;
                }
                if (Input.GetKey(KeyCode.D)) {
                    if (h == -1f) { h = 0; } else {
                        h = 1f;
                    }
                }
                if (Input.GetKey(KeyCode.S)) {
                    v = -1f;
                }
                if (Input.GetKey(KeyCode.W)) {
                    if (v == -1f) { v = 0; } else {
                        v = 1f;
                    }
                }
            }

            direction = new Vector2(h, v);
        }
        if (followPath) {
            FollowPath();
        }
        Move(direction);

	}

    void Move(Vector2 dir) {
        if (angleMovement) {

            if (dir.x > 0) {
                angle -= turnRate;
            } else if (dir.x < 0) {
                angle += turnRate;
            }

            if (dir.y > 0) {
                transform.position = (Vector2)transform.position + (Vector2) (Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.right) * speed;
            }
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.right * 1.5f);


        } else {
            CheckCollision(dir);
        }
    }

    void RequestPath() {

        updatePath = 0;
        // pathfinding.AddPathRequest(this);
    }

    void CheckCollision(Vector2 dir) {

        Collision = Vector2.zero;
        Vector2 startPos = Vector2.zero;

        float amount = 0;
        float maxX = speed;
        float maxY = speed;

        if (dir.x != 0) {
            startPos.x = transform.position.x + dir.x * (box.size.x * transform.localScale.x) * 0.5f;
            startPos.y = transform.position.y + (box.size.y * transform.localScale.y) * 0.5f;

            amount = (box.size.y * transform.localScale.y) / (rays - 1);

            for (int i = 0; i < rays; i++) {

                if (i != 0) {
                    startPos.y -= amount;
                }

                RaycastHit2D hitX = Physics2D.Raycast(startPos, dir.x > 0 ? Vector2.right : -Vector2.right, speed, Wall);
                if (hitX) {
                    Collision.x = 1;
                    maxX = hitX.distance;
                    break;
                }

            }
        }
        if (dir.y != 0) {
            startPos.x = transform.position.x - (box.size.x * transform.localScale.x) * 0.5f;
            startPos.y = transform.position.y + dir.y * (box.size.y * transform.localScale.y) * 0.5f;
            amount = (box.size.x * transform.localScale.x) / (rays - 1);

            for (int i = 0; i < rays; i++) {

                if (i != 0) {
                    startPos.x += amount;
                }
                RaycastHit2D hitY = Physics2D.Raycast(startPos, dir.y > 0 ? Vector2.up : -Vector2.up, speed, Wall);
                if (hitY) {
                    Collision.y = 1;
                    maxY = hitY.distance;
                    break;
                }
            }
        }

        float s = direction.x != 0 && direction.y != 0 ? speed * fixedDiagnolSpeed : speed;
        velocity.x = maxX < speed ? maxX - 0.02f : (direction.x * s * (Collision.x == 0 ? 1 : 0));
        velocity.y = maxY < speed ? maxY - 0.02f : (direction.y * s * (Collision.y == 0 ? 1 : 0));











        transform.position = (Vector2)transform.position + velocity;
    }

    public void FollowPathWithAngle() {

    }


    void OnDrawGizmos() {
        if (drawGizmos) {
            if (path != null && path.Count > 0)
            for (int i = 0; i < path.Count; i++) {
                Gizmos.DrawCube(path[i], new Vector3(0.5f, 0.5f, 0.5f));
            }
        }
    }
    
    public void SetPath(List<Vector2> p) {


        currentOrder = p.Count > 1 ? 1 : 0;
        path = p;

    }



    Vector2 DirectionFromTo(Vector2 p1, Vector2 p2) {
        int x = 0;
        int y = 0;
        if (p1.x < p2.x) { x = 1; } else if (p1.x > p2.x) { x = -1; }
        if (p1.y < p2.y) { y = 1; } else if (p1.y > p2.y) { y = -1; }


        return new Vector2(x, y);
    }

    void FollowPath() {

        if (path != null && path.Count > 0) {

            Vector3 newPos = transform.position;
            if (Mathf.Abs(transform.position.x - path[currentOrder].x) < speed * 0.35f) {
                direction.x = 0; newPos.x = path[currentOrder].x;
            } else {

                if (transform.position.x < path[currentOrder].x) {
                    direction.x = 1;
                } else {
                    direction.x = -1;
                }
            }

            if (Mathf.Abs(transform.position.y - path[currentOrder].y) < speed * 0.35f) {
                direction.y = 0; newPos.y = path[currentOrder].y;
            } else {
                if (transform.position.y < path[currentOrder].y) {
                    direction.y = 1;
                } else {
                    direction.y = -1;
                }
            }

            if (direction.x == 0 || direction.y == 0) { transform.position = newPos; }

            if (Vector2.Distance((Vector2)transform.position, path[currentOrder]) < (speed + 0.1f) * 0.35f) {
                currentOrder++;
                if (currentOrder >= path.Count) {
                    path = null;
                    pathMode = pathMode == 0 ? 1 : 0;
                }
            }
        }
        
    }
}
