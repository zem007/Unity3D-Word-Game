using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordList : MonoBehaviour
{
    private static WordList S;

    [Header("Set in Inspector")]
    public TextAsset wordListText;
    public int numToParseBeforeYield = 10000;   //yield之前读取10000行数据
    public int wordLengthMin = 3;
    public int wordLengthMax = 7;

    [Header("Set Dynamically")]
    public int currLine = 0;
    public int totalLines;
    public int longWordCount;
    public int wordCount;

    private string[] lines;
    private List<string> longWords;
    private List<string> words;

    void Awake()
    {
        S = this;
    }

    public void Init() 
    {
        lines = wordListText.text.Split('\n');
        totalLines = lines.Length;
        StartCoroutine(ParseLines());
    }

    public static void INIT() {
        S.Init();
    }

    public IEnumerator ParseLines() 
    {
        string word;
        longWords = new List<string>();
        words = new List<string>();
        for(currLine = 0; currLine < totalLines; currLine ++) {
            word = lines[currLine];
            if(word.Length == wordLengthMax) {
                longWords.Add(word);
            }
            if(word.Length >= wordLengthMin && word.Length <= wordLengthMax) {
                words.Add(word);
            }
            if(currLine % numToParseBeforeYield == 0) {
                longWordCount = longWords.Count;
                wordCount = words.Count;
                yield return null;    //每帧延时执行
                // yield return new WaitForSeconds(2);    //延时2s执行
            }
        }
        longWordCount = longWords.Count;
        wordCount = words.Count;

        //告知gameObject，Parse 已经完毕, 并调用其中某脚本的WordListParseComplete方法
        gameObject.SendMessage("WordListParseComplete");
    }

    public static List<string> GET_WORDS() {
        return(S.words);
    }

    public static string GET_WORD(int ndx) {
        return(S.words[ndx]);
    }

    public static List<string> GET_LONG_WORDS() {
        return(S.longWords);
    }

    public static string GET_LONG_WORD(int ndx) {
        return(S.longWords[ndx]);
    }

    public static int WORD_COUNT {
        get {return S.wordCount;}
    }

    public static int LONG_WORD_COUNT {
        get {return S.longWordCount;}
    }

    public static int NUM_TO_PARSE_BEFORE_YIELD {
        get {return S.numToParseBeforeYield;}
    }

    public static int WORD_LENGTH_MIN {
        get {return S.wordLengthMin;}
    }

    public static int WORD_LENGTH_MAX {
        get {return S.wordLengthMax;}
    }
}
