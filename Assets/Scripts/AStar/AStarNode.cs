

public enum ENodeType  {
    Walk,
    Stop
}

/// A星寻路算法

public class AStarNode
{
    
    //坐标
    public int x;
    public int y;
    
    
    //寻路消耗
    public float f;
    //离起点的距离
    public float g;
    //离终点的距离
    public float h;
    //父对象
    public AStarNode father;
    //格子类型
    public ENodeType type;

    public bool minNode = false;

    public AStarNode(int x, int y, ENodeType type)
    {
        this.x = x;
        this.y = y;
        this.type = type;
    }

}
