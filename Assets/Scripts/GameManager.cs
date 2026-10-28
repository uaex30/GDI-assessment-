// AI-assisted: written by Claude (Anthropic) at the student's request.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public PacStudentController pac;
    public Animator[] ghosts;
    public Text scoreText;
    public Text timerText;
    public Text scaredText;
    public Text countdownText;
    public Text gameOverText;
    public Image[] lifeImages;

    public AudioSource sfx;
    public AudioSource music;
    public AudioClip normalMusic;
    public AudioClip scaredMusic;
    public AudioClip deathClip;
    public AudioClip eatGhostClip;
    public AudioClip cherryClip;

    public bool InputEnabled { get; private set; }

    readonly Dictionary<Vector2Int, GameObject> pellets = new Dictionary<Vector2Int, GameObject>();
    readonly Vector3 pacSpawn = new Vector3(1, -1, 0);
    int score;
    int lives = 3;
    float gameTime;
    bool timerRunning;
    float scaredTimeLeft;
    int ghostCombo;
    GhostController[] ghostCtrls;
    bool gameOver;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ghostCtrls = new GhostController[ghosts.Length];
        for (int i = 0; i < ghosts.Length; i++)
        {
            ghostCtrls[i] = ghosts[i].gameObject.AddComponent<GhostController>();
            ghostCtrls[i].behaviour = i + 1;
            ghostCtrls[i].pac = pac;
            ghostCtrls[i].spawn = ghosts[i].transform.position;
        }
        foreach (Transform t in GameObject.Find("Level").GetComponentsInChildren<Transform>())
        {
            if (t.name.StartsWith("Pellet") || t.name.StartsWith("PowerPellet"))
                pellets[TileOf(t.position)] = t.gameObject;
        }
        scaredText.gameObject.SetActive(false);
        gameOverText.gameObject.SetActive(false);
        UpdateScore();
        StartCoroutine(RoundStart());
    }

    public static Vector2Int TileOf(Vector3 position)
    {
        return new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
    }

    public static string FormatTime(float seconds)
    {
        int minutes = (int)(seconds / 60f);
        int secs = (int)seconds % 60;
        int hundredths = (int)(seconds * 100f) % 100;
        return string.Format("{0:00}:{1:00}:{2:00}", minutes, secs, hundredths);
    }

    public bool HasPellet(Vector3 position)
    {
        return pellets.ContainsKey(TileOf(position));
    }

    IEnumerator RoundStart()
    {
        InputEnabled = false;
        countdownText.gameObject.SetActive(true);
        foreach (string step in new[] { "3", "2", "1", "GO!" })
        {
            countdownText.text = step;
            yield return new WaitForSeconds(1f);
        }
        countdownText.gameObject.SetActive(false);
        InputEnabled = true;
        timerRunning = true;
    }

    void Update()
    {
        if (gameOver) return;

        if (timerRunning)
        {
            gameTime += Time.deltaTime;
            timerText.text = FormatTime(gameTime);
        }

        if (scaredTimeLeft > 0f)
        {
            scaredTimeLeft -= Time.deltaTime;
            scaredText.text = "Scared: " + Mathf.CeilToInt(Mathf.Max(scaredTimeLeft, 0f));
            if (scaredTimeLeft <= 3f && scaredTimeLeft + Time.deltaTime > 3f)
                SetGhostMode(GhostController.Mode.Recovering);
            if (scaredTimeLeft <= 0f) EndScared();
        }

        if (InputEnabled) CheckGhostCollisions();
    }

    public void OnPacArrived(Vector3 position)
    {
        Vector2Int tile = TileOf(position);
        if (!pellets.TryGetValue(tile, out GameObject pellet)) return;

        pellets.Remove(tile);
        bool power = pellet.name.StartsWith("PowerPellet");
        Destroy(pellet);
        score += power ? 50 : 10;
        UpdateScore();
        if (power) StartScared();
        if (pellets.Count == 0) StartCoroutine(GameOver());
    }

    public void OnCherryEaten()
    {
        score += 100;
        UpdateScore();
        sfx.PlayOneShot(cherryClip);
    }

    void UpdateScore()
    {
        scoreText.text = score.ToString("000000");
    }

    void StartScared()
    {
        scaredTimeLeft = 10f;
        ghostCombo = 0;
        scaredText.gameObject.SetActive(true);
        SetGhostMode(GhostController.Mode.Scared);
        if (music.clip != scaredMusic)
        {
            music.clip = scaredMusic;
            music.loop = true;
            music.Play();
        }
    }

    void EndScared()
    {
        scaredTimeLeft = 0f;
        scaredText.gameObject.SetActive(false);
        SetGhostMode(GhostController.Mode.Normal);
        music.clip = normalMusic;
        music.loop = true;
        music.Play();
    }

    void SetGhostMode(GhostController.Mode mode)
    {
        foreach (GhostController ghost in ghostCtrls)
            if (!ghost.IsDead) ghost.mode = mode;
    }

    void CheckGhostCollisions()
    {
        foreach (GhostController ghost in ghostCtrls)
        {
            if (ghost.IsDead) continue;
            if (Vector2.Distance(ghost.transform.position, pac.transform.position) > 0.6f) continue;

            if (ghost.mode == GhostController.Mode.Scared || ghost.mode == GhostController.Mode.Recovering)
            {
                ghostCombo++;
                int points = 300 * ghostCombo;
                score += points;
                StartCoroutine(ShowComboPopup(ghost.transform.position, points));
                UpdateScore();
                sfx.PlayOneShot(eatGhostClip);
                ghost.Die();
            }
            else
            {
                StartCoroutine(PacDies());
                return;
            }
        }
    }

    // Design innovation: each ghost eaten in the same scared period is worth more
    // (300, 600, 900, 1200), shown as a popup that rises from the eaten ghost.
    IEnumerator ShowComboPopup(Vector3 worldPosition, int points)
    {
        GameObject popup = new GameObject("ComboPopup", typeof(RectTransform), typeof(Text), typeof(Outline));
        popup.transform.SetParent(scoreText.canvas.transform, false);
        Text text = popup.GetComponent<Text>();
        text.font = scoreText.font;
        text.fontSize = 48;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.color = new Color(1f, 0.85f, 0.2f);
        text.text = "x" + ghostCombo + "  +" + points;
        RectTransform rt = popup.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(400, 80);

        Vector3 start = Camera.main.WorldToScreenPoint(worldPosition);
        for (float t = 0f; t < 1f; t += Time.deltaTime)
        {
            rt.position = start + Vector3.up * 80f * t;
            text.color = new Color(1f, 0.85f, 0.2f, 1f - t);
            yield return null;
        }
        Destroy(popup);
    }

    IEnumerator PacDies()
    {
        InputEnabled = false;
        timerRunning = false;
        lives--;
        lifeImages[lives].enabled = false;
        sfx.PlayOneShot(deathClip);
        pac.PlayDeath();
        yield return new WaitForSeconds(2f);

        if (lives <= 0)
        {
            StartCoroutine(GameOver());
            yield break;
        }
        pac.ResetTo(pacSpawn);
        foreach (GhostController ghost in ghostCtrls) ghost.ResetToSpawn();
        StartCoroutine(RoundStart());
    }

    IEnumerator GameOver()
    {
        gameOver = true;
        InputEnabled = false;
        timerRunning = false;
        gameOverText.gameObject.SetActive(true);

        int bestScore = PlayerPrefs.GetInt("HighScore", 0);
        float bestTime = PlayerPrefs.GetFloat("BestTime", float.MaxValue);
        if (score > bestScore || (score == bestScore && gameTime < bestTime))
        {
            PlayerPrefs.SetInt("HighScore", score);
            PlayerPrefs.SetFloat("BestTime", gameTime);
            PlayerPrefs.Save();
        }

        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("StartScene");
    }
}
