using UnityEngine;
using System.Collections;

public class MouseInput : MonoBehaviour {

    private Game game;

    // tile highlighting
    private GameObject focusTile = null;
    private Color focusColor;

    void Awake()
    {
        game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
    }


    // Use this for initialization
    void Start ()
    {

	}

    // Update is called once per frame
    void Update()
    {
        setFocusTile();

        if (Input.GetMouseButton(1))
        {
            game.rightClick();
        }
    }

    void FixedUpdate()
    {

    }

    // sets the tile currently focused on
    GameObject setFocusTile()
    {
        RaycastHit hit = getMouseRaycastHit();

        if (hit.collider)
        {

            GameObject target = hit.collider.gameObject;

            if (target == focusTile)
            {
                return focusTile;  // if target is the same as focustile, then same focustile is still highlighted
            }
            else
            {
                // target changed so revert focustile back to normal, make sure focustile isn't null like in startup
                if (focusTile != null)
                {
                    if (focusTile.name == "drone")
                    {
                        focusTile.GetComponentsInChildren<Renderer>()[2].material.color = focusColor;
                    } else
                    {
                        focusTile.GetComponent<Renderer>().material.color = focusColor;
                    }
                } else
                {

                    if (target.name == "drone")
                    {
                        focusColor = target.GetComponentsInChildren<Renderer>()[2].material.color;

                        focusTile = target;
                        focusTile.GetComponentsInChildren<Renderer>()[2].material.color = Color.yellow;
                        return focusTile;
                    }
                    else
                    {                        
                        focusColor = target.GetComponent<Renderer>().material.color;

                        focusTile = target;
                        focusTile.GetComponent<Renderer>().material.color = Color.yellow;
                        return focusTile;
                    }

                }
            }
        }

        focusTile = null;
        return null;
    }

    public static RaycastHit getMouseRaycastHit()
    {
        Vector3 input = Input.mousePosition;
        Vector3 point = Camera.main.ScreenToWorldPoint(input);

        RaycastHit hit;
        Physics.Raycast(point, Camera.main.transform.forward, out hit);

        return hit;
    }
}


