using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Tab : MonoBehaviour
{
    public int tabNumber;
    public Image bg;
    public TMP_Text label;
    public string tabName;
    public TabGroup tabGroup;

    private void Start()
    {
        label.text = tabName;
    }

    public void SwitchToThisTab(BottomContainer bottomContainer)
    {
        if (tabGroup.currentTab != tabNumber)
        {
            Vector2 newPosition;
            Vector2 newSCPosition;

            newPosition = bottomContainer.viewLocation - new Vector2(900 * (tabNumber - tabGroup.currentTab), 0);
            newSCPosition = new Vector2(260 * (tabNumber-1), 0);
            tabGroup.currentTab = tabNumber;
            DOTween.To(() => bottomContainer.rect.anchoredPosition, x => bottomContainer.rect.anchoredPosition = x, newPosition, 0.75f)
                .SetEase(Ease.OutCubic);
            DOTween.To(() => tabGroup.SCImage.anchoredPosition, x => tabGroup.SCImage.anchoredPosition = x, newSCPosition, 0.75f)
                .SetEase(Ease.OutCubic);
            bottomContainer.viewLocation = newPosition;
        }
    }
}
