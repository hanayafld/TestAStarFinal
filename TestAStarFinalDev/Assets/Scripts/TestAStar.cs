using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TestAStar : MonoBehaviour
{
    //타일 생성
    public Button btnSetting;
    public Button btnOpenList;
    public Button btnCloseList;
    public Button btnFinal;

    public int col;
    public int row;
    public GameObject tilePos;
    public Vector2 startPoint;
    public Vector2 endPoint;

    private GameObject tileGo;
    private GameObject[,] tiles;
    private GameObject startNode;
    private GameObject endNode;

    //타일 탐색
    public Button btnFind;

    private GameObject motherNode;
    private List<GameObject> openList;
    private List<GameObject> closeList;
    private bool firstFind = true;

    //최종경로
    private List<GameObject> routeList;

    void Awake()
    {
        this.tileGo = Resources.Load<GameObject>("Tile");
        this.tiles = new GameObject[this.col, this.row];
        this.openList = new List<GameObject>();
        this.closeList = new List<GameObject>();
        this.routeList = new List<GameObject>();
    }

    void Start()
    {
        //타일생성
        this.CreateTile();

        //타일지정
        this.TileSeting();
        this.btnSetting.onClick.AddListener(() =>
        {
            StopAllCoroutines();
            this.btnSetting.gameObject.SetActive(false);
            this.btnFind.gameObject.SetActive(true);
        });
        this.SetMotherNode(this.tiles[(int)startPoint.x, (int)startPoint.y]);

        this.btnFind.onClick.AddListener(() =>
        {
            if (this.firstFind == true)
            {
                this.FindNode();
                this.firstFind = false;
            }
            else
            {
                this.SetMotherNode(this.openList[0]);
                this.FindNode();
            }
        });

        this.btnFinal.onClick.AddListener(() =>
        {
            this.RouteFind(this.closeList[closeList.Count - 1]);
            Debug.Log(this.routeList.Count);
        });

        this.btnOpenList.onClick.AddListener(() =>
        {
            for (int i = 0; i < openList.Count; i++)
            {
                var list = openList[i].GetComponent<Tile>();
                Debug.LogFormat("열린리스트 {0} : {1}, {2} ( F : {3}, G : {4}, H : {5})", i, list.x, list.y, list.f, list.g, list.h);
            }
        });

        this.btnCloseList.onClick.AddListener(() =>
        {
            for (int i = 0; i < closeList.Count; i++)
            {
                var list = closeList[i].GetComponent<Tile>();
                Debug.LogFormat("닫힌리스트 {0} : {1}, {2}", i, list.x, list.y);
            }
        });
    }

    //타일생성
    private void CreateTile()
    {
        for (int i = 0; i < this.col; i++)
        {
            for (int j = 0; j < this.row; j++)
            {
                if (i == this.startPoint.x && j == this.startPoint.y)
                {
                    //시작위치 생성
                    var tile = Instantiate(tileGo);
                    this.tiles[i, j] = tile;
                    tile.transform.SetParent(this.tilePos.transform);
                    tile.transform.position = new Vector3(j, -i);
                    tile.GetComponent<Tile>().textCoord.text = i.ToString() + ", " + j.ToString();
                    tile.GetComponent<Tile>().tileType = 2;
                    tile.GetComponent<Tile>().colorRender.color = Color.green;
                    tile.GetComponent<Tile>().x = (int)this.startPoint.x;
                    tile.GetComponent<Tile>().y = (int)this.startPoint.y;
                    this.startNode = tile;
                }
                else if (i == this.endPoint.x && j == endPoint.y)
                {
                    //엔드포인트 생성
                    var tile = Instantiate(tileGo);
                    this.tiles[i, j] = tile;
                    tile.transform.SetParent(this.tilePos.transform);
                    tile.transform.position = new Vector3(j, -i);
                    tile.GetComponent<Tile>().textCoord.text = i.ToString() + ", " + j.ToString();
                    tile.GetComponent<Tile>().tileType = 3;
                    tile.GetComponent<Tile>().colorRender.color = Color.red;
                    tile.GetComponent<Tile>().x = (int)this.endPoint.x;
                    tile.GetComponent<Tile>().y = (int)this.endPoint.y;
                    this.endNode = tile;
                }
                else
                {
                    var tile = Instantiate(tileGo);
                    this.tiles[i, j] = tile;
                    tile.transform.SetParent(this.tilePos.transform);
                    tile.transform.position = new Vector3(j, -i);
                    tile.GetComponent<Tile>().textCoord.text = i.ToString() + ", " + j.ToString();
                    tile.GetComponent<Tile>().x = i;
                    tile.GetComponent<Tile>().y = j;
                }
            }
        }
    }

    //타일지정
    private void TileSeting()
    {
        StartCoroutine(this.TileSetingImpl());
    }
    private IEnumerator TileSetingImpl()
    {
        while (true)
        {
            if (Input.GetMouseButtonUp(0))
            {
                Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);
                if (hit.collider != null)
                {
                    if (hit.collider.GetComponent<Tile>().tileType == 0)
                    {
                        hit.collider.GetComponent<Tile>().tileType = 1;
                        hit.collider.GetComponent<Tile>().colorRender.color = Color.blue;
                    }
                    else if (hit.collider.GetComponent<Tile>().tileType == 1)
                    {
                        hit.collider.GetComponent<Tile>().tileType = 0;
                        hit.collider.GetComponent<Tile>().colorRender.color = Color.black;
                    }
                }
            }
            yield return null;

        }
    }

    //마더 노드 지정
    private void SetMotherNode(GameObject tile)
    {
        Debug.LogFormat("마더 노드 :{0}, {1}", -tile.transform.position.y, tile.transform.position.x);
        this.motherNode = tile;
        this.motherNode.GetComponent<Tile>().tileType = 1;
        if (!(tile.GetComponent<Tile>().x == this.startPoint.x && tile.GetComponent<Tile>().y == this.startPoint.y))
        {
            this.motherNode.GetComponentInChildren<SpriteRenderer>().color = new Color(155, 0, 255);
        }
    }

    //다음 마더노드 찾기
    private void FindNode()
    {
        var mother = this.motherNode.GetComponent<Tile>();
        var minX = mother.x - 1;
        var minY = mother.y - 1;
        var maxX = mother.x + 1;
        var maxY = mother.y + 1;

        #region indexOver방지
        if (minX < 0)
        {
            minX = 0;
        }
        if (minY < 0)
        {
            minY = 0;
        }
        if (maxX >= this.col)
        {
            maxX = this.col - 1;
        }
        if (maxY >= this.row)
        {
            maxY = this.row - 1;
        }
        #endregion

        for (int i = minX; i <= maxX; i++)
        {
            for (int j = minY; j <= maxY; j++)
            {
                if (this.tiles[i, j].GetComponent<Tile>().tileType == 0)
                {
                    if (this.CheackOpenList(this.tiles[i, j]))
                    {
                        this.openList.Add(this.tiles[i, j]);
                    }
                    //자신의 마더노드가 null이면(처음 열린목록에 들어간다면) 값을 세팅
                    if (tiles[i, j].GetComponent<Tile>().motherNode == null)
                    {
                        this.SetValue(this.tiles[i, j]);
                    }
                    else//자신의 마더노드가 null이 아니고(이미 열린목록에 있는 아이라면) 값을 변경하기전에 G값이 더 작은지 검사 if값이 작다면 새로운 부모를 정함
                    {
                        var currentG = this.tiles[i, j].GetComponent<Tile>().g;
                        var newG = (int)((Vector2.Distance(tiles[i, j].transform.position, this.motherNode.transform.position)) * 10);
                        newG += this.motherNode.GetComponent<Tile>().g;
                        if (newG < currentG)
                        {
                            this.SetValue(this.tiles[i, j]);
                        }

                    }
                    this.tiles[i, j].GetComponent<Tile>().motherNode = this.motherNode;
                    this.tiles[i, j].GetComponent<Tile>().colorRender.color = Color.gray;
                }
                else if (i == mother.x && j == mother.y)
                {
                    this.tiles[i, j].GetComponent<Tile>().tileType = 1;
                    this.openList.Remove(this.tiles[i, j]);
                    this.closeList.Add(this.tiles[i, j]);
                    this.tiles[i, j].GetComponent<Tile>().motherNode = this.closeList[closeList.Count - 1].gameObject;
                }
                else if (i == endPoint.x && j == endPoint.y)
                {
                    Debug.Log("찾음");
                    this.btnFind.gameObject.SetActive(false);
                    this.btnFinal.gameObject.SetActive(true);
                    this.tiles[i, j].GetComponent<Tile>().motherNode = this.closeList[this.closeList.Count - 1];
                    this.closeList.Add(this.tiles[i, j]);

                    //Debug.LogFormat("{0}, {1}", this.tiles[i, j].GetComponent<Tile>().x, this.tiles[i, j].GetComponent<Tile>().y);
                    //Debug.LogFormat("{0}, {1}", this.tiles[i, j].GetComponent<Tile>().motherNode.GetComponent<Tile>().x, this.tiles[i, j].GetComponent<Tile>().motherNode.GetComponent<Tile>().y);
                }
            }
        }

        this.OpenListSort();
    }

    private bool CheackOpenList(GameObject tile)
    {
        foreach (var node in this.openList)
        {
            if (node == tile)
            {
                return false;
            }
        }
        return true;
    }

    //값지정
    private void SetValue(GameObject tile)
    {
        var tileScript = tile.GetComponent<Tile>();
        var targetScript = this.endNode.GetComponent<Tile>();

        //G변경
        var disG = (int)((Vector2.Distance(tile.transform.position, this.motherNode.transform.position)) * 10);
        disG += this.motherNode.GetComponent<Tile>().g;
        tileScript.textG.text = disG.ToString();
        tileScript.g = disG;

        //H변경
        var disH = (int)Mathf.Round(Vector2.Distance(this.endNode.transform.position, tile.transform.position) * 10);
        tileScript.textH.text = disH.ToString();
        tileScript.h = disH;

        //F변경
        tileScript.f = tileScript.g + tileScript.h;
        tileScript.textF.text = tileScript.f.ToString();

        var xPos = this.motherNode.transform.position.x - tileScript.Arrow.transform.position.x;
        var yPos = this.motherNode.transform.position.y - tileScript.Arrow.transform.position.y;
        var rad = Mathf.Atan2(yPos, xPos) * Mathf.Rad2Deg;
        tileScript.Arrow.transform.rotation = Quaternion.Euler(new Vector3(0, 0, rad));

        tileScript.Arrow.SetActive(true);
    }

    //openList정렬
    private void OpenListSort()
    {
        //Debug.Log("솔트시작");
        int i = 0;
        int j = 0;
        int min = 0;

        for (i = 0; i < this.openList.Count - 1; i++)//전체 훑기 Count-1 : 마지막칸은 자동으로 정렬되고 뒷칸이 없으므로 제외
        {
            min = i;//최소값을 들고 있을 제 3의 손//맨 처음엔 검사하는 첫배열을 들고있음

            for (j = i + 1; j < openList.Count; j++)// 최소값을 탐색한다.
            {
                if (openList[j].GetComponent<Tile>().f < openList[min].GetComponent<Tile>().f)//초기값 or 최소값이 j보다 크면 j를 최소값에 넣는다.
                {
                    min = j;
                }
            }
            if (min != 0)//스왑 알고리즘
            {
                this.SwapList(i, min);
            }
        }
    }

    private void SwapList(int index1, int index2)
    {
        //값을 임시로 저장할 temp변수
        var temp = openList[index2];
        openList[index2] = openList[index1];
        openList[index1] = temp;
    }

    private int i = 0;
    private GameObject RouteFind(GameObject node)
    {
        Debug.Log(i++);
        var tile = node.GetComponent<Tile>();

        Debug.LogFormat("어머니 : {0}, {1}", tile.GetComponent<Tile>().motherNode.GetComponent<Tile>().x, tile.GetComponent<Tile>().motherNode.GetComponent<Tile>().y);
        this.routeList.Add(node);
        //if (tile.x == this.startPoint.x && tile.y == startPoint.y)
        if(tile.x==3&&tile.y==6)
        {
            Debug.Log("경로 완성!");
            //왜 씨발 어머니의 어머니가 어머니냐 좆같네
            return node;
        }
        else
        {
            this.RouteFind(tile.GetComponent<Tile>().motherNode);
        }
        return null;
    }
}
