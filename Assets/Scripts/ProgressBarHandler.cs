using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.ComponentModel;
using DG.Tweening;

public class ProgressBarHandler : MonoBehaviour
{
    [Serializable]
    public class BarWithThreshold
    {
        public ProgressBar bar;
        public int threshold;
    }
    private string dataPath;
    public TMP_Text headerText;
    public int barCount;
    public int current;
    private int old;
    public Color colorDone;
    public Color colorLocked;
    public GameObject questListObject;
    public QuestItem questItemPrefab;
    public GameObject separatorPrefab;
    public BarWithThreshold[] bars;
    public Dictionary<int, bool> questCompletionDictionary;
    private QuestItem[] quests;

    private void Awake()
    {
#if UNITY_ANDROID || UNITY_IOS
        dataPath = Application.persistentDataPath;
#else
        dataPath = Application.dataPath;
#endif
    }

    void Start()
    {
        QuestListData loadedQuestData = LoadQuests();
        headerText.text = loadedQuestData.role + " PanoQuest";
        ColorUtility.TryParseHtmlString("#"+loadedQuestData.color, out colorDone);
        for (int i = 0; i < bars.Length; i++)
        {
            bars[i].bar.fill.color = colorDone;
            bars[i].bar.counter.color = colorLocked;
            bars[i].threshold = loadedQuestData.milestones[i];
            bars[i].bar.counterText.text = bars[i].threshold.ToString();
        }
        quests = new QuestItem[loadedQuestData.quests.Length];
        questCompletionDictionary = Load();
        for (int i = 0; i < loadedQuestData.quests.Length; i++)
        {
            quests[i] = Instantiate(questItemPrefab, questListObject.transform);
            quests[i].pbar = this;
            quests[i].pointsbox.color = colorDone;
            quests[i].check.color = colorDone;
            quests[i].ID = loadedQuestData.quests[i].ID;
            quests[i].description.text = loadedQuestData.quests[i].text;
            quests[i].points = loadedQuestData.quests[i].points;
            quests[i].pointsLabel.text = quests[i].points.ToString();
            if (questCompletionDictionary.ContainsKey(loadedQuestData.quests[i].ID))
            {
                quests[i].checkbox.isOn = questCompletionDictionary[quests[i].ID];
            }
            else
            {
                questCompletionDictionary.Add(loadedQuestData.quests[i].ID, false);
            }
            if (i < loadedQuestData.quests.Length-1)
                Instantiate(separatorPrefab, questListObject.transform);
        }

        foreach (BarWithThreshold i in bars)
        {
            i.bar.fill.color = colorDone;
        }
    }

    void Update()
    {
        GetCurrentFill();
    }

    void GetCurrentFill()
    {
        if (old != current)
        {
            int previousThreshold = 0;
            int oldBar = -1;
            int currentBar = -1;
            if (old >= 0 && old <= bars[0].threshold)
                oldBar = 0;
            if (current >= 0 && current <= bars[0].threshold)
                currentBar = 0;
            if (oldBar == -1 || currentBar == -1)
            {
                for (int i=1; i < bars.Length; i++)
                {
                    if (old > bars[i - 1].threshold && old <= bars[i].threshold)
                        oldBar = i;
                    if (current > bars[i - 1].threshold && current <= bars[i].threshold)
                        currentBar = i;
                }
                if (oldBar == -1)
                    oldBar = bars.Length - 1;
                if (currentBar == -1)
                    currentBar = bars.Length - 1;
            }
            
            Sequence mySequence = DOTween.Sequence();
            
            if (old < current)
            {
                for (int i = oldBar; i <= currentBar; i++)
                {
                    if (i > 0) previousThreshold = bars[i - 1].threshold;
                    float newFillAmount = (float)(current - previousThreshold) / (float)(bars[i].threshold - previousThreshold);
                    if (newFillAmount >= 1.0f)
                    {
                        int paintBar = i;
                        mySequence.Append(DOTween.To(() => bars[paintBar].bar.fillAmount, x => bars[paintBar].bar.fillAmount = x, 1.0f, 0.1f));
                        mySequence.Append(DOTween.To(() => bars[paintBar].bar.counter.color, x => bars[paintBar].bar.counter.color = x, colorDone, 0.1f));
                    }
                    else
                    {
                        int paintBar = i;
                        mySequence.Append(DOTween.To(() => bars[paintBar].bar.fillAmount, x => bars[paintBar].bar.fillAmount = x, newFillAmount, 0.2f));
                    }
                }
            }
            else
            {
                for (int i = oldBar; i >= currentBar; i--)
                {
                    if (i > 0) previousThreshold = bars[i - 1].threshold;
                    else previousThreshold = 0;
                    float newFillAmount = (float)(current - previousThreshold) / (float)(bars[i].threshold - previousThreshold);
                    if (newFillAmount <= 1.0f)
                    {
                        int paintBar = i;
                        mySequence.Append(DOTween.To(() => bars[paintBar].bar.counter.color, x => bars[paintBar].bar.counter.color = x, colorLocked, 0.1f));
                    }
                    if (newFillAmount <= 0.0f)
                    {
                        int paintBar = i;
                        mySequence.Append(DOTween.To(() => bars[paintBar].bar.fillAmount, x => bars[paintBar].bar.fillAmount = x, 0.0f, 0.1f));
                    }
                    else
                    {
                        int paintBar = i;
                        mySequence.Append(DOTween.To(() => bars[paintBar].bar.fillAmount, x => bars[paintBar].bar.fillAmount = x, newFillAmount, 0.2f));
                    }
                }
            }
        }
        old = current;
    }

