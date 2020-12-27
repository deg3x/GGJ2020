using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Video : MonoBehaviour
{
    public VideoPlayer vid;
    public Texture tex;

    private void Start()
    {
        StartCoroutine(PlayVideo());
    }

    IEnumerator PlayVideo()
    {
        vid.Prepare();
        while(!vid.isPrepared)
        {
            yield return null;
        }

        this.GetComponent<RawImage>().texture = vid.texture;
        this.GetComponent<RawImage>().color = Color.white;
        vid.Play();

        while(vid.isPlaying)
        {
            yield return null;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
