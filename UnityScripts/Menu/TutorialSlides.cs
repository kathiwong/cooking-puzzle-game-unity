using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using TMPro;

public class TutorialSlides : MonoBehaviour
{
    [Header("References")]
    public VideoPlayer videoPlayer;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;

    [Header("Desktop / Editor Videos")]
    public VideoClip[] videos;

    [Header("WebGL URLs")]
    public string[] webUrls;

    [Header("Slide Text")]
    public string[] titles;
    public string[] descriptions;

    private int currentIndex = 0;
    private Coroutine playRoutine;

    private void Start()
    {
        ShowSlide(0);
    }

    public void ShowSlide(int index)
    {
        if (!ValidateSlideData(index))
        {
            return;
        }

        currentIndex = index;

        if (titleText != null)
            titleText.text = titles[index];

        if (descriptionText != null)
            descriptionText.text = descriptions[index];

        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
        }

        playRoutine = StartCoroutine(PrepareAndPlay(index));
    }

    private IEnumerator PrepareAndPlay(int index)
    {
        if (videoPlayer == null)
        {
            Debug.LogWarning("TutorialVideoSlides: VideoPlayer is not assigned.");
            yield break;
        }

        videoPlayer.Stop();

#if UNITY_WEBGL && !UNITY_EDITOR
        if (string.IsNullOrWhiteSpace(webUrls[index]))
        {
            Debug.LogWarning($"TutorialVideoSlides: Web URL missing at index {index}.");
            yield break;
        }

        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = webUrls[index];
#else
        if (videos == null || index >= videos.Length || videos[index] == null)
        {
            Debug.LogWarning($"TutorialVideoSlides: VideoClip missing at index {index}.");
            yield break;
        }

        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.clip = videos[index];
#endif

        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        videoPlayer.time = 0;
        videoPlayer.Play();
    }

    public void Next()
    {
        int slideCount = GetSlideCount();

        if (currentIndex < slideCount - 1)
        {
            ShowSlide(currentIndex + 1);
        }
    }

    public void Prev()
    {
        if (currentIndex > 0)
        {
            ShowSlide(currentIndex - 1);
        }
    }

    private int GetSlideCount()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return webUrls != null ? webUrls.Length : 0;
#else
        return videos != null ? videos.Length : 0;
#endif
    }

    private bool ValidateSlideData(int index)
    {
        if (titles == null || descriptions == null)
        {
            Debug.LogWarning("TutorialVideoSlides: titles or descriptions array is null.");
            return false;
        }

        int slideCount = GetSlideCount();

        if (slideCount == 0)
        {
            Debug.LogWarning("TutorialVideoSlides: no slides configured.");
            return false;
        }

        if (titles.Length != slideCount || descriptions.Length != slideCount)
        {
            Debug.LogWarning(
                $"TutorialVideoSlides: array size mismatch. " +
                $"Slides={slideCount}, Titles={titles.Length}, Descriptions={descriptions.Length}"
            );
            return false;
        }

        if (index < 0 || index >= slideCount)
        {
            Debug.LogWarning($"TutorialVideoSlides: invalid slide index {index}.");
            return false;
        }

        return true;
    }
}