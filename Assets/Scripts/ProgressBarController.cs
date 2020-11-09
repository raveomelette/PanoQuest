using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ProgressBarController : MonoBehaviour
{
    public Queue<Sequence> SequenceQueue = new Queue<Sequence>();
    public int currentProgress;
    public int currentBar = 0;
    public Color colorDone;
    public Color colorCounter;
    public Color colorLocked;
    public ProgressBar[] bars;
    Sequence pbSequence;

    int previousThreshold(int bar)
    {
        if (bar == 0)
            return 0;
        else
            return bars[bar-1].threshold;
    }

    int findBar(float value)
    {
        int bar = 0;
        while (bar < bars.Length && !(previousThreshold(bar) <= value && bars[bar].threshold > value))
            bar++;
        return bar;
    }

    public void updateBars()
    {
        for (int i = 0; i < bars.Length; i++)
        {
            if (currentProgress < bars[i].threshold)
            {
                if (currentProgress >= previousThreshold(i))
                {
                    bars[i].fillAmount = (currentProgress - previousThreshold(i)) / (float)(bars[i].threshold - previousThreshold(i));
                    currentBar = i;
                }
            }
            else
            {
                bars[i].fillAmount = 1.0f;
                bars[i].counter.color = colorCounter;
                currentBar = i;
            }
        }
    }

    public void changeCurrentProgress(int value)
    {
        if ((currentProgress + value) < 0)
            value = -currentProgress;
        else if ((currentProgress + value) > bars[bars.Length-1].threshold)
            value = bars[bars.Length - 1].threshold-currentProgress;
        int searchBar = currentBar;
        int targetBar = findBar(currentProgress + value);
        while (searchBar != targetBar)
        {
            if (searchBar < targetBar)
            {
                int workBar = searchBar;
                QueueAnim(DOTween.To(() => bars[workBar].fillAmount, x => bars[workBar].fillAmount = x, 1.0f, 0.1f));
                QueueAnim(DOTween.To(() => bars[workBar].counter.color, x => bars[workBar].counter.color = x, colorCounter, 0.25f));
                searchBar++;
            }
            else
            {
                int workBar = searchBar;
                QueueAnim(DOTween.To(() => bars[workBar].counter.color, x => bars[workBar].counter.color = x, colorLocked, 0.1f));
                QueueAnim(DOTween.To(() => bars[workBar].fillAmount, x => bars[workBar].fillAmount = x, 0.0f, 0.1f));
                searchBar--;
            }
        }
        if (searchBar < bars.Length)
        {
            float newFillAmount = (currentProgress + value - previousThreshold(searchBar)) / (float)(bars[searchBar].threshold - previousThreshold(searchBar));

            currentBar = searchBar;
            if ((currentProgress + value) < bars[searchBar].threshold)
                QueueAnim(DOTween.To(() => bars[searchBar].counter.color, x => bars[searchBar].counter.color = x, colorLocked, 0.1f));
            QueueAnim(DOTween.To(() => bars[searchBar].fillAmount, x => bars[searchBar].fillAmount = x, newFillAmount, 0.1f));
        }
        else currentBar = searchBar - 1;
        currentProgress += value;
    }

    public void InitProgressBar(QuestList questList)
    {
        for (int i = 0; i < bars.Length; i++)
        {
            bars[i].fill.color = colorDone;
            bars[i].counter.color = colorLocked;
            bars[i].threshold = questList.milestones[i];
            bars[i].counterText.text = bars[i].threshold.ToString();
        }
    }

    public void QueueAnim(Tween tween)
    {
        var sequence = DOTween.Sequence();
        sequence.Pause();
        sequence.Append(tween);
        SequenceQueue.Enqueue(sequence);
        if (SequenceQueue.Count == 1)
            SequenceQueue.Peek().Play();
        sequence.OnComplete(OnCompleteQueuedAnim);
    }

    private void OnCompleteQueuedAnim()
    {
        SequenceQueue.Dequeue();
        if (SequenceQueue.Count > 0)
            SequenceQueue.Peek().Play();
    }
}