    public void Save()
    {
        QuestProgressSaveData[] questProgressCurrent = new QuestProgressSaveData[questCompletionDictionary.Count];
        int counter = 0;
        foreach (KeyValuePair<int, bool> questEntry in questCompletionDictionary) 
        {
            questProgressCurrent[counter] = new QuestProgressSaveData
            {
                ID = questEntry.Key,
                complete = questEntry.Value
            };
            counter++;
        }
        RoleSaveData[] roleSaveData = new RoleSaveData[]
        {
            new RoleSaveData
            {
                role = headerText.text,
                quests = questProgressCurrent
            }
        };
        SaveObject saveObject = new SaveObject
        {
            version = 1,
            roles = roleSaveData
        };
        string saveJson = JsonUtility.ToJson(saveObject);
        Debug.Log(saveJson);

        File.WriteAllText(dataPath + "/save.txt", saveJson);
    }

    private Dictionary<int, bool> Load()
    {
        Dictionary<int, bool> loadedProgress = new Dictionary<int, bool>();
        if (File.Exists(dataPath + "/save.txt"))
        {
            string saveString = File.ReadAllText(dataPath + "/save.txt");
            SaveObject saveObject = JsonUtility.FromJson<SaveObject>(saveString);
            Debug.Log("Save file loaded:");

            foreach (QuestProgressSaveData i in saveObject.roles[0].quests)
            {
                loadedProgress.Add(i.ID, i.complete);
                Debug.Log($"Loaded progress in quest {i.ID}: {i.complete}");
            }
        }
        else
        {
            Debug.Log("No save file.");
        }
        return loadedProgress;
    }

