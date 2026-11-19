using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

public class EOD_StatDisplay : MonoBehaviour
{
    public UnityEvent FinishedTicking;

    [SerializeField]
    private TMP_Text TextBox;

    [SerializeField]
    private float maxNumSpeedThreshold = 100000;

    [SerializeField]
    private float secondTickDelay = 0.01f;

    [SerializeField]
    private float maxTickingDuration = 5f;

    private float currentAmount = 0;
    private float TargetNumber = 0;
    //private float amountPerTick = 0;

    private void Start()
    {
        // Goal: Display 10k Doubloons in 2.5 seconds
        // That's 4000 doubloons per second
        // Which, with 0.1s per tick, is 400 doubloons per tick
        // So we do 10k / (2.5 / 0.1) = 10k / 25 = 400

        //float amountPerTick = TargetNumber / (maxTickingDuration / secondTickDelay);
        //StartCoroutine(TickBoxText(secondTickDelay, amountPerTick));
        //StartStatTicking(10000);
    }

    public void StartStatTicking(float targetNumber)
    {
        TargetNumber = targetNumber;
        float amountPerTick = TargetNumber / (maxTickingDuration / secondTickDelay);
        StartCoroutine(TickBoxText(secondTickDelay, amountPerTick));
    }

    private IEnumerator TickBoxText(float secondDelay, float amountPerTick)
    {
        while (currentAmount < TargetNumber)
        {
            currentAmount += amountPerTick;
            TextBox.text = Mathf.Round(currentAmount).ToString();
            yield return new WaitForSeconds(secondDelay);
        }

        FinishedTicking.Invoke();
        yield return null;
    }
}
