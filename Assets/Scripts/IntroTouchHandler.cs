using DG.Tweening;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class IntroTouchHandler : MonoBehaviour, IDragHandler, IPointerDownHandler, IEndDragHandler, IPointerClickHandler
{
    private bool introAlreadyStarting = false;
    private bool introAlreadyEnding = false;
    private bool isDrag = false;
    private float dragThreshold = 12.5f;
    private Vector2 viewLocation;
    private Vector2 viewLocationDot;
    public float percentThreshold = 0.2f;
    public RectTransform introContainer;
    public Intro intro;
    public RectTransform currentPageDot;
    public TMP_Text tapToContinueLabel;
    
    void Start()
    { 
        viewLocation = introContainer.anchoredPosition;
        viewLocationDot = currentPageDot.anchoredPosition;
    }

    public void OnPointerDown(PointerEventData data)
    {
        isDrag = false;
        if (intro.currentIntroPage == 4)
            tapToContinueLabel.DOColor(new Color(1.0f, 1.0f, 1.0f, 0.0f), 0.75f).SetEase(Ease.InOutCubic);
    }

    public void OnDrag(PointerEventData data)
    {
        float dragDistance = Vector2.Distance(data.pressPosition, data.position);
        if (dragDistance > dragThreshold)
            isDrag = true;
        if (intro.introStarted) { 
            float difference = (data.pressPosition.x - data.position.x) / Screen.width * 900;
            float differenceDot = difference / 820f * 54.666f;
            introContainer.anchoredPosition = viewLocation - new Vector2((difference / (Mathf.Abs(difference) + 1230)) * 1230, 0);
            currentPageDot.anchoredPosition = viewLocationDot + new Vector2((differenceDot / (Mathf.Abs(differenceDot) + 81.999f)) * 81.999f, 0);
        }   
    }
    public void OnEndDrag(PointerEventData data)
    {
        if (intro.introStarted)
        {
            float percentage = (data.pressPosition.x - data.position.x) / Screen.width;
            if (Mathf.Abs(percentage) >= percentThreshold)
            {
                Vector2 newLocation = viewLocation;
                Vector2 newLocationDot = viewLocationDot;
                if (percentage > 0 && intro.currentIntroPage < 4)
                {
                    newLocation -= new Vector2(820, 0);
                    newLocationDot += new Vector2(54.666f, 0);
                    intro.currentIntroPage++;
                    if (intro.currentIntroPage == 4)
                        Invoke("showTapToContinue", 7.5f);
                }
                else if (percentage < 0 && intro.currentIntroPage > 0)
                {
                    newLocation += new Vector2(820, 0);
                    newLocationDot -= new Vector2(54.666f, 0);
                    intro.currentIntroPage--;
                }
                DOTween.To(() => introContainer.anchoredPosition, x => introContainer.anchoredPosition = x, newLocation, 0.5f);
                DOTween.To(() => currentPageDot.anchoredPosition, x => currentPageDot.anchoredPosition = x, newLocationDot, 0.5f);
                viewLocation = newLocation;
                viewLocationDot = newLocationDot;
            }
            else
            {
                DOTween.To(() => introContainer.anchoredPosition, x => introContainer.anchoredPosition = x, viewLocation, 0.5f);
                DOTween.To(() => currentPageDot.anchoredPosition, x => currentPageDot.anchoredPosition = x, viewLocationDot, 0.5f);
            }
        }
    }

    public void showTapToContinue()
    {
        tapToContinueLabel.DOColor(new Color(1.0f, 1.0f, 1.0f, 0.333333f), 0.75f).SetEase(Ease.InOutCubic);
    }

    public void OnPointerClick(PointerEventData data)
    {
        if (isDrag)
            return;
        else
        {
            if (intro.introStarted == false && !introAlreadyStarting)
            {
                introAlreadyStarting = true;
                intro.IntroPhase0();
            }
            if (intro.currentIntroPage == 4 && !introAlreadyEnding)
            {
                introAlreadyEnding = true;
                intro.IntroPhase2();
            }
        }
    }
}
