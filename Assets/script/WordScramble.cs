using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Result
{
    public int totalScore = 0;

    [Header("REF UI")]
    public Text textTime;
    public Text textTotalScore;
}

[System.Serializable]
public class Word
{
    public string word;

    [Header("Leave empty if you want randomize")]
    public string desiredRandom;

    [Space(10)]
    public float timelimit;

    public string GetString()
    {
        if (!string.IsNullOrEmpty(desiredRandom))
        {
            return desiredRandom;
        }
        string result = word;

        while (result == word)
        {
            result = "";
            List<char> characters = new List<char>(word.ToCharArray());
            while (characters.Count > 0)
            {
                int indexChar = Random.Range(0, characters.Count - 1);
                result += characters[indexChar];

                characters.RemoveAt(indexChar);
            }
        }
        return result;
    }
}

public class WordScramble : MonoBehaviour
{
    public Word[] words;

    [Space(10)]
    public Result result;

    [Header("UI Refrence")]
    public CharObj prefab;
    public Transform container;
    public float space;

    List<CharObj> charObjs = new List<CharObj>();
    CharObj firstSelected;

    public int currentWord;
    public static WordScramble main;
    public float LerpSpeed = 5;

    private float totalScore;

    void Awake()
    {
        main = this;
    }

    void Start()
    {
        ShowScramble(currentWord);
        result.textTotalScore.text = result.totalScore.ToString();
    }

    void Update()
    {
        RepositionObject();

        totalScore = Mathf.Lerp(totalScore, result.totalScore, Time.deltaTime * 5);
        result.textTotalScore.text = Mathf.RoundToInt(totalScore).ToString();
    }

    void RepositionObject()
    {
        if (charObjs.Count == 0)
        {
            return;
        }
        float center = (charObjs.Count - 1) / 2;
        for (int i = 0; i < charObjs.Count; i++)
        {
            charObjs[i].rectTransform.anchoredPosition
                = Vector2.Lerp(charObjs[i].rectTransform.anchoredPosition,
                    new Vector2((i - center) * space, 0), LerpSpeed * Time.deltaTime);
            charObjs[i].index = i;
        }
    }

    /// <summary>
    /// show random word to screen
    /// </summary>
    public void ShowScramble()
    {
        ShowScramble(Random.Range(0, words.Length - 1));
    }

    /// <summary>
    /// show word from collection with desired index
    /// </summary>
    /// <param name="index">index of the element</param>
    public void ShowScramble(int index)
    {
        charObjs.Clear();
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        if (index > words.Length - 1)
        {
            //Debug.LogError("index out of range,please enter range between 0 to " + (words.Length - 1).ToString());
            Debug.Log("You Won");
            return;
        }

        char[] chars = words[index].GetString().ToCharArray();
        foreach (char c in chars)
        {
            CharObj clone = Instantiate(prefab.gameObject).GetComponent<CharObj>();
            clone.transform.SetParent(container);

            charObjs.Add(clone.Init(c));
        }

        currentWord = index;
        StartCoroutine(TimeLimit());
    }

    public void Swap(int indexA, int indexB)
    {
        CharObj tmpA = charObjs[indexA];

        charObjs[indexA] = charObjs[indexB];
        charObjs[indexB] = tmpA;

        charObjs[indexA].transform.SetAsLastSibling();
        charObjs[indexB].transform.SetAsLastSibling();

        CheckWord();
    }

    public void Select(CharObj charObj)
    {
        if (firstSelected)
        {
            Swap(firstSelected.index, charObj.index);

            //unselect
            firstSelected.Select();
            charObj.Select();

        }
        else
        {
            firstSelected = charObj;
        }
    }

    public void Unselect()
    {
        firstSelected = null;
    }

    public void CheckWord()
    {
        StartCoroutine(CoCheck());
    }

    IEnumerator CoCheck()
    {
        yield return new WaitForSeconds(0.5f);

        string word = "";
        foreach (CharObj charObj in charObjs)
        {
            word += charObj.character;
        }

        if (timelimit <= 0)
        {
            currentWord++;
            ShowScramble(currentWord);
            yield break;
        }

        if (word == words[currentWord].word)
        {
            currentWord++;
            result.totalScore += Mathf.RoundToInt(timelimit);
            result.textTotalScore.text = result.totalScore.ToString();
            ShowScramble(currentWord);
        }
    }

    float timelimit;
    IEnumerator TimeLimit()
    {
        timelimit = words[currentWord].timelimit;
        result.textTime.text = Mathf.RoundToInt(timelimit).ToString();

        int myWod = currentWord;

        yield return new WaitForSeconds(1);

        while (timelimit > 0)
        {
            if (myWod != currentWord)
            {
                yield break;
            }

            timelimit -= Time.deltaTime;
            result.textTime.text = Mathf.RoundToInt(timelimit).ToString();
            yield return null;
        }

        result.textTotalScore.text = result.totalScore.ToString();
        CheckWord();
    }
}
