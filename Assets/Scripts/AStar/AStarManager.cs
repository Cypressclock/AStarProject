using System.Collections.Generic;
using UnityEngine;


// A星寻路算法

public class AStarManager
{
    private static AStarManager instance;
    public static AStarManager GetInstance()
    {
        if (instance == null)
        {
            instance = new AStarManager();
        }

        return instance;
    }

    //地图宽高
    private int mapW;
    private int mapH;
    //地图所有格子对象
    public AStarNode[,] mapNodes;
  
    //开启列表
    private List<AStarNode> openList;
    public List<AStarNode> openList2 = new List<AStarNode>();
    //关闭列表
    private List<AStarNode> closeList;

    
    ///// 初始化地图
    
    /// <param name="w"></param>
    /// <param name="h"></param>
    /// <param name="stops"></param>
    /// <param name="stopNum"></param>
    public void InitMap(int w, int h, Vector2[] stops, int stopNum)
    {
        mapNodes = new AStarNode[w,h];

        mapW = w;
        mapH = h;
        //创建格子
        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                mapNodes[i, j] = new AStarNode(i, j, ENodeType.Walk);
              
            }
        }
        //随机阻挡
        if (stopNum == 0)
        {
            for (int i = 0; i < stops.Length; i++)
            {
                int x = (int)stops[i].x;
                int y = (int)stops[i].y;
                mapNodes[x, y].type = ENodeType.Stop;
            }
        }
        else
        {
            for (int i = 0; i < stopNum; i++)
            {
                int x = Random.Range(0, w);
                int y = Random.Range(0, h);
                mapNodes[x, y].type = ENodeType.Stop;
            }
        }


        //初始化开启和关闭列表
        openList = new List<AStarNode>();
        closeList = new List<AStarNode>();
    }

    
    // 寻路
   
    public List<AStarNode> FindPath(Vector2 startPos, Vector2 endPos)
    {
        int sx = Mathf.FloorToInt(startPos.x);
        int sy = Mathf.FloorToInt(startPos.y);
        
        int ex = Mathf.FloorToInt(endPos.x);
        int ey = Mathf.FloorToInt(endPos.y);
        
        
        //是否越界
        if (IsMapExternal(sx, sy) || IsMapExternal(ex, ey))
        {
            Debug.LogError("起点或终点在地图外");
            return null;
        }
        //是否阻挡
        var starNode = mapNodes[sx,sy];
        var endNode = mapNodes[ex,ey];
        if (starNode.type == ENodeType.Stop || endNode.type == ENodeType.Stop)
        {
            Debug.LogError("起点或终点是阻挡");
            return null;
        }
        
        //会多次寻路，需要清空上一次数据
        openList.Clear();
        openList2.Clear();
        closeList.Clear();
        starNode.f = 0;
        starNode.g = 0;
        starNode.h = 0;
        starNode.father = null;
        starNode.minNode = false;

        closeList.Add(starNode);
        
        if (FindEndPoint(sx, sy,ex, ey))
        {
            var pathEndNode = closeList[closeList.Count - 1];
            var pathList = new List<AStarNode>();
            AStarNode cNode = pathEndNode;
            while (true)
            {
                pathList.Add(cNode);
                if (cNode.father == null)
                {
                    break;
                }
                cNode = cNode.father;
            }
            pathList.Reverse();
           
            return pathList;
        }
        
        return null;
    }
    
    private bool FindEndPoint(int x, int y, int ex, int ey)
    {
        Debug.Log(x + " " +  y);
        var fatherNode = mapNodes[x, y];
        //寻找周围的8个点，放入开启列表
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                if (i == 0 && j == 0)//中心点。自己
                {
                    continue;
                }
                int cx = x + i;
                int cy = y + j;
                if (cx == ex && cy == ey)//目标点
                {
                    //放入关闭列表中，然后从开启列表中移除
                    var curNode = mapNodes[cx, cy];
                    curNode.father = fatherNode;
                    closeList.Add(curNode);
                    return true;
                }
                if (!IsMapExternal(cx, cy)) //地图内
                {
                    var curNode = mapNodes[cx, cy];
                    /* if (openList.Contains(curNode))
                     {
                         float d = 1;
                         if (i != 0 && j != 0)//四个角
                         {
                             d = 1.4f;
                         }
                         float gThis = fatherNode.g + d;
                         if (gThis < curNode.g)
                         {
                             curNode.g = gThis;
                             curNode.f = curNode.g + curNode.h;
                             curNode.father = fatherNode;
                         }
                     }*/
                    if (curNode.type == ENodeType.Walk && !openList.Contains(curNode) && !closeList.Contains(curNode)) // 不是阻挡 ,未加入开启或者关闭列表中
                    {
                        curNode.father = fatherNode;
                        curNode.minNode = false;
                        //计算寻路消耗 ： f = g + h
                        //g = 父节点的g + 当前点到父节点的直线距离
                        float g = fatherNode.g;
                        float d = 1;
                        if (i != 0 && j != 0)//四个角
                        {
                            d = 1.4f;
                        }
                        var fatherPos = new Vector2(fatherNode.x, fatherNode.y);
                        var cg = d;
                        g += cg;
                        
                        //h用曼哈顿距离计算
                        float h = 0;
                        float h1 = Mathf.Abs(ex - cx);
                        float h2 = Mathf.Abs(ey - cy);
                        h = h1 + h2;

                        float f = g + h;
                        curNode.f = f;
                        curNode.g = g;
                        curNode.h = h;
                       
                        openList.Add(curNode);
                        
                        openList2.Add(curNode);//打印测试用
                    }
                    
                }
            }
        }

        if (openList.Count == 0)//未找到终点时，开启列表会一个一个全部移入关闭列表中
        {
            Debug.LogError("死路");
            return false;
        }
        //选出开启列表中寻路消耗最小的点
         openList.Sort(((node1, node2) =>
         {
             return node1.f >= node2.f ? 1 : -1;
         } ));
        var minNode = openList[0];
        /*var minNode = openList[openList.Count - 1];
        for (int i = openList.Count - 2; i >= 0; i--)
         {
             var node = openList[i];
             if (node.f < minNode.f)
             {
                 minNode = node;
             }
         }*/
        //放入关闭列表中，然后从开启列表中移除
        closeList.Add(minNode);
        openList.Remove(minNode);
        minNode.minNode = true;
        
        //如果是终点，直接返回
        //否则，继续寻路
        if (minNode.x == ex && minNode.y == ey)
        {
            return true;
        }

        return FindEndPoint(minNode.x, minNode.y, ex, ey);
       
    }
  
    // 是否在地图外
    
    private bool IsMapExternal(int x, int y)
    {
        if (x < 0 || x >= mapW || y < 0 || y >= mapH )
        {
            return true;
        }

        return false;
    }

    //是否阻挡
    private bool IsMapObstruct(int x, int y)
    {
        return true;
    }
}
