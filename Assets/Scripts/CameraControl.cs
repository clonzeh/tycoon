using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {

    public Camera cam;
    public GameObject target;

    private GameObject focusTile = null;
    private Color focusColor = Color.magenta;

    public float moveUnits;

    // Use this for initialization
    void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
        
        // setFocusTile();

        //
        if (Input.GetButton("Horizontal"))
        {

            if (Input.GetAxis("Horizontal") > 0)
            {
                cam.transform.Translate(moveUnits * 1, 0, 0);
            }
            else
            {
                cam.transform.Translate(moveUnits * -1, 0, 0);
            }

        } else if (Input.GetButton("Vertical"))
        {
            
            float yFactor = Mathf.Cos(60 * Mathf.Deg2Rad);
            float zFactor = Mathf.Sin(60 * Mathf.Deg2Rad);

            if (Input.GetAxis("Vertical") > 0)
            {
                cam.transform.Translate(0, moveUnits * yFactor, moveUnits * zFactor);
            }
            else
            {
                cam.transform.Translate(0, -moveUnits * yFactor, -moveUnits * zFactor);
            }
        }
        
    }

    // sets the tile currently focused on
    GameObject setFocusTile()
    {
        RaycastHit hit;

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit))
        {
            // hit.collider.GetComponentInParent<Renderer>().material.color = Color.red;
            GameObject target = hit.collider.gameObject;
            if (target.name == "rocktile(Clone)")
            {
                if (focusTile != null)
                {
                    focusTile.GetComponentInParent<Renderer>().material.color = focusColor;
                }
                focusTile = target;
                focusColor = target.GetComponentInParent<Renderer>().material.color;
                focusTile.GetComponentInParent<Renderer>().material.color = Color.red;
                return focusTile;
            }
        }

        focusTile = null;
        return null;
    }
}
