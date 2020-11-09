using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UnityEngine.Events;
using System;

public class DetailedQuest : MonoBehaviour
{
    public string id;
    public TMP_Text label;
    public GameObject invisibleToggle;
    public Image arrow;
    public GameObject body;
    public TMP_Text description;
    public TMP_Text deadline;
    public Image rewardbox;
    public int points = 0;
    public TMP_Text rewardPointsLabel;
    public Toggle checkbox;
    public ProgressBarController pbController;

    public void Check()
    {
        if (checkbox.isOn)
        {
            if (rewardbox.gameObject.activeSelf)
            {
                rewardbox.DOColor(pbController.colorLocked, 0.2f);
                pbController.changeCurrentProgress(points);
            }
            rewardPointsLabel.DOColor(new Color(1.0f, 1.0f, 1.0f, 0.5f), 0.2f);
            arrow.DOColor(new Color(1.0f, 1.0f, 1.0f, 0.5f), 0.2f);
            label.DOColor(new Color(1.0f, 1.0f, 1.0f, 0.5f), 0.2f);
            deadline.DOColor(new Color(1.0f, 1.0f, 1.0f, 0.5f), 0.2f);
            PlayerPrefs.SetInt(id, 1);
        }
        else
        {
            if (rewardbox.gameObject.activeSelf)
            {
                rewardbox.DOColor(pbController.colorDone, 0.2f);
                pbController.changeCurrentProgress(-points);
            }
            rewardPointsLabel.DOColor(Color.white, 0.2f);
            arrow.DOColor(Color.white, 0.2f);
            label.DOColor(Color.white, 0.2f);
            deadline.DOColor(Color.white, 0.2f);
            PlayerPrefs.SetInt(id, 0);
        }
    }
}
