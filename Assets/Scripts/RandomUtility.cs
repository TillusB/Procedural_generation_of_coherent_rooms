using UnityEngine;
using System.Collections;

public class RandomUtility : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public Vector3 RandomVector(float min, float max)
    {
        return new Vector3(Random.Range(min, max), 1, Random.Range(min, max));
    }

    public Vector3 RandomDirection()
    {
        int switchCase = (int)Random.Range((int)0f, (int)3f);
        switch (switchCase)
        {
            case 0:
                return new Vector3(-1, 0, 0); //left
                break;
            case 1:
                return new Vector3(1, 0, 0); //right
            case 2:
                return new Vector3(0, 0, -1); //down
            case 3:
                return new Vector3(0, 0, 1); // up
        }
        throw new System.Exception("Error, no direction gotten");
    }

    
}
