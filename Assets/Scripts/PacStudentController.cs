// AI-assisted: the movement, animation, audio, dust, tunnel and collision code in this file
// was written by Claude (Anthropic) at the student's request. levelMap is the student's data.
using UnityEngine;
using UnityEngine.InputSystem;

public class PacStudentController : MonoBehaviour
{
    int[,] levelMap =
    {
        {1,2,2,2,2,2,2,2,2,2,2,2,2,7},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,4},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,4},
        {2,6,4,0,0,4,5,4,0,0,0,4,5,4},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,3},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,5},
        {2,5,3,4,4,3,5,3,3,5,3,4,4,4},
        {2,5,3,4,4,3,5,4,4,5,3,4,4,3},
        {2,5,5,5,5,5,5,4,4,5,5,5,5,4},
        {1,2,2,2,2,1,5,4,3,4,4,3,0,4},
        {0,0,0,0,0,2,5,4,3,4,4,3,0,3},
        {0,0,0,0,0,2,5,4,4,0,0,0,0,0},
        {0,0,0,0,0,2,5,4,4,0,3,4,4,8},
        {2,2,2,2,2,1,5,3,3,0,4,0,0,0},
        {0,0,0,0,0,0,5,0,0,0,4,0,0,0},
    };

    char lastInput = ' ';
    char currentInput = ' ';
    bool isLerping = false;
    Vector3 lerpStart;
    Vector3 lerpTarget;
    float lerpElapsed = 0f;
    float lerpDuration = 0.2f;

    public AudioClip moveClip;
    public AudioClip pelletClip;
    public AudioClip wallClip;
    public ParticleSystem dust;
    public ParticleSystem wallHitEffect;

    Animator animator;
    AudioSource moveAudio;
    AudioSource pelletAudio;
    char animatedDirection = ' ';
    bool wasMoving = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.speed = 0f;

        moveAudio = GetComponent<AudioSource>();
        moveAudio.clip = moveClip;
        moveAudio.loop = true;
        moveAudio.playOnAwake = false;

        pelletAudio = gameObject.AddComponent<AudioSource>();
        pelletAudio.clip = pelletClip;
        pelletAudio.playOnAwake = false;
    }

    public void PlayPelletSound()
    {
        pelletAudio.PlayOneShot(pelletClip);
    }

    public void PlayDeath()
    {
        isLerping = false;
        moveAudio.Stop();
        if (dust != null) dust.Stop();
        animator.speed = 1f;
        animator.Play("Rabbit_Dead");
        animatedDirection = ' ';
    }

    public void ResetTo(Vector3 position)
    {
        transform.position = position;
        isLerping = false;
        lastInput = ' ';
        currentInput = ' ';
        wasMoving = false;
        animator.Play("Rabbit_Walk_Right");
        animator.speed = 0f;
        animatedDirection = ' ';
    }

    bool CanMove()
    {
        return GameManager.Instance == null || GameManager.Instance.InputEnabled;
    }

    void Update()
    {
        if (!CanMove())
        {
            if (moveAudio.isPlaying) moveAudio.Stop();
            if (dust != null && dust.isPlaying) dust.Stop();
            return;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.wKey.wasPressedThisFrame) lastInput = 'w';
            if (keyboard.aKey.wasPressedThisFrame) lastInput = 'a';
            if (keyboard.sKey.wasPressedThisFrame) lastInput = 's';
            if (keyboard.dKey.wasPressedThisFrame) lastInput = 'd';
        }

        if (isLerping)
        {
            lerpElapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(lerpStart, lerpTarget, lerpElapsed / lerpDuration);
            if (lerpElapsed >= lerpDuration)
            {
                transform.position = lerpTarget;
                isLerping = false;
                if (GameManager.Instance != null) GameManager.Instance.OnPacArrived(lerpTarget);
                if (!CanMove()) return;
            }
        }

        if (!isLerping)
        {
            if (TryStartMove(lastInput))
            {
                currentInput = lastInput;
            }
            else if (!TryStartMove(currentInput) && wasMoving)
            {
                pelletAudio.PlayOneShot(wallClip);
                if (wallHitEffect != null) wallHitEffect.Play();
            }
        }

        wasMoving = isLerping;
        UpdateAnimationAndAudio();
    }

    void UpdateAnimationAndAudio()
    {
        if (isLerping)
        {
            if (currentInput != animatedDirection)
            {
                animator.Play(WalkState(currentInput));
                animatedDirection = currentInput;
            }
            animator.speed = 1f;
            if (!moveAudio.isPlaying) moveAudio.Play();
            if (dust != null && !dust.isPlaying) dust.Play();
        }
        else
        {
            animator.speed = 0f;
            if (moveAudio.isPlaying) moveAudio.Stop();
            if (dust != null && dust.isPlaying) dust.Stop();
        }
    }

    void ChooseMoveClip()
    {
        bool towardsPellet = GameManager.Instance != null && GameManager.Instance.HasPellet(lerpTarget);
        AudioClip clip = towardsPellet ? pelletClip : moveClip;
        if (moveAudio.clip != clip)
        {
            moveAudio.clip = clip;
            moveAudio.Play();
        }
    }

    string WalkState(char input)
    {
        switch (input)
        {
            case 'w': return "Rabbit_Up_Walk";
            case 'a': return "Rabbit_Walk_Left";
            case 's': return "Rabbit_Down_Walk";
            default: return "Rabbit_Walk_Right";
        }
    }

    bool TryStartMove(char input)
    {
        Vector2Int dir = Direction(input);
        if (dir == Vector2Int.zero) return false;

        int col = Mathf.RoundToInt(transform.position.x);
        int row = Mathf.RoundToInt(-transform.position.y);

        if (row == 14 && (col + dir.x < 0 || col + dir.x > 27))
        {
            col = col + dir.x < 0 ? 27 : 0;
            transform.position = new Vector3(col, -row, transform.position.z);
            return true;
        }

        if (!IsWalkable(col + dir.x, row + dir.y)) return false;

        lerpStart = new Vector3(col, -row, transform.position.z);
        lerpTarget = new Vector3(col + dir.x, -(row + dir.y), transform.position.z);
        lerpElapsed = 0f;
        isLerping = true;
        ChooseMoveClip();
        return true;
    }

    Vector2Int Direction(char input)
    {
        switch (input)
        {
            case 'w': return new Vector2Int(0, -1);
            case 'a': return new Vector2Int(-1, 0);
            case 's': return new Vector2Int(0, 1);
            case 'd': return new Vector2Int(1, 0);
            default: return Vector2Int.zero;
        }
    }

    public bool IsWalkableTile(int col, int row)
    {
        return IsWalkable(col, row);
    }

    bool IsWalkable(int col, int row)
    {
        if (col > 13) col = 27 - col;
        if (row > 14) row = 28 - row;

        if (col < 0 || col > 13 || row < 0 || row > 14) return false;

        int tile = levelMap[row, col];
        return tile == 0 || tile == 5 || tile == 6;
    }
}
