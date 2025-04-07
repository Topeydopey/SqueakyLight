using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;



public class Kaaching : MonoBehaviour
{
    private string folderPath = "screenshot";
    public Image panelImage;
    public Image Grid;
    private float startAlpha = 1f;
    private float endAlpha = 0f;
    private float duration = 1f;
    private float timeElapsed;
    private bool Flashing;

    void Start()
    {
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
    }


    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Grid.gameObject.SetActive(false);
            Invoke("TakeScreenshot", 0.1f);
            timeElapsed = 0f;
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            Grid.gameObject.SetActive(true);
            Flashing = true;
        }


        if (Flashing == true && timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;

            float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, timeElapsed / duration);

            Color color = panelImage.color;
            color.a = currentAlpha;
            panelImage.color = color;
        }
        else if (Flashing == true && timeElapsed >= duration)
        {
            Flashing = false;
        }
    }


    void TakeScreenshot()
    {
        string screenshotPath = $"{folderPath}/Screenshot_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png";
        ScreenCapture.CaptureScreenshot(screenshotPath);
    }
}
