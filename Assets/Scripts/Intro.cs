using DG.Tweening;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Intro : MonoBehaviour
{
    public bool introCompleted = false;
    public bool introStarted = false;
    public int currentIntroPage;
    public RectTransform welcomeText;
    public Image panoLogo; 
    public RectTransform bg;
    public RectTransform bgContainer;
    public CanvasGroup tabGroup;
    public GameObject introTextMask;
    public TMP_Text firstIntroPage;
    public TMP_Text lastIntroPage;
    public IntroTouchHandler introTouchHandler;
    public Image[] pageDots;
    public Image currentPageDot;
    public CanvasGroup bottomFirstPage;
    public DetailedQuest detailedQuestPrefab;
    public GameObject questSeparatorPrefab;
    public DepartmentListItem departmentPrefab;
    public GameObject deptListParent;
    public GameObject QLAdaptationObject;
    public GameObject QLMentorObject;
    public GameObject QLDepartmentObject;
    public ProgressBarController pbController;
    public QuestList[] questLists;
    public QuestList adaptationList;
    public QuestList departmentList;
    public QuestList mentorList;
    public TMP_Text deptListChooseLabel;
    public GameObject deptListObject;
    public LayoutElement deptListLE;
    public ContentSizeFitter deptListCSF;
    public RectTransform deptListRT;
    public float deptListHeight;

    public void Start()
    {
        UnityEngine.Object qlTextAsset = Resources.Load<TextAsset>("Questlists/adaptation");
        adaptationList = JsonConvert.DeserializeObject<QuestList>(qlTextAsset.ToString(), new QuestDataItemConverter());
        DetailedQuest[] adaptationQuests = InitQuestList(adaptationList, QLAdaptationObject);
        if (PlayerPrefs.HasKey("department"))
        {
            qlTextAsset = Resources.Load<TextAsset>($"Questlists/DepartmentLists/{PlayerPrefs.GetString("department")}");
            if (qlTextAsset != null)
            {
                departmentList = JsonConvert.DeserializeObject<QuestList>(qlTextAsset.ToString(), new QuestDataItemConverter());
                InitCustomLists(departmentList);
                introCompleted = true;
            }
        }
        if (!introCompleted)
        {
            UnityEngine.Object[] qlTextAssets = Resources.LoadAll("Questlists/DepartmentLists/", typeof(TextAsset));
            questLists = new QuestList[qlTextAssets.Length];
            DepartmentListItem[] departmentList = new DepartmentListItem[qlTextAssets.Length];
            for (int i = 0; i < qlTextAssets.Length; i++)
            {
                questLists[i] = JsonConvert.DeserializeObject<QuestList>(qlTextAssets[i].ToString(), new QuestDataItemConverter());
                departmentList[i] = Instantiate(departmentPrefab, deptListParent.transform);
                departmentList[i].intro = this;
                departmentList[i].deptList = questLists[i];
                departmentList[i].deptName.text = questLists[i].listName;
            }
            bottomFirstPage.gameObject.SetActive(false);
            bottomFirstPage.interactable = false;
            bottomFirstPage.alpha = 0;
            introTouchHandler.gameObject.SetActive(true);
            Vector2 nullVector = new Vector2(0, 0);
            tabGroup.gameObject.SetActive(false);
            tabGroup.alpha = 0;
            bgContainer.anchorMin = nullVector;
            welcomeText.anchorMin = new Vector2(0, -0.18f);
            panoLogo.gameObject.SetActive(true);
            deptListObject.SetActive(true);
            deptListChooseLabel.gameObject.SetActive(true);
            deptListRT.anchorMin = new Vector2(0, 0.83275f);
        }
    }

    public void InitCustomLists(QuestList departmentList)
    {
        pbController.InitProgressBar(departmentList);
        DetailedQuest[] departmentQuests = InitQuestList(departmentList, QLDepartmentObject);
        TextAsset qlTextAsset = null;
        if (departmentList.hasCustomMentorList == true)
            qlTextAsset = Resources.Load<TextAsset>($"Questlists/MentorLists/{departmentList.listName}");
        if (qlTextAsset == null)
            qlTextAsset = Resources.Load<TextAsset>($"Questlists/mentor");
        mentorList = JsonConvert.DeserializeObject<QuestList>(qlTextAsset.ToString(), new QuestDataItemConverter());
        DetailedQuest[] mentorQuests = InitQuestList(mentorList, QLMentorObject);
    }


    public DetailedQuest[] InitQuestList(QuestList questList, GameObject ListParent)
    {
        bool needToUpdateBars = false;
        int questsnum = questList.quests.Count;
        int i = 0;
        DetailedQuest[] detailedQuests = new DetailedQuest[questsnum];
        foreach (KeyValuePair<int, QuestDataItem> entry in questList.quests)
        {
            detailedQuests[i] = Instantiate(detailedQuestPrefab, ListParent.transform);
            detailedQuests[i].id = questList.listName + ":" + entry.Key;
            detailedQuests[i].pbController = pbController;
            detailedQuests[i].label.text = entry.Value.name;
            if (PlayerPrefs.HasKey(detailedQuests[i].id))
                detailedQuests[i].checkbox.isOn = PlayerPrefs.GetInt(detailedQuests[i].id) == 1 ? true : false;
            if (entry.Value.features.HasFlag(QuestDataItem.QuestFeatures.Deadline))
            {
                detailedQuests[i].label.rectTransform.anchoredPosition += new Vector2(0, 20);
                detailedQuests[i].deadline.text = "Срок: " + entry.Value.deadline;
                detailedQuests[i].deadline.gameObject.SetActive(true);
            }
            if (entry.Value.features.HasFlag(QuestDataItem.QuestFeatures.Description))
            {
                detailedQuests[i].description.text = entry.Value.description;
                detailedQuests[i].body.SetActive(true);
                detailedQuests[i].arrow.gameObject.SetActive(true);
                detailedQuests[i].invisibleToggle.SetActive(true);
            }
            if (entry.Value.features.HasFlag(QuestDataItem.QuestFeatures.Points))
            {
                detailedQuests[i].points = entry.Value.points;
                detailedQuests[i].rewardPointsLabel.text = detailedQuests[i].points.ToString();
                detailedQuests[i].rewardbox.color = pbController.colorDone;
                detailedQuests[i].rewardbox.gameObject.SetActive(true);
            }
            if (entry.Value.features < (QuestDataItem.QuestFeatures)7)
            detailedQuests[i].label.rectTransform.offsetMax += new Vector2(70, 0);
            if (detailedQuests[i].checkbox.isOn)
            {
                if (detailedQuests[i].rewardbox.gameObject.activeSelf)
                {
                    needToUpdateBars = true;
                    detailedQuests[i].rewardbox.color = pbController.colorLocked;
                    if ((pbController.currentProgress + detailedQuests[i].points) < 0)
                        pbController.currentProgress = 0;
                    else if ((pbController.currentProgress + detailedQuests[i].points) > pbController.bars[pbController.bars.Length - 1].threshold)
                        pbController.currentProgress = pbController.bars[pbController.bars.Length - 1].threshold;
                    else
                        pbController.currentProgress += detailedQuests[i].points;
                }
                detailedQuests[i].rewardPointsLabel.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
                detailedQuests[i].arrow.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
                detailedQuests[i].label.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
                detailedQuests[i].deadline.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
            }
            if (i < questsnum - 1)
                Instantiate(questSeparatorPrefab, ListParent.transform);
            i++;
        }
        if (needToUpdateBars)
            pbController.updateBars();
        return detailedQuests;
    }

    public void IntroPhase0()
    {
        deptListRT.gameObject.SetActive(true);
        Sequence sequence = DOTween.Sequence();
        sequence.Append(panoLogo.DOColor(new Color(1.0f, 1.0f, 1.0f, 0.0f), 0.75f).SetEase(Ease.InOutCubic))
                .Append(DOTween.To(() => welcomeText.anchorMin, x => welcomeText.anchorMin = x, new Vector2(0.0f, 0.8885f), 1f).SetEase(Ease.InOutCubic))
                .Append(deptListChooseLabel.DOColor(new Color(1.0f, 1.0f, 1.0f, 1.0f), 0.5f).SetEase(Ease.InOutCubic))
                .Insert(2f, DOTween.To(() => deptListRT.anchorMin, x => deptListRT.anchorMin = x, new Vector2(0, 0), 1f).SetEase(Ease.InOutCubic))
                
                .OnComplete(() => {
                    panoLogo.gameObject.SetActive(false);
                    Destroy(panoLogo.gameObject);
                });
    }

    public void IntroPhase1()
    {
        introTextMask.SetActive(true);
        Sequence sequence = DOTween.Sequence();
        sequence.Append(currentPageDot.DOColor(new Color(1.0f, 1.0f, 1.0f, 0.5f), 0.5f).SetEase(Ease.InOutCubic));
        foreach (Image i in pageDots)
            sequence.Insert(0.0f, i.DOColor(new Color(0.0f, 0.0f, 0.0f, 0.5f), 0.5f).SetEase(Ease.InOutCubic));
        sequence.Insert(0.0f, firstIntroPage.DOColor(new Color(1.0f, 1.0f, 1.0f, 1.0f), 1f).SetEase(Ease.InOutCubic))
                .OnComplete(() => {
                    introStarted = true;
                });
    }

    public void IntroPhase2()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(lastIntroPage.DOColor(new Color(1.0f, 1.0f, 1.0f, 0.0f), 0.5f).SetEase(Ease.InOutCubic))
                .Insert(0, currentPageDot.DOColor(new Color(1.0f, 1.0f, 1.0f, 0.0f), 0.5f).SetEase(Ease.InOutCubic));
        foreach (Image i in pageDots)
            sequence.Insert(0, i.DOColor(new Color(0.0f, 0.0f, 0.0f, 0.0f), 0.5f).SetEase(Ease.InOutCubic));
        bottomFirstPage.gameObject.SetActive(true);
        tabGroup.gameObject.SetActive(true);
        sequence.Append(DOTween.To(() => bgContainer.anchorMin, x => bgContainer.anchorMin = x, new Vector2(0.0f, 0.8482599f), 1f).SetEase(Ease.InOutCubic))
                .Insert(1.25f, DOTween.To(() => tabGroup.alpha, x => tabGroup.alpha = x, 1f, 0.5f).SetEase(Ease.InOutCubic))
                .Insert(1.5f, DOTween.To(() => bottomFirstPage.alpha, x => bottomFirstPage.alpha = x, 1, 0.5f).SetEase(Ease.InOutCubic))
                .OnComplete(() =>
                {
                    bottomFirstPage.interactable = true;
                    introTouchHandler.gameObject.SetActive(false);
                    Destroy(introTouchHandler.gameObject);
                    introTextMask.gameObject.SetActive(false);
                    Destroy(introTextMask.gameObject);
                    introCompleted = true;
                });
    }
}

