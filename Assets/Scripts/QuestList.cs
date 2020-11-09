using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Newtonsoft.Json;
using UnityEngine.Scripting;

[Preserve]
public class QuestList 
{
    public string listName;
    public bool hasCustomMentorList;
    public int[] milestones;
    public Dictionary<int, QuestDataItem> quests;

    public void Add(int id, QuestDataItem quest)
    {
        quests.Add(id, quest);
    }

    public QuestList()
    {
        listName = "";
        hasCustomMentorList = false;
        milestones = new int[0];
        quests = new Dictionary<int, QuestDataItem>();
    }
    
    public QuestList(string _listName)
    {
        listName = _listName;
        hasCustomMentorList = false;
        milestones = new int[0];
        quests = new Dictionary<int, QuestDataItem>();
    }
    public QuestList(string _listName, bool _isDepartment, bool _hasCustomMentorList, int[] _milestones)
    {
        listName = _listName;
        hasCustomMentorList = _hasCustomMentorList;
        milestones = _milestones;
        quests = new Dictionary<int, QuestDataItem>();
    }
}
