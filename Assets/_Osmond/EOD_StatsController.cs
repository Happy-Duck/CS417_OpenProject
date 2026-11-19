using System.Collections.Generic;
using UnityEngine;

public class EOD_StatsController : MonoBehaviour
{
    public EOD_StatDisplay[] StatDisplays;
    public float[] DisplayTargetNumbers;
    //public Dictionary<EOD_StatDisplay, float> DisplayToTargetNumberMapping;

    private int currentBox = 0;

    private void Start()
    {
        StartNextStatTicking();
    }

    public void StartNextStatTicking()
    {
        if (currentBox >= StatDisplays.Length)
        {
            return;
        }
        StatDisplays[currentBox].StartStatTicking(DisplayTargetNumbers[currentBox]);
        currentBox++;
    }
}
