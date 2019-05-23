using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public TextMesh textCoord;
    public TextMesh textF;
    public TextMesh textG;
    public TextMesh textH;
    public GameObject Arrow;
    public SpriteRenderer colorRender;

    public int x, y;
    public Vector2 vec2;
    public int g, h, f;
    
    public int tileType;
    //타입 0 : 일반 타일, 1 : 이동불가 타일 + 닫힌 타일, 2 : 시작 타일, 3 : 끝 타일
    public GameObject motherNode;

    public Tile()
    {
        this.vec2 = new Vector2(this.x, this.y);
    }
}
