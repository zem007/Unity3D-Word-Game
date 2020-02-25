using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Letter : MonoBehaviour
{
    [Header("Set Dynamically")]
    public TextMesh tMesh;    //3D Text中的Text Mesh属性, 其中3D Text是Letter的child
    public Renderer tRend;    //3D Text中的Renderer, 用来控制char是否显示
    public bool big = false;

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

    public Vector3 pos {
        set { transform.position = value; }
    }

}
