using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// A星寻路算法
public class MyTest : MonoBehaviour
{

    public Color normalColor, stopColor, pathColor, findColor, startColor, endColor;
    public int mapW = 10;
    public int mapH = 10;
    public float cubeOffset = 0.1f;//格子间距
    public int stopNum = 20;//随机阻挡数量
    public Vector2[] stops;//固定阻挡坐标
    
    private Dictionary<string, GameObject> goDic;
    private Vector2 mousePos = Vector2.one * -1;
    private List<AStarNode> pathList;//寻路路径

    private bool setStart = false;

    private int step = 0;

    [SerializeField]
    private bool openStep;
    
    void Start()
    {
        AStarManager.GetInstance().InitMap(mapW, mapH, stops, stopNum);
        
        StartCoroutine(CreateCube());

        if (mapW * mapH < 30)
        {
            transform.position = new Vector3(1, 45.5f, 2.2f);
            Camera.main.orthographicSize = 4;
        }
    }

    IEnumerator CreateCube()
    {
        goDic = new Dictionary<string, GameObject>();
        var nodes = AStarManager.GetInstance().mapNodes;
        //创建格子
        for (int i = 0; i < mapW; i++)
        {
            for (int j = 0; j < mapH; j++)
            {
                if (j%6==0)
                {
                    yield return null;
                }
                
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.transform.position = new Vector3(i + cubeOffset*i, 0, j + cubeOffset*j);
                go.name = i + "_" + j;
                goDic.Add(go.name, go);

                //创建格子阻挡
                var node = nodes[i, j];
                if (node.type == ENodeType.Stop)
                {
                    go.GetComponent<MeshRenderer>().material.SetColor("_Color", stopColor);
                }
                else
                {
                    go.GetComponent<MeshRenderer>().material.SetColor("_Color", normalColor);
                }
            }
        }
       
    }

    public void ShowNextStep()
    {
        var list2 = AStarManager.GetInstance().openList2;//之前已经有了
     
        if (step >= list2.Count)
        {
            var list = pathList;
            for (int i = 0; i < list.Count; i++)
            {
                var node = list[i];
                var goName = node.x + "_" + node.y;
                var pathGo = goDic[goName];
                pathGo.GetComponent<MeshRenderer>().material.SetColor("_Color", pathColor);
            }
            return;
        }
      
        //for (int i = 0; i < list2.Count; i++)
        {
            var node = list2[step];
            var goName = node.x + "_" + node.y;
            var pathGo = goDic[goName];
            pathGo.GetComponent<MeshRenderer>().material.SetColor("_Color", node.minNode ? Color.magenta : findColor);
        }
                        
        
        step++;
        print(step);
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit raycastHit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out raycastHit, 1000))
            {
                var go = raycastHit.collider.gameObject;
                string[] names = go.name.Split('_');
                go.GetComponent<MeshRenderer>().material.SetColor("_Color", startColor);
                int x = int.Parse(names[0]);
                int y = int.Parse(names[1]);
                var pos = new Vector2(x, y);
                if (!setStart)//第一次点击，设置为起点
                {
                    var list2 = AStarManager.GetInstance().openList2;//之前已经有了
                    if (list2.Count > 0)
                    {
                        ClearMouse();
                    }
                    mousePos = pos;
                    setStart = true;
                }
                else //第二次点击，进行寻路
                {
                    print(mousePos + ":" + pos);
                    var list = AStarManager.GetInstance().FindPath(mousePos, pos);
                    step = 0;
                    if (list != null)
                    {
                        if (!openStep)
                        {
                            var list2 = AStarManager.GetInstance().openList2;//之前已经有了
                            for (int i = 0; i < list2.Count; i++)
                            {
                                var node = list2[i];
                                var goName = node.x + "_" + node.y;
                                var pathGo = goDic[goName];
                                pathGo.GetComponent<MeshRenderer>().material.SetColor("_Color", findColor);
                            }
                            
                            for (int i = 0; i < list.Count; i++)
                            {
                                var node = list[i];
                                var goName = node.x + "_" + node.y;
                                var pathGo = goDic[goName];
                                pathGo.GetComponent<MeshRenderer>().material.SetColor("_Color", pathColor);
                            }
                        }
                        
                        pathList = list;

                        var startGoName = mousePos.x + "_" + mousePos.y;
                        var startGo = goDic[startGoName];
                        startGo.GetComponent<MeshRenderer>().material.SetColor("_Color", startColor);
                        
                        go.GetComponent<MeshRenderer>().material.SetColor("_Color", endColor);
                        
                    }
                    mousePos = Vector2.one * -1;
                    setStart = false;
                }
            }
        }else if (Input.GetMouseButtonDown(1))
        {
            //clear
            ClearMouse();
        }else if (openStep && Input.GetKeyUp(KeyCode.Space))
        {
            ShowNextStep();
        }
    }

    private void ClearMouse()
    {
        if (setStart) //第一次点击，设置为起点
        {
            var go = goDic[mousePos.x + "_" + mousePos.y];
            go.GetComponent<MeshRenderer>().material.SetColor("_Color", normalColor);
            setStart = false;
        }

        if (pathList != null)
        {
            for (int i = 0; i < pathList.Count; i++)
            {
                var node = pathList[i];
                var goName = node.x + "_" + node.y;
                var pathGo = goDic[goName];
                pathGo.GetComponent<MeshRenderer>().material.SetColor("_Color", normalColor);
            }
            pathList = null;
        }
        
        var list2 = AStarManager.GetInstance().openList2;
        for (int i = 0; i < list2.Count; i++)
        {
            var node = list2[i];
            var goName = node.x + "_" + node.y;
            var pathGo = goDic[goName];
            pathGo.GetComponent<MeshRenderer>().material.SetColor("_Color", normalColor);
        }
        list2.Clear();
    }
}
