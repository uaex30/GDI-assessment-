using UnityEngine;

public class PacStudentMovement : MonoBehaviour
{
    public float speed = 3f;
    public AudioSource moveAudio;
    public AudioClip moveClip;

    Vector3[] waypoints;
    int currentTarget = 0;

    void Start()
    {
        waypoints = new Vector3[]
        {
            new Vector3(2f, -1f, 0f),
            new Vector3(5f, -1f, 0f),
            new Vector3(5f, -3f, 0f),
            new Vector3(2f, -3f, 0f)
        };

        if (moveAudio != null)
        {
            moveAudio.clip = moveClip;
            moveAudio.loop = true;
            moveAudio.Play();
        }
    }

    void Update()
    {
        Vector3 direction = waypoints[currentTarget] - transform.position;

        transform.position +=
            direction.normalized * speed * Time.deltaTime;

        if (Vector3.Distance(transform.position, waypoints[currentTarget]) < 0.05f)
        {
            transform.position = waypoints[currentTarget];

            currentTarget++;

            if (currentTarget >= waypoints.Length)
            {
                currentTarget = 0;
            }
        }
    }
}