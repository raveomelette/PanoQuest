using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEditor;

public class DetailedQuestBody : LayoutElement
{

    public RectTransform rt;

    protected override void Start()
    {
        base.Start();
        this.preferredHeight = 0;
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
    }

    public void CollapseQuestDetails(Image arrow)
    {
        if (this.preferredHeight != 0)
        {
            DOTween.To(() => this.preferredHeight, x => this.preferredHeight = x, 0f, 0.5f);
            arrow.transform.DORotate(new Vector3(0, 0, 0), 0.5f);
        }
        else
        {
            DOTween.To(() => this.preferredHeight, x => this.preferredHeight = x, rt.rect.height+20, 0.5f);
            arrow.transform.DORotate(new Vector3(0, 0, 180), 0.5f);
        }
    }

}