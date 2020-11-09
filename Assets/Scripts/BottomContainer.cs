using UnityEngine;

public class BottomContainer : MonoBehaviour
{
    public RectTransform rect;
    public Vector2 viewLocation;
    public float percentThreshold = 0.3333f;
    public TabGroup tabGroup;

    void Start()
    {
        viewLocation = rect.anchoredPosition;
    }
}
