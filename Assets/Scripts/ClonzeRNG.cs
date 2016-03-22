using UnityEngine;
using System.Collections;

public class ClonzeRNG : MonoBehaviour {

    public GameObject grassTile;
    public GameObject desertTile;
    public GameObject waterTile;
    public GameObject roadTile;

    public Transform mapContainer;
    public int xPos = 0;
    public int yPos = 0;
    public float seed = 0;

    float fTimer = 0;
    float timer = 2; //every 2 seconds

    public float[] freq;// 600 170 70 25 10
    public float[] amp;// 0.6 0.2 0.1 0.07 0.03

    // Use this for initialization
    void Start () {
        for (int gridX = -64; gridX <= 64; gridX++)
        {
            for (int gridY = -64; gridY <= 64; gridY++)
            {

                Vector3 v3 = new Vector3(gridX, 0, gridY);

                GameObject currentTile = null;
                var tileInt = Random.Range(0, 4);

                switch (tileInt)
                {
                    case 0: currentTile = grassTile; break;
                    case 1: currentTile = desertTile; break;
                    case 2: currentTile = waterTile; break;
                    case 3: currentTile = roadTile; break;
                }

                GameObject obj = (GameObject)Instantiate(currentTile, v3, transform.rotation);
                obj.transform.rotation = Quaternion.FromToRotation(obj.transform.up, -obj.transform.forward);
                obj.transform.parent = mapContainer;

                float height = 0;
                for (int i = 0; i < freq.Length; i++)
                {
                    var f = freq[i];
                    var a = amp[i];
                    height += Mathf.PerlinNoise((seed * 1000f + gridX + 1 * xPos) / f, (seed * 1000f + gridY + 1 * yPos) / f) * a;
                }
                Color clr = Color.green;

                if (height > 0.55f) { clr = new Color(0, 0.5f, 0, 1); }
                if (height > 0.67f) { clr = Color.grey; }
                if (height > 0.75f) { clr = Color.white; }


                if (height < 0.42f) { clr = Color.yellow; }
                if (height < 0.4f) { clr = Color.blue; }
                if (height < 0.35f) { clr = new Color(0, 0, 0.5f, 1); }
                //set the material color of your tile here
                obj.transform.GetComponent<Renderer>().material.color = clr;
            }
        }
    }

    void UpdateMap()
    {
        foreach (Transform t in mapContainer)
        {
            float height = 0;
            for (int i = 0; i < freq.Length; i++)
            {
                var f = freq[i];
                var a = amp[i];
                height += Mathf.PerlinNoise((seed * 1000f + t.position.x + 1 * xPos) / f, (seed * 1000f + t.position.z + 1 * yPos) / f) * a;
            }
            Color clr = Color.green;

            if (height > 0.55f) { clr = new Color(0, 0.5f, 0, 1); }
            if (height > 0.67f) { clr = Color.grey; }
            if (height > 0.75f) { clr = Color.white; }
            if (height < 0.42f) { clr = Color.yellow; }
            if (height < 0.4f) { clr = Color.blue; }
            if (height < 0.35f) { clr = new Color(0, 0, 0.5f, 1); }

            t.transform.GetComponent<Renderer>().material.color = clr;
        }
    }

    // Update is called once per frame
    void Update () {
        fTimer += Time.deltaTime;
        if (fTimer > 2)
        {
            fTimer = 0;
            // UpdateMap();
        }
    }
}
