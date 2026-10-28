// AI-assisted: written by Claude (Anthropic) at the student's request.
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadLevel1()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void LoadStart()
    {
        SceneManager.LoadScene("StartScene");
    }
}
