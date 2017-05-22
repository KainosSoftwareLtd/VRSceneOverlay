using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicTracker : MonoBehaviour
{
    public AudioSource audioSource = new AudioSource();
    public GameObject hat;
    public float delay = 0.1f;
    private Vector3 unit = new Vector3(0.01f, 0.01f, 0.01f);
    private float changeTime = 0;
    int i = 0;
    bool onTheWayUp = true;

    void Start()
    {
        audioSource.clip = Microphone.Start(null,
                    loop: true,
                    lengthSec: 1,
                    frequency: 4096
                    );
        audioSource.loop = true;
        while (!(Microphone.GetPosition(null) > 0)) { }
        audioSource.Play();
    }

    private void Update()
    {
        if (MicLoudness() > 0.01f)
        {
            ScalePulse();
            if (changeTime <= 0)
            {
                changeTime = delay;
            }
            else
                changeTime -= Time.deltaTime;
        }
        else
        {
            if (changeTime <= 0)
            {
                NormalSize();
            }
            else
                changeTime -= Time.deltaTime;
        }
    }

    private float MicLoudness()
    {
        int sampleWindow = 128;
        float levelMax = 0;
        float[] waveData = new float[sampleWindow];
        int micPosition = Microphone.GetPosition(null) - (sampleWindow + 1);

        if (micPosition < 0)
            return 0;

        audioSource.clip.GetData(waveData, micPosition);

        for (int i = 0; i < sampleWindow; i++)
        {
            float wavePeak = waveData[i] * waveData[i];
            if (levelMax < wavePeak)
                levelMax = wavePeak;
        }
        return levelMax;
    }

    private void ScalePulse()
    {
        if (i < 60 && onTheWayUp)
        {
            i++;
            hat.transform.localScale += unit;
            onTheWayUp = !(i == 60);
        }
        else if (i > 0 && !onTheWayUp)
        {
            i--;
            hat.transform.localScale -= unit;
            onTheWayUp = (i == 0);
        }
    }
    private void NormalSize()
    {
        if (i > 0)
        {
            i--;
            hat.transform.localScale -= unit;
            onTheWayUp = (i == 0);
        }
    }
}
