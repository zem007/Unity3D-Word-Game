using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum GameMode{
    preGame,      //游戏开始前
    loading,      //Word.txt正在解析中
    makeLevel,    
    levelPrep,
    inLevel
}

public class WordGame : MonoBehaviour
{
    public static WordGame S;

    [Header("Set in Inspector")]
    public GameObject prefabLetter;
    public Rect wordArea = new Rect(-24, 19, 48, 28);
    public float letterSize = 1.5f;
    public bool showAllWyrds = true;
    public float bigLetterSize = 4f;
    public Color bigColorDim = new Color(0.8f, 0.8f, 0.8f);
    public Color bigColorSelected = new Color(1f, 0.9f, 0.7f);
    public Vector3 bigLetterCenter = new Vector3(0,-16,0);
    public Color[] wyrdPalette;

    [Header("Set Dynamically")]
    public GameMode mode = GameMode.preGame;
    public WordLevel currLevel;
    public List<Wyrd> wyrds;
    public List<Letter> bigLetters;
    public List<Letter> bigLettersActive;
    public string testWord;
    private string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    private Transform letterAnchor, bigLetterAnchor;

    void Awake() {
        S = this;
        letterAnchor = new GameObject("LetterAnchor").transform;
        bigLetterAnchor = new GameObject("BigLetterAnchor").transform;
    }

    void Start() {
        mode = GameMode.loading;
        //调用WordList脚本中的INIT方法，来解析txt中的单词
        WordList.INIT();
    }

    //该方法是txt文件解析后，被WordList脚本的SendMessage调用的
    public void WordListParseComplete() {
        mode = GameMode.makeLevel;
        currLevel = MakeWordLevel();
    }

    public WordLevel MakeWordLevel(int levelNum = -1) {
        WordLevel level = new WordLevel();
        if(levelNum == -1) {
            //选择一个随机level
            level.longWordIndex = Random.Range(0, WordList.LONG_WORD_COUNT);
        } else {

        }
        level.levelNum = levelNum;
        level.word = WordList.GET_LONG_WORD(level.longWordIndex);
        level.charDict = WordLevel.MakeCharDict(level.word);

        StartCoroutine(FindSubWordsCoroutine(level));

        return(level);
    }

    public IEnumerator FindSubWordsCoroutine(WordLevel level) {
        level.subWords = new List<string>();
        string str;

        List<string> words = WordList.GET_WORDS();   //获取所有的单词集

        for(int i=0; i<WordList.WORD_COUNT; i++) {
            str = words[i];   // one of the single word
            if(WordLevel.CheckWordInLevel(str, level)) {
                level.subWords.Add(str);
            }
            // yield
            if(i % WordList.NUM_TO_PARSE_BEFORE_YIELD == 0) {
                yield return null;   //每秒yield一次
            }
        }

        level.subWords.Sort();
        level.subWords = SortWordsByLength(level.subWords).ToList();

        //所有满足条件的单词已获取
        SubWordSearchComplete();
    }

    public static IEnumerable<string> SortWordsByLength(IEnumerable<string> ws) {
        ws = ws.OrderBy(s => s.Length);
        return ws;
    }

    public void SubWordSearchComplete() {
        mode = GameMode.levelPrep;
        Layout();
    }

    void Layout() 
    {
        //把当前level的所有subword显示在屏幕上
        wyrds = new List<Wyrd>();

        GameObject go;
        Letter lett;
        string word;
        Vector3 pos;
        float left = 0;
        float columnWidth = 3;
        char c;
        Color col;
        Wyrd wyrd;

        //一共有多少行letter
        int numRows = Mathf.RoundToInt(wordArea.height/letterSize);
        //生成一个word
        for(int i=0; i<currLevel.subWords.Count; i++) {
            wyrd = new Wyrd();
            word = currLevel.subWords[i];
            columnWidth = Mathf.Max(columnWidth, word.Length);

            //初始化word中的每一个letter
            for(int j=0; j<word.Length; j++) {
                c = word[j];
                go = Instantiate<GameObject>(prefabLetter);
                go.transform.SetParent(letterAnchor);
                lett = go.GetComponent<Letter>();
                lett.c = c;

                pos = new Vector3(wordArea.x + left + j*letterSize, wordArea.y, 0);
                pos.y -= (i%numRows) * letterSize;
                lett.posImmediate = pos + Vector3.up * (20 + i%numRows);
                lett.pos = pos;
                lett.timeStart = Time.time + i * 0.05f;
                go.transform.localScale = Vector3.one*letterSize;
                wyrd.Add(lett);
            }
            if(showAllWyrds) wyrd.visible = true;
            //根据长度来给wyrd上色
            wyrd.color = wyrdPalette[word.Length - WordList.WORD_LENGTH_MIN];
            wyrds.Add(wyrd);
            //如果是numRows(th)，新增一列
            if(i%numRows == numRows - 1) {
                left += (columnWidth + 0.5f) * letterSize;
            }
        }

        //初始化bigLetter并放置到位
        bigLetters = new List<Letter>();
        bigLettersActive = new List<Letter>();

        for(int i=0; i<currLevel.word.Length; i++) {
            c = currLevel.word[i];
            go = Instantiate<GameObject>(prefabLetter);
            go.transform.SetParent(bigLetterAnchor);
            lett = go.GetComponent<Letter>();
            lett.c = c;
            go.transform.localScale = Vector3.one * bigLetterSize;

            //把bigLetter的初始位置设为最下方
            pos = new Vector3(0, -100, 0);
            lett.posImmediate = pos;
            lett.pos = pos;
            //令bigLetters最后进入
            lett.timeStart = Time.time + currLevel.subWords.Count * 0.05f;
            lett.easingCuve = Easing.Sin + "-0.18";    //Bouncy easing

            col = bigColorDim;
            lett.color = col;
            lett.visible = true;
            lett.big = true;
            bigLetters.Add(lett);
        }
        //打乱bigLetters
        bigLetters = ShuffleLetters(bigLetters);
        //排列到屏幕上
        ArrangeBigLetters();
        mode = GameMode.inLevel;
    }

