using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WaitAndLoadScene : MonoBehaviour
{
    [SerializeField] private float waitTime;
    [SerializeField] private string nextScene;

    private float loadTime;
    private bool isLoading;

    private Animator fade;

    void Awake()
    {
        fade = GameObject.FindGameObjectWithTag("Fade").GetComponent<Animator>();

        loadTime = Time.time + waitTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLoading && Time.time > waitTime)
        {
            isLoading = true;
            StartCoroutine(LoadNextScene());
        }
    }

    private IEnumerator LoadNextScene()
    {
        fade.Play("FadeOut");

        // Wait a bit
        yield return new WaitForSeconds(.55f);

        // Load next scene
        SceneManager.LoadScene(nextScene);
    }
}
