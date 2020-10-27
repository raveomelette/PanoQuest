using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;


public class QuestItem : MonoBehaviour
{
    static bool unlocked;
    public int points;
    public int ID;
    public TMP_Text description;
    public TMP_Text pointsLabel;
    public Image pointsbox;
    public Toggle checkbox;
    public Image check;
    public ProgressBarHandler pbar;
    // Start is called before the first frame update
    void Start()
    {
        unlocked = true;
    }

    // Update is called once per frame
    void Update()
    {
        checkbox.interactable = unlocked;
    }

    public void ToggleChanged(bool questCompleted)
    {
        unlocked = false;
        StartCoroutine(CheckBoxDelay());
        if (questCompleted)
        {
            pbar.current += points;
            pointsbox.color = pbar.colorLocked;
            pointsLabel.color = pbar.colorLocked;
            description.color = pbar.colorLocked;
            pbar.questCompletionDictionary[ID] = true;
        }
        else 
        { 
            pbar.current -= points;
            pointsbox.color = pbar.colorDone;
            pointsLabel.color = Color.white;
            description.color = Color.white;
            pbar.questCompletionDictionary[ID] = false;
        }
        if (pbar.current < 0)
            pbar.current = 0;
        pbar.Save();
    }

    IEnumerator CheckBoxDelay()
    {
        Debug.Log(Time.time);
        yield return new WaitForSeconds(0.3f);
        Debug.Log(Time.time);

        // This line will be executed after 10 seconds passed
        unlocked = true;
    }
}
