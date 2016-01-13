using UnityEngine;
using System.Collections;

/// <summary>
/// Room objects contain all information about a room.
/// Bound, position, collider (,renderer?), doors and neighbours.
/// </summary>
public class Room : MonoBehaviour {
    //Attributes
    public Vector3 otherRoom = Vector3.zero;
    public Vector3 size = new Vector3 (1,1,1);
    public Vector3 position = new Vector3 (0,0,0);
    public System.Collections.Generic.List<Room> neighbours = new System.Collections.Generic.List<Room>();
    public System.Collections.Generic.List<Generator.Door> doors = new System.Collections.Generic.List<Generator.Door>();
    public bool collides = false;
    private Color color;
    public Rigidbody rb;

    //Methods
    void Start () {
        transform.parent = GameObject.Find("Rooms").transform; // Root Object um alle Collider einzublenden
        gameObject.AddComponent<MeshRenderer>();
        gameObject.AddComponent<BoxCollider>();
        rb = gameObject.AddComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
        gameObject.transform.position = position;
        gameObject.transform.localScale = size;
        //gameObject.GetComponent<BoxCollider>().isTrigger = true;
        //rb.isKinematic = true;
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void Update () {
        if (!collides) color = Color.green;
        rb.velocity = Vector3.zero;
        transform.rotation = Quaternion.EulerAngles(Vector3.zero);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawCube(transform.position, size);
    }

    /*void OnTriggerEnter(Collider other)
    {
        color = Color.red;
        if (otherRoom == Vector3.zero)
        {
            otherRoom = other.transform.position;
        }
        collides = true;
    }
    void OnTriggerExit(Collider other)
    {
        color = Color.green;
        otherRoom = Vector3.zero;
        collides = false;
    }
    void OnTriggerstay(Collider other)
    {
        color = Color.red;
        if (otherRoom == Vector3.zero)
        {
            otherRoom = other.transform.position;
        }
        collides = true;
    }*/
    void OnCollisionEnter(Collision other)
    {

        color = Color.red;
        if(otherRoom == Vector3.zero)
        {
            otherRoom = other.gameObject.transform.position;
        }
        collides = true;
    }

    void OnCollisionExit(Collision other)
    {
        color = Color.green;
        otherRoom = Vector3.zero;
        collides = false;
    }
    void OnCollisionstay(Collision other)
    {
        color = Color.red;
        if (otherRoom == Vector3.zero)
        {
            otherRoom = other.transform.position;
        }
        collides = true;
    }

    /// <summary>
    /// Adds a room to this rooms neighbours array.
    /// Conditionally recursive.
    /// </summary>
    /// <param name="newNeighbour">Room that should be added as a neighbour of this Room</param>
    public void AddNeighbour(Room newNeighbour)
    {
        neighbours.Add(newNeighbour);
        if (!newNeighbour.neighbours.Contains(this))
        {
            newNeighbour.AddNeighbour(this);
        }
        else return;
    }
    /// <summary>
    /// Move this Room into given direction.
    /// Normalises direction Vector3 to make sure the room wont jump.
    /// </summary>
    /// <param name="direction">Vector3 that points in the direction this Room should move in</param>
    public void Push(Vector3 direction)
    {
        color = Color.yellow;
        transform.position += direction.normalized/100;
    }
    /// <summary>
    /// TODO: Set this Rooms type (public/private/open/???)
    /// </summary>
    /// <param name="type">TODO: Type of this Room</param>
    public void SetRoomType(string type)
    {

    }
}
