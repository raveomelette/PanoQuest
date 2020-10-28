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
            string defaultQuests = "{\"version\":1,\"role\":\"Support\",\"color\":\"A0CC3A\",\"milestones\":[500,1000,1500,2500],\"quests\":[{\"ID\":1,\"text\":\"Поиграй в каждый проект\",\"points\":50},{\"ID\":2,\"text\":\"Достигни 15 лиги в МП\",\"points\":50},{\"ID\":3,\"text\":\"Достигни 15 лиги в АП\",\"points\":50},{\"ID\":4,\"text\":\"Достигни лиги Конь-1 в ABC\",\"points\":50},{\"ID\":5,\"text\":\"Дойди до 5 главы в Кликере\",\"points\":50},{\"ID\":6,\"text\":\"Закажи еду вместе с коллегами\",\"points\":50},{\"ID\":7,\"text\":\"Познакомься с коллегами из каждого отдела\",\"points\":50},{\"ID\":8,\"text\":\"Узнай, чем занимаются\\nкоманды на проекте\",\"points\":50},{\"ID\":9,\"text\":\"Узнай, за что ответственнен\\nкаждый отдел\",\"points\":50},{\"ID\":10,\"text\":\"Посети пятничное собрание Panoramik\",\"points\":50},{\"ID\":11,\"text\":\"Разберись с функционалом\\nадминок проектов\",\"points\":50},{\"ID\":12,\"text\":\"Научись работать с конфигами\\nна Google диске\",\"points\":50},{\"ID\":13,\"text\":\"Разберись с функционалом Zendesk\",\"points\":50},{\"ID\":14,\"text\":\"Ответь на свой первый тикет\",\"points\":50},{\"ID\":15,\"text\":\"Прочекай клиентские логи у игрока\",\"points\":50},{\"ID\":16,\"text\":\"Отправь самостоятельно тикет\",\"points\":50},{\"ID\":17,\"text\":\"Напиши SQL-запрос для\\nпоиска чего-либо в БД\",\"points\":50},{\"ID\":18,\"text\":\"Освой использование _TestUtilFunctions в Unity\",\"points\":50},{\"ID\":19,\"text\":\"Научись мониторить состояние проектов с помощью Grafana\",\"points\":50},{\"ID\":20,\"text\":\"Освой использование Adminer\\nдля работы с базой данных\",\"points\":50},{\"ID\":21,\"text\":\"Реши проблему в Дискорде\",\"points\":50},{\"ID\":22,\"text\":\"Прочитай гайды в слайте\",\"points\":50},{\"ID\":23,\"text\":\"Переживи корпоратив\",\"points\":100},{\"ID\":24,\"text\":\"Забань читера\",\"points\":100},{\"ID\":25,\"text\":\"Переживи свою первую\\nночную смену\",\"points\":100},{\"ID\":26,\"text\":\"Научись пользоваться\\nсерверными логами\",\"points\":100},{\"ID\":27,\"text\":\"Почини(намеренно) / поломай(случайно) акк в битах\",\"points\":100},{\"ID\":28,\"text\":\"Научись пользоваться Sourcetree\",\"points\":100},{\"ID\":29,\"text\":\"Сделай макрос\",\"points\":100},{\"ID\":30,\"text\":\"Научись делать новости\",\"points\":100},{\"ID\":31,\"text\":\"Переживи свой первый\\nночной разъёб\",\"points\":100},{\"ID\":32,\"text\":\"Протести ивент\",\"points\":100},{\"ID\":33,\"text\":\"Реши тикет с тэгом Hardcore\",\"points\":100},{\"ID\":34,\"text\":\"Вместе с серверными реши релизную проблему\",\"points\":150},{\"ID\":35,\"text\":\"Пройди любой онлайн-курс по SQL\",\"points\":150}]}";
            Debug.Log("No quest list found! Loading default quest list.");
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
