using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Letter : MonoBehaviour
{
    [Header("Set in Inspector")]
    public float timeDuration = 0.5f;
    public string easingCuve = Easing.InOut;

    [Header("Set Dynamically")]
    public TextMesh tMesh;    //3D Text中的Text Mesh属性, 其中3D Text是Letter的child
    public Renderer tRend;    //3D Text中的Renderer, 用来控制char是否显示
    public bool big = false;
    //线性插入
    public List<Vector3> pts = null;
    public float timeStart = -1;

    private char _c;    //这个Letter显示的char
    private Renderer rend;   //Letter中的Renderer

    void Awake() 
    {
        tMesh = GetComponentInChildren<TextMesh>();
        tRend = tMesh.GetComponent<Renderer>();
        rend = GetComponent<Renderer>();    //Letter中的Renderer
        visible = false;
    }

    public char c {
        get { return(_c); }
        set { 
            _c = value;
            tMesh.text = _c.ToString();
        }
    }

    public string str {
        get { return(_c.ToString()); }
        set { c = value[0]; }
    }

    public bool visible {
        get { return(tRend.enabled); }
        set { tRend.enabled = value; }
    }

    public Color color {
        get { return(rend.material.color); }
        set { rend.material.color = value; }
    }

    //非立即移动到新位置，而是为Letter增加移动的动画
    public Vector3 pos {
        set { 
            Vector3 mid = (transform.position + value)/2f;
            float mag = (transform.position -value).magnitude;
            mid += Random.insideUnitSphere * mag * 0.25f;

            //贝塞尔曲线
            pts = new List<Vector3>() {transform.position, mid, value};

            if(timeStart == -1) timeStart = Time.time;
        }
    }

    //Letter立即移动到新的位置
    public Vector3 posImmediate {
        set{
            transform.position = value;
        }
    }

    void Update() {
        if(timeStart == -1) return;

        float u = (Time.time - timeStart)/timeDuration;
        u = Mathf.Clamp01(u);
        float u1 = Easing.Ease(u, easingCuve);
        Vector3 v = Utils.Bezier(u1, pts);
        transform.position = v;

        if(u == 1) timeStart = -1;
    }
}
