// AI-assisted: written by Claude (Anthropic) at the student's request.
using System.Collections;
using UnityEngine;

public class CherryController : MonoBehaviour
{
    public Sprite cherrySprite;
    public Transform pac;
    public float speed = 4f;
    public float spawnDelay = 5f;

    readonly Vector3 levelCentre = new Vector3(13.5f, -14f, 0f);

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnDelay);
            yield return StartCoroutine(RunCherry());
        }
    }

    IEnumerator RunCherry()
    {
        Camera cam = Camera.main;
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;
        float offscreenRadius = Mathf.Sqrt(halfWidth * halfWidth + halfHeight * halfHeight) + 1f;

        float angle = Random.Range(0f, Mathf.PI * 2f);
        Vector3 dir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
        Vector3 start = levelCentre + dir * offscreenRadius;
        Vector3 end = levelCentre - dir * offscreenRadius;

        GameObject cherry = new GameObject("Cherry");
        SpriteRenderer sr = cherry.AddComponent<SpriteRenderer>();
        sr.sprite = cherrySprite;
        sr.sortingOrder = 5;
        cherry.transform.position = start;

        float duration = Vector3.Distance(start, end) / speed;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cherry.transform.position = Vector3.Lerp(start, end, elapsed / duration);
            if (Vector2.Distance(cherry.transform.position, pac.position) < 0.6f)
            {
                GameManager.Instance.OnCherryEaten();
                break;
            }
            yield return null;
        }
        Destroy(cherry);
    }
}
