using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EyesAnim : MonoBehaviour
{
    public Image eye;
    public Sprite eyeOpen;
    public Sprite eyeClosed;

    private float nextBlinkTime;
    private float timeSinceLastBlink;

    void Start()
    {
        SetNextBlinkTime();
    }

    void Update()
    {
        timeSinceLastBlink += Time.deltaTime;

        if (timeSinceLastBlink >= nextBlinkTime)
        {
            CloseEyes();
            StartCoroutine(OpenEyesAfterDelay(0.2f));
            SetNextBlinkTime();
            timeSinceLastBlink = 0f;
        }
    }

    private void SetNextBlinkTime()
    {
        nextBlinkTime = Random.Range(2f, 4f); 
    }

    private IEnumerator OpenEyesAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        OpenEyes();
    }

    public void OpenEyes()
    {
        eye.sprite = eyeOpen;
    }

    public void CloseEyes()
    {
        eye.sprite = eyeClosed;
    }
}
