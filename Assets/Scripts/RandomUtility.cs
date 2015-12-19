using UnityEngine;
using System.Collections;

public class RandomUtility : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    /// <summary>
    /// Returns a random position within given minimum and maximum.
    /// </summary>
    /// <param name="min">Minimum value</param>
    /// <param name="max">Maximum value</param>
    /// <returns>Vector3</returns>
    public Vector3 RandomVector(float min, float max)
    {
        return new Vector3(Random.Range(min, max), 1, Random.Range(min, max));
    }
    /// <summary>
    /// Retruns right, left, up, down randomly.
    /// </summary>
    /// <returns>Vector3</returns>
    public Vector3 RandomDirectionLRUD()
    {
        int switchCase = (int)Random.Range((int)0f, (int)4f);
        switch (switchCase)
        {
            case 0:
                Debug.Log("left");
                return new Vector3(-.1f, 0, 0); //left
            case 1:
                Debug.Log("right");
                return new Vector3(.1f, 0, 0); //right
            case 2:
                Debug.Log("down");
                return new Vector3(0, 0, -.1f); //down
            case 3:
                Debug.Log("up");
                return new Vector3(0, 0, .1f); // up
        }
        throw new System.Exception("Error, no direction gotten");
    }

    
}
