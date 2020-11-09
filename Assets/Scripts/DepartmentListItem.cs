using DG.Tweening;
using TMPro;
using UnityEngine;

public class DepartmentListItem : MonoBehaviour
{
    public Intro intro;
    public TMP_Text deptName;
    public QuestList deptList;

    public void SetDeptList()
    {
        PlayerPrefs.SetString("department", deptList.listName);
        intro.InitCustomLists(deptList);
        Sequence sequence = DOTween.Sequence(); 
        sequence.Append(DOTween.To(() => intro.deptListRT.anchorMin, x => intro.deptListRT.anchorMin = x, new Vector2(0, 0.83275f), 1f).SetEase(Ease.InOutCubic))                
                .Insert(0.25f, intro.deptListChooseLabel.DOColor(new Color(1.0f, 1.0f, 1.0f, 0.0f), 0.5f).SetEase(Ease.InOutCubic))
                .OnComplete(() => {
                    intro.deptListChooseLabel.gameObject.SetActive(false);
                    Destroy(intro.deptListChooseLabel.gameObject);
                    intro.deptListLE.gameObject.SetActive(false);
                    Destroy(intro.deptListLE.gameObject);
                    intro.IntroPhase1();
                });
    }
}
