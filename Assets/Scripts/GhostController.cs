// AI-assisted: written by Claude (Anthropic) at the student's request.
using System.Collections.Generic;
using UnityEngine;

public class GhostController : MonoBehaviour
{
    public enum Mode { Normal, Scared, Recovering, Dead }

    public int behaviour;
    public PacStudentController pac;
    public Vector3 spawn;
    public Mode mode = Mode.Normal;

    Animator animator;
    bool inHouse = true;
    bool isLerping;
    Vector3 lerpStart;
    Vector3 lerpTarget;
    float lerpElapsed;
    float lerpDuration;
    Vector2Int facing = Vector2Int.zero;
    readonly Queue<Vector3> exitPath = new Queue<Vector3>();
    string playingState = "";

    static readonly Vector2Int[] Dirs = { new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(-1, 0) };

    public bool IsDead => mode == Mode.Dead;

    void Start()
    {
        animator = GetComponent<Animator>();
        BuildExitPath();
    }

    void BuildExitPath()
    {
        exitPath.Clear();
        int exitX = transform.position.x < 13.5f ? 13 : 14;
        exitPath.Enqueue(new Vector3(exitX, -14, 0));
        exitPath.Enqueue(new Vector3(exitX, -13, 0));
        exitPath.Enqueue(new Vector3(exitX, -12, 0));
        exitPath.Enqueue(new Vector3(exitX, -11, 0));
        inHouse = true;
    }

    public void ResetToSpawn()
    {
        transform.position = spawn;
        mode = Mode.Normal;
        isLerping = false;
        facing = Vector2Int.zero;
        BuildExitPath();
    }

    public void Die()
    {
        mode = Mode.Dead;
        isLerping = false;
    }

    void Update()
    {
        if (mode == Mode.Dead)
        {
            transform.position = Vector3.MoveTowards(transform.position, spawn, 8f * Time.deltaTime);
            PlayState("Dog1_Dead");
            if (transform.position == spawn)
            {
                mode = Mode.Normal;
                facing = Vector2Int.zero;
                BuildExitPath();
            }
            return;
        }

        if (GameManager.Instance != null && !GameManager.Instance.InputEnabled) return;

        if (isLerping)
        {
            lerpElapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(lerpStart, lerpTarget, lerpElapsed / lerpDuration);
            if (lerpElapsed < lerpDuration) return;
            transform.position = lerpTarget;
            isLerping = false;
        }

        if (inHouse)
        {
            if (exitPath.Count == 0) inHouse = false;
            else StartLerp(exitPath.Dequeue());
        }
        if (!inHouse && !isLerping) ChooseNextTile();
        UpdateAnimation();
    }

    void StartLerp(Vector3 target)
    {
        Vector3 delta = target - transform.position;
        if (delta.sqrMagnitude > 0.01f)
            facing = new Vector2Int(Mathf.RoundToInt(Mathf.Sign(delta.x) * (Mathf.Abs(delta.x) > 0.5f ? 1 : 0)),
                                    Mathf.RoundToInt(Mathf.Sign(delta.y) * (Mathf.Abs(delta.y) > 0.5f ? 1 : 0)));
        lerpStart = transform.position;
        lerpTarget = target;
        lerpElapsed = 0f;
        lerpDuration = mode == Mode.Normal ? 0.25f : 0.35f;
        isLerping = true;
    }

    bool Walkable(Vector2Int tile)
    {
        int col = tile.x;
        int row = -tile.y;
        if (row == 14 && (col <= 5 || col >= 22)) return false;
        return pac.IsWalkableTile(col, row);
    }

    void ChooseNextTile()
    {
        Vector2Int here = GameManager.TileOf(transform.position);
        Vector2Int back = -facing;
        var options = new List<Vector2Int>();
        foreach (var d in Dirs)
            if (d != back && Walkable(here + d)) options.Add(d);
        if (options.Count == 0 && Walkable(here + back)) options.Add(back);
        if (options.Count == 0) return;

        Vector2Int chosen;
        float currentDist = Vector2.Distance(transform.position, pac.transform.position);
        int effective = mode == Mode.Normal ? behaviour : 1;

        if (effective == 4)
        {
            chosen = options[0];
            Vector2Int left = new Vector2Int(-facing.y, facing.x);
            Vector2Int right = new Vector2Int(facing.y, -facing.x);
            foreach (var d in new[] { left, facing, right, back })
                if (options.Contains(d)) { chosen = d; break; }
        }
        else if (effective == 1 || effective == 2)
        {
            var filtered = options.FindAll(d =>
            {
                float dist = Vector2.Distance((Vector2)(here + d), pac.transform.position);
                return effective == 1 ? dist >= currentDist : dist <= currentDist;
            });
            var pool = filtered.Count > 0 ? filtered : options;
            chosen = pool[Random.Range(0, pool.Count)];
        }
        else
        {
            chosen = options[Random.Range(0, options.Count)];
        }

        StartLerp(new Vector3(here.x + chosen.x, here.y + chosen.y, 0));
    }

    void UpdateAnimation()
    {
        string dir = facing.y > 0 ? "Up" : facing.y < 0 ? "Down" : facing.x < 0 ? "Left" : "Right";
        switch (mode)
        {
            case Mode.Normal: PlayState("Dog1_Walk_" + dir); break;
            case Mode.Scared: PlayState("Dog1_Scared_" + dir); break;
            case Mode.Recovering: PlayState("Dog1_Recovering"); break;
        }
    }

    void PlayState(string state)
    {
        if (state == playingState) return;
        animator.Play(state);
        playingState = state;
    }
}