    List<Letter> ShuffleLetters(List<Letter> letts) {
        List<Letter> newL = new List<Letter>();
        int ndx;
        while(letts.Count > 0) {
            ndx = Random.Range(0, letts.Count);
            newL.Add(letts[ndx]);
            letts.RemoveAt(ndx);
        }
        return(newL);
    }

    void ArrangeBigLetters() {
        float halfWidth = ((float)bigLetters.Count)/2f - 0.5f;
        Vector3 pos;
        for(int i=0; i<bigLetters.Count; i++) {
            pos = bigLetterCenter;
            pos.x += (i-halfWidth)*bigLetterSize;
            bigLetters[i].pos = pos;
        }
        halfWidth = ((float)bigLettersActive.Count)/2f - 0.5f;
        for(int i=0; i < bigLettersActive.Count; i++) {
            pos = bigLetterCenter;
            pos.x += (i - halfWidth) * bigLetterSize;
            pos.y += bigLetterSize*1.25f;
            bigLettersActive[i].pos = pos;
        }
    }

    void Update() 
    {
        Letter ltr;
        char c;
        switch(mode) {
            case GameMode.inLevel:
                //对此帧中，用户输入的字符串循环
                foreach(char cIt in Input.inputString) {
                    c = System.Char.ToUpperInvariant(cIt);
                    if(upperCase.Contains(c)) {
                        //从bigLetter中找到一个可能的Letter
                        ltr = FindNextLetterByChar(c);
                        if(ltr != null) {
                            testWord += c.ToString();
                            bigLettersActive.Add(ltr);
                            bigLetters.Remove(ltr);
                            ltr.color = bigColorSelected;
                            ArrangeBigLetters();
                        }
                    }
                    if(c == '\b') {    //backspace
                        if(bigLettersActive.Count == 0) return;
                        if(testWord.Length > 1) {
                            //去掉testWord的最后一个字母
                            testWord = testWord.Substring(0, testWord.Length - 1);
                        } else {
                            testWord = "";
                        }

                        ltr = bigLettersActive[bigLettersActive.Count - 1]; //去掉最后一个Word
                        bigLettersActive.Remove(ltr);
                        bigLetters.Add(ltr);
                        ltr.color = bigColorDim;
                        ArrangeBigLetters();
                    }
                    if(c == '\n' || c == '\r') {   //return键或enter键
                        //测试testWord和WordLevel中的words
                        CheckWord();
                    }
                    if(c == ' ') {    //空格键
                        bigLetters = ShuffleLetters(bigLetters);
                        ArrangeBigLetters();
                    }
                }
                break;
        }
    }

    Letter FindNextLetterByChar(char c) {
        foreach(Letter ltr in bigLetters) {
            if(ltr.c == c) {
                return(ltr);
            }
        }
        return null;
    }

    public void CheckWord() {
        string subWord;
        bool foundTestWord = false;

        List<int> containedWords = new List<int>();
        for(int i=0; i<currLevel.subWords.Count; i++) {
            if(wyrds[i].found) {
                continue;
            }

            subWord = currLevel.subWords[i];
            if(string.Equals(testWord, subWord)) {
                HighlightWyrd(i);
                ScoreManager.SCORE(wyrds[i], 1);
                foundTestWord = true;
            } else if(testWord.Contains(subWord)) {
                containedWords.Add(i);
            }
        }
        if(foundTestWord) {
            int numContained = containedWords.Count;
            int ndx;
            for(int i=0; i<containedWords.Count; i++) {
                ndx = numContained - i - 1;   //反向HighLight
                HighlightWyrd(containedWords[ndx]);
                ScoreManager.SCORE(wyrds[containedWords[ndx]], i+2);
            }
        }
        ClearBigLettersActive();
    }

    void HighlightWyrd(int ndx) {
        wyrds[ndx].found = true;
        // 颜色变浅
        wyrds[ndx].color = (wyrds[ndx].color + Color.white)/2f;
        wyrds[ndx].visible = true;
    }

    void ClearBigLettersActive() {
        testWord = "";
        foreach(Letter ltr in bigLettersActive) {
            bigLetters.Add(ltr);
            ltr.color = bigColorDim;
        }
        bigLettersActive.Clear();
        ArrangeBigLetters();
    }
}
