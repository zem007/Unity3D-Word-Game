using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WordLevel
{
    public int levelNum;
    public int longWordIndex;
    public string word;
    public Dictionary<char, int> charDict;
    public List<string> subWords;

    //下面方法作用：输入一个字符串，然后返回一个字典，储存了所有字母和出现频率
    public static Dictionary<char, int> MakeCharDict(string w) {
        Dictionary<char, int> dict = new Dictionary<char, int>();
        char c;
        for(int i=0; i<w.Length; i++) {
            c = w[i];
            if(dict.ContainsKey(c)) {
                dict[c]++;
            } else {
                dict.Add(c, 1);
            }
        }
        return dict;
    }

    //下面的方法作用：检查一个单词是否能用level.charDict中的字母拼写成
    public static bool CheckWordInLevel(string str, WordLevel level) {
        Dictionary<char, int> counts = new Dictionary<char, int>();
        for(int i=0; i < str.Length; i++) {
            char c= str[i];
            if(level.charDict.ContainsKey(c)) {
                if(!counts.ContainsKey(c)) {
                    counts.Add(c,1);
                } else {
                    counts[c]++;
                }
                if(counts[c] > level.charDict[c]) {
                    return false;
                }
            } else {
                return false;
            }
        }
        return true;
    }
}