    private QuestListData LoadQuests()
    {
        QuestListData questListData;
        if (File.Exists(dataPath + "/quests.txt"))
        {
            string questsString = File.ReadAllText(dataPath + "/quests.txt");
            questListData = JsonUtility.FromJson<QuestListData>(questsString);
            Debug.Log("Quest list loaded!");
        }
        else
        {
            string defaultQuests = "{\r\n   \"version\":1,\r\n   \"role\":\"Support\",\r\n   \"color\":\"A0CC3A\",\r\n   \"milestones\":[\r\n      500,\r\n      1000,\r\n      1500,\r\n      2500\r\n   ],\r\n   \"quests\":[\r\n      {\r\n         \"ID\":1,\r\n         \"text\":\"\u041F\u043E\u0438\u0433\u0440\u0430\u0439 \u0432 \u043A\u0430\u0436\u0434\u044B\u0439 \u043F\u0440\u043E\u0435\u043A\u0442\",\r\n         \"points\":50\r\n      },\r\n      {\r\n         \"ID\":2,\r\n         \"text\":\"\u0414\u043E\u0441\u0442\u0438\u0433\u043D\u0438 15 \u043B\u0438\u0433\u0438 \u0432 \u041C\u041F\",\r\n         \"points\":50\r\n      },\r\n      {\r\n         \"ID\":3,\r\n         \"text\":\"\u0414\u043E\u0441\u0442\u0438\u0433\u043D\u0438 15 \u043B\u0438\u0433\u0438 \u0432 \u0410\u041F\",\r\n         \"points\":50\r\n      },\r\n      {\r\n         \"ID\":4,\r\n         \"text\":\"\u0414\u043E\u0441\u0442\u0438\u0433\u043D\u0438 \u043B\u0438\u0433\u0438 \u041A\u043E\u043D\u044C-1 \u0432 ABC\",\r\n         \"points\":50\r\n      },\r\n      {\r\n         \"ID\":5,\r\n         \"text\":\"\u0414\u043E\u0439\u0434\u0438 \u0434\u043E 5 \u0433\u043B\u0430\u0432\u044B \u0432 \u041A\u043B\u0438\u043A\u0435\u0440\u0435\",\r\n         \"points\":50\r\n      },\r\n      {\r\n         \"ID\":6,\r\n         \"text\":\"\u0417\u0430\u043A\u0430\u0436\u0438 \u0435\u0434\u0443 \u0432\u043C\u0435\u0441\u0442\u0435 \u0441 \u043A\u043E\u043B\u043B\u0435\u0433\u0430\u043C\u0438\",\r\n         \"points\":50\r\n      },\r\n      {\r\n         \"ID\":7,\r\n         \"text\":\"\u041F\u043E\u0437\u043D\u0430\u043A\u043E\u043C\u044C\u0441\u044F \u0441 \u043A\u043E\u043B\u043B\u0435\u0433\u0430\u043C\u0438 \u0438\u0437 \u043A\u0430\u0436\u0434\u043E\u0433\u043E \u043E\u0442\u0434\u0435\u043B\u0430\",\r\n         \"points\":50\r\n      },\r\n      {\r\n         \"ID\":8,\r\n         \"text\":\"\u0423\u0437\u043D\u0430\u0439, \u0447\u0435\u043C \u0437\u0430\u043D\u0438\u043C\u0430\u044E\u0442\u0441\u044F\\n\u043A\u043E\u043C\u0430\u043D\u0434\u044B \u043D\u0430 \u043F\u0440\u043E\u0435\u043A\u0442\u0435\",\r\n         \"points\":50\r\n      },\r\n      {\r\n         \"ID\":9,\r\n         \"text\":\"\u0423\u0437\u043D\u0430\u0439, \u0437\u0430 \u0447\u0442\u043E \u043E\u0442\u0432\u0435\u0442\u0441\u0442\u0432\u0435\u043D\u043D\u0435\u043D\\n\u043A\u0430\u0436\u0434\u044B\u0439 \u043E\u0442\u0434\u0435\u043B\",\r\n         \"points\":50\r\n      },\r\n      {\r\n         \"ID\":10,\r\n         \"text\":\"\u041F\u043E\u0441\u0435\u0442\u0438 \u043F\u044F\u0442\u043D\u0438\u0447\u043D\u043E\u0435 \u0441\u043E\u0431\u0440\u0430\u043D\u0438\u0435 Panoramik\",\r\n         \"points\":50\r\n      },\r\n      {\r\n         \"ID\":11,\r\n         \"text\":\"\u0420\u0430\u0437\u0431\u0435\u0440\u0438\u0441\u044C \u0441 \u0444\u0443\u043D\u043A\u0446\u0438\u043E\u043D\u0430\u043B\u043E\u043C\\n\u0430\u0434\u043C\u0438\u043D\u043E\u043A \u043F\u0440\u043E\u0435\u043A\u0442\u043E\u0432\",\r\n         \"points\":50\r\n      },\r\n      {\r\n         \"ID\":12,\r\n         \"text\":\"\u041D\u0430\u0443\u0447\u0438\u0441\u044C \u0440\u0430\u0431\u043E\u0442\u0430\u0442\u044C \u0441 \u043A\u043E\u043D\u0444\u0438\u0433\u0430\u043C\u0438\\n\u043D\u0430 Google \u0434\u0438\u0441\u043A\u0435\",\r\n         \"points\":50\r\n      },\r\n      {\r\n         \"ID\":13,\r\n         \"text\":\"\u0420\u0430\u0437\u0431\u0435\u0440\u0438\u0441\u044C \u0441 \u0444\u0443\u043D\u043A\u0446\u0438\u043E\u043D\u0430\u043B\u043E\u043C Zendesk\",\r\n         \"points\":50\r\n      },\r\n      {\r\n         \"ID\":14,\r\n         \"text\":\"\u041E\u0442\u0432\u0435\u0442\u044C \u043D\u0430 \u0441\u0432\u043E\u0439 \u043F\u0435\u0440\u0432\u044B\u0439 \u0442\u0438\u043A\u0435\u0442\",\r\n         \"points\":50\r\n      },\r\n      {\r\n         \"ID\":15,\r\n         \"text\":\"\u041F\u0440\u043E\u0447\u0435\u043A\u0430\u0439 \u043A\u043B\u0438\u0435\u043D\u0442\u0441\u043A\u0438\u0435 \u043B\u043E\u0433\u0438 \u0443 \u0438\u0433\u0440\u043E\u043A\u0430\",\r\n         \"points\":50\r\n      },\r\n      {\r\n         \"ID\":16,\r\n         \"text\":\"\u041E\u0442\u043F\u0440\u0430\u0432\u044C \u0441\u0430\u043C\u043E\u0441\u0442\u043E\u044F\u0442\u0435\u043B\u044C\u043D\u043E \u0442\u0438\u043A\u0435\u0442\",\r\n         \"points\":50\r\n      },\r\n      {\r\n         \"ID\":17,\r\n         \"text\":\"\u041D\u0430\u043F\u0438\u0448\u0438 SQL-\u0437\u0430\u043F\u0440\u043E\u0441 \u0434\u043B\u044F\\n\u043F\u043E\u0438\u0441\u043A\u0430 \u0447\u0435\u0433\u043E-\u043B\u0438\u0431\u043E \u0432 \u0411\u0414\",\r\n         \"points\":50\r\n      },\r\n      {\r\n         \"ID\":18,\r\n         \"text\":\"\u041E\u0441\u0432\u043E\u0439 \u0438\u0441\u043F\u043E\u043B\u044C\u0437\u043E\u0432\u0430\u043D\u0438\u0435 _TestUtilFunctions \u0432 Unity\",\r\n         \"points\":50\r\n      },\r\n      {\r\n         \"ID\":19,\r\n         \"text\":\"\u041D\u0430\u0443\u0447\u0438\u0441\u044C \u043C\u043E\u043D\u0438\u0442\u043E\u0440\u0438\u0442\u044C \u0441\u043E\u0441\u0442\u043E\u044F\u043D\u0438\u0435 \u043F\u0440\u043E\u0435\u043A\u0442\u043E\u0432 \u0441 \u043F\u043E\u043C\u043E\u0449\u044C\u044E Grafana\",\r\n         \"points\":50\r\n      },\r\n      {\r\n         \"ID\":20,\r\n         \"text\":\"\u041E\u0441\u0432\u043E\u0439 \u0438\u0441\u043F\u043E\u043B\u044C\u0437\u043E\u0432\u0430\u043D\u0438\u0435 Adminer\\n\u0434\u043B\u044F \u0440\u0430\u0431\u043E\u0442\u044B \u0441 \u0431\u0430\u0437\u043E\u0439 \u0434\u0430\u043D\u043D\u044B\u0445\",\r\n         \"points\":50\r\n      },\r\n      {\r\n         \"ID\":21,\r\n         \"text\":\"\u0420\u0435\u0448\u0438 \u043F\u0440\u043E\u0431\u043B\u0435\u043C\u0443 \u0432 \u0414\u0438\u0441\u043A\u043E\u0440\u0434\u0435\",\r\n         \"points\":50\r\n      },\r\n      {\r\n         \"ID\":22,\r\n         \"text\":\"\u041F\u0440\u043E\u0447\u0438\u0442\u0430\u0439 \u0433\u0430\u0439\u0434\u044B \u0432 \u0441\u043B\u0430\u0439\u0442\u0435\",\r\n         \"points\":50\r\n      },\r\n      {\r\n         \"ID\":23,\r\n         \"text\":\"\u041F\u0435\u0440\u0435\u0436\u0438\u0432\u0438 \u043A\u043E\u0440\u043F\u043E\u0440\u0430\u0442\u0438\u0432\",\r\n         \"points\":100\r\n      },\r\n      {\r\n         \"ID\":24,\r\n         \"text\":\"\u0417\u0430\u0431\u0430\u043D\u044C \u0447\u0438\u0442\u0435\u0440\u0430\",\r\n         \"points\":100\r\n      },\r\n      {\r\n         \"ID\":25,\r\n         \"text\":\"\u041F\u0435\u0440\u0435\u0436\u0438\u0432\u0438 \u0441\u0432\u043E\u044E \u043F\u0435\u0440\u0432\u0443\u044E\\n\u043D\u043E\u0447\u043D\u0443\u044E \u0441\u043C\u0435\u043D\u0443\",\r\n         \"points\":100\r\n      },\r\n      {\r\n         \"ID\":26,\r\n         \"text\":\"\u041D\u0430\u0443\u0447\u0438\u0441\u044C \u043F\u043E\u043B\u044C\u0437\u043E\u0432\u0430\u0442\u044C\u0441\u044F\\n\u0441\u0435\u0440\u0432\u0435\u0440\u043D\u044B\u043C\u0438 \u043B\u043E\u0433\u0430\u043C\u0438\",\r\n         \"points\":100\r\n      },\r\n      {\r\n         \"ID\":27,\r\n         \"text\":\"\u041F\u043E\u0447\u0438\u043D\u0438(\u043D\u0430\u043C\u0435\u0440\u0435\u043D\u043D\u043E) / \u043F\u043E\u043B\u043E\u043C\u0430\u0439(\u0441\u043B\u0443\u0447\u0430\u0439\u043D\u043E) \u0430\u043A\u043A \u0432 \u0431\u0438\u0442\u0430\u0445\",\r\n         \"points\":100\r\n      },\r\n      {\r\n         \"ID\":28,\r\n         \"text\":\"\u041D\u0430\u0443\u0447\u0438\u0441\u044C \u043F\u043E\u043B\u044C\u0437\u043E\u0432\u0430\u0442\u044C\u0441\u044F Sourcetree\",\r\n         \"points\":100\r\n      },\r\n      {\r\n         \"ID\":29,\r\n         \"text\":\"\u0421\u0434\u0435\u043B\u0430\u0439 \u043C\u0430\u043A\u0440\u043E\u0441\",\r\n         \"points\":100\r\n      },\r\n      {\r\n         \"ID\":30,\r\n         \"text\":\"\u041D\u0430\u0443\u0447\u0438\u0441\u044C \u0434\u0435\u043B\u0430\u0442\u044C \u043D\u043E\u0432\u043E\u0441\u0442\u0438\",\r\n         \"points\":100\r\n      },\r\n      {\r\n         \"ID\":31,\r\n         \"text\":\"\u0417\u0430\u043D\u0443\u043B\u0438\u0442\u044C \u0442\u0438\u043A\u0435\u0442\u044B \u043D\u0430 \u043E\u0434\u043D\u043E\u043C \u0438\u0437 \u043F\u0440\u043E\u0435\u043A\u0442\u043E\u0432\",\r\n         \"points\":100\r\n      },\r\n      {\r\n         \"ID\":32,\r\n         \"text\":\"\u041F\u0440\u043E\u0442\u0435\u0441\u0442\u0438 \u0438\u0432\u0435\u043D\u0442\",\r\n         \"points\":100\r\n      },\r\n      {\r\n         \"ID\":33,\r\n         \"text\":\"\u0420\u0435\u0448\u0438 \u0442\u0438\u043A\u0435\u0442 \u0441 \u0442\u044D\u0433\u043E\u043C Hardcore\",\r\n         \"points\":100\r\n      },\r\n      {\r\n         \"ID\":34,\r\n         \"text\":\"\u041F\u0435\u0440\u0435\u0436\u0438\u0432\u0438 \u0441\u0432\u043E\u0439 \u043F\u0435\u0440\u0432\u044B\u0439 \u043D\u043E\u0447\u043D\u043E\u0439 \u0440\u0430\u0437\u044A\u0451\u0431\",\r\n         \"points\":150\r\n      },\r\n      {\r\n         \"ID\":35,\r\n         \"text\":\"\u041F\u0440\u043E\u0439\u0434\u0438 \u043B\u044E\u0431\u043E\u0439 \u043E\u043D\u043B\u0430\u0439\u043D-\u043A\u0443\u0440\u0441 \u043F\u043E SQL\",\r\n         \"points\":150\r\n      }\r\n   ]\r\n}";
            questListData = JsonUtility.FromJson<QuestListData>(defaultQuests);
            File.WriteAllText(dataPath + "/quests.txt", defaultQuests);
        }
        return questListData;
    }    

    [Serializable]
    public class QuestListData
    {
        public int version;
        public string role;
        public string color;
        public int[] milestones;
        public QuestData[] quests;
    }

    [Serializable]
    public class QuestData
    {
        public int ID;
        public string text;
        public int points;
    }

    [Serializable]
    public class SaveObject
    {
        public int version;
        public RoleSaveData[] roles;
    }

    [Serializable]
    public class RoleSaveData
    {
        public string role;
        public QuestProgressSaveData[] quests;

    }

    [Serializable]
    public class QuestProgressSaveData
    {
        public int ID;
        public bool complete;
    }
}
