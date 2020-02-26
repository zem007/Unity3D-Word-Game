using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    private static ScoreManager S;
    [Header("Set in Inspector")]
    public List<float> scoreFontSizes = new List<float> {36, 64, 64, 1};
    public Vector3 scoreMidPoint = new Vector3(1,1,0);
    public float scoreTravelTime = 3f;
    public float scoreComboDelay = 0.5f;

    private RectTransform rectTrans;

    void Awake() {
        S = this;
        rectTrans = GetComponent<RectTransform>();
    }

    //下列静态方法Score能被任意访问
    public static void SCORE(Wyrd wyrd, int combo) {
        S.Score(wyrd, combo);
    }

    void Score(Wyrd wyrd, int combo) {
        //贝塞尔曲线
        List<Vector2> pts = new List<Vector2>();

        Vector3 pt = wyrd.letters[0].transform.position;
        pt = Camera.main.WorldToViewportPoint(pt);
        pts.Add(pt);
        pts.Add(scoreMidPoint);
        pts.Add(rectTrans.anchorMax);

        //设置Floating Score的值
        int value = wyrd.letters.Count * combo;
        FloatingScore fs = Scoreboard.S.CreateFloatingScore(value, pts);

        fs.timeDuration = scoreTravelTime;
        fs.timeStart = Time.time + combo * scoreComboDelay;
        fs.fontSizes = scoreFontSizes;
        fs.easingCurve = Easing.InOut + Easing.InOut;

        //使显示类似为 “3 x 2”
        string txt = wyrd.letters.Count.ToString();
        if(combo > 1) {
            txt += "x" + combo;
        }
        fs.GetComponent<Text>().text = txt;
    }
}
