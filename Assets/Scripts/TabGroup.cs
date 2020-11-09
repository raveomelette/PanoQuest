using UnityEngine;

public class TabGroup : MonoBehaviour
{
    public int currentTab = 0;
    public Tab[] tabs;
    public RectTransform SCImage;

    private void Start()
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            tabs[i].tabNumber = i;
            tabs[i].tabGroup = this;
        }
    }

    
}
