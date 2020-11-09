using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BottomPageSwitcher : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public BottomContainer bottomContainer;
    public ScrollRect scrollRect;

    public void OnDrag(PointerEventData data)
    {
        float difference = (data.pressPosition.x - data.position.x) / Screen.width * 900;
        if (difference > 200)
        {
            scrollRect.vertical = false;
            bottomContainer.rect.anchoredPosition = bottomContainer.viewLocation - new Vector2((difference - 200) / (Mathf.Abs(difference - 200) + 615) * 615, 0);
        }
        else if (difference < -200)
        {
            scrollRect.vertical = false;
            bottomContainer.rect.anchoredPosition = bottomContainer.viewLocation - new Vector2((difference + 200) / (Mathf.Abs(difference + 200) + 615) * 615, 0);
        }
        else
        {
            scrollRect.vertical = true;
        }
    }

    public void OnEndDrag(PointerEventData data)
    {
        float percentage = (data.pressPosition.x - data.position.x) / Screen.width;
        if (Mathf.Abs(percentage) >= bottomContainer.percentThreshold)
        {
            if (percentage > 0)
            {
                if (bottomContainer.tabGroup.currentTab < bottomContainer.tabGroup.tabs.Length - 1)
                    bottomContainer.tabGroup.tabs[bottomContainer.tabGroup.currentTab + 1].SwitchToThisTab(bottomContainer);
                else
                    DOTween.To(() => bottomContainer.rect.anchoredPosition, x => bottomContainer.rect.anchoredPosition = x, bottomContainer.viewLocation, 0.75f)
                .SetEase(Ease.OutCubic)
                .OnComplete(() => { scrollRect.vertical = true; });
            }
            else if (percentage < 0)
            {
                if (bottomContainer.tabGroup.currentTab > 0)
                    bottomContainer.tabGroup.tabs[bottomContainer.tabGroup.currentTab - 1].SwitchToThisTab(bottomContainer);
                else
                    DOTween.To(() => bottomContainer.rect.anchoredPosition, x => bottomContainer.rect.anchoredPosition = x, bottomContainer.viewLocation, 0.75f)
                .SetEase(Ease.OutCubic)
                .OnComplete(() => { scrollRect.vertical = true; });
            }
            scrollRect.vertical = true;
        }
        else
        {
            DOTween.To(() => bottomContainer.rect.anchoredPosition, x => bottomContainer.rect.anchoredPosition = x, bottomContainer.viewLocation, 0.75f)
                .SetEase(Ease.OutCubic)
                .OnComplete(() => { scrollRect.vertical = true; });
        }
    }
}
