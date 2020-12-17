using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NextScene : MonoBehaviour
{
    public Button playBtn;
    public AudioSource clickSound;

    void Start()
    {
        playBtn.GetComponent<Button>().onClick.AddListener(OnClick);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnClick()
    {
        clickSound.Play();
        StartCoroutine(LoadNewScene());
    }

    IEnumerator LoadNewScene()
    {
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene("Main");
    }
}
