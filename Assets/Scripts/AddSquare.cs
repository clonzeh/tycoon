using UnityEngine;
using System.Collections;

public class AddSquare : MonoBehaviour {

    public Rigidbody grassTile;
    public Rigidbody desertTile;
    public Rigidbody waterTile;
    public Rigidbody roadTile;

	// Use this for initialization
	void Start ()
    {
        var x = -64;
        var z = -64;
        Rigidbody currentTile = null;

        while (z <= 64)
        {
            var v3 = new Vector3(x, 0, z);
            var tileInt = Random.Range(0, 4);

            switch (tileInt)
            {
                case 0: currentTile = grassTile;  break;
                case 1: currentTile = desertTile; break;
                case 2: currentTile = waterTile; break;
                case 3: currentTile = roadTile; break;
            }

            Instantiate(currentTile, v3, transform.rotation);
            x++;
            if (x == 64)
            {
                x = -64;
                z++;
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
