using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;


public class ProgressBar : Image
{
    public Image fill;
    public Image counter;
    public TMP_Text counterText;
    public int threshold;
}