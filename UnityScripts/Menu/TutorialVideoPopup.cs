using UnityEngine;
using UnityEngine.Video;

public class TutorialVideoPopup : MonoBehaviour
{
    public GameObject popup;
    public VideoPlayer videoPlayer;

    public void OpenPopup()
    {
        popup.SetActive(true);

        if (videoPlayer != null)
        {
            videoPlayer.time = 0;
            videoPlayer.Play();
        }

        popup.transform.SetAsLastSibling();
    }

    public void ClosePopup()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }

        popup.SetActive(false);
    }
}