using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HexDirection
{
    NE, E, SE, SW, W, NW
}
public enum HexEdgeType
{
    Flat, Slope, Cliff
}
public static class HexDirectionExtensions
{
    public static HexDirection Opposite(this HexDirection direction)
    {
        return (int)direction < 3 ? (direction + 3) : (direction - 3);
    }
    public static HexDirection Previous(this HexDirection direction)
    {
        return direction == HexDirection.NE ? HexDirection.NW : (direction - 1);
    }
    public static HexDirection Next(this HexDirection direction)
    {
        return direction == HexDirection.NW ? HexDirection.NE : (direction + 1);
    }
}


[System.Serializable]
public struct HexCoordinates
{
    [SerializeField]
    private int x, z;

    public int X => x;
    public int Y => -X - Z;
    public int Z => z;
    public static HexCoordinates FromOffsetCoordinates(int x, int z)
    {
        return new HexCoordinates(x - z / 2, z);
    }
    public HexCoordinates(int x, int z)
    {
        this.x = x;
        this.z = z; 
    }
    public override string ToString()
    {
        return "(" + X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() + ")";
    }
    public string ToStringOnSeparateLines()
    {
        return X.ToString() + "\n" + Y.ToString() + "\n" + Z.ToString();
    }
    public static HexCoordinates FromPosition(Vector3 position)
    {
        float x = position.x / (HexMetrics.innerRadius * 2f);
        float y = -x;
        
        float offset = position.z / (HexMetrics.outerRadius * 3f);
        x -= offset;
        y -= offset;

        int iX = Mathf.RoundToInt(x);
        int iY = Mathf.RoundToInt(y);
        int iZ = Mathf.RoundToInt(-x - y);

        if (iX + iY + iZ != 0)
        {
            float dX = Mathf.Abs(x - iX);
            float dY = Mathf.Abs(y - iY);
            float dZ = Mathf.Abs(-x - y - iZ);

            if (dX > dY && dX > dZ)
            {
                iX = -iY - iZ;
            }
            else if (dZ > dY)
            {
                iZ = -iX - iY;
            }
        }

        return new HexCoordinates(iX, iZ);
    }
}

public static class HexMetrics
{
    public const float outerRadius = 10f;
    public const float innerRadius = outerRadius * 0.866025404f;
    public const float solidFactor = 0.75f;
    public const float blendFactor = 1f - solidFactor;
    public const float elevationStep = 5f;
    public const int terracesPerSlope = 2;
    public const int terraceSteps = terracesPerSlope * 2 + 1;
    public const float horizontalTerraceStepSize = 1f / terraceSteps;
    public const float verticalTerraceStepSize = 1f / (terracesPerSlope + 1);
    // 六个点的LocalPosition
    static Vector3[] corners = {

        new Vector3(0f, 0f, outerRadius),                         // up
        new Vector3(innerRadius, 0, 0.5f * outerRadius),         // up right
        new Vector3(innerRadius, 0f, -0.5f * outerRadius),        // down right
        new Vector3(0f, 0f, -outerRadius),                         // down
        new Vector3(-innerRadius, 0f, -0.5f * outerRadius),       // down left
        new Vector3(-innerRadius, 0f, 0.5f * outerRadius),        // up left
        new Vector3(0f, 0f, outerRadius)                          // up 
    };

    public static Vector3 GetFirstCorner(HexDirection direction) => corners[(int)direction];
    public static Vector3 GetSecondCorner(HexDirection direction) => corners[(int)direction + 1];
    public static Vector3 GetFirstSolidCorner(HexDirection direction)
    {
        return corners[(int)direction] * solidFactor;
    }
    public static Vector3 GetSecondSolidCorner(HexDirection direction)
    {
        return corners[(int)direction + 1] * solidFactor;
    }
    public static Vector3 GetBridge(HexDirection direction)    // 得到剩下部分的三角形高度的向量
    {
        return (corners[(int)direction] + corners[(int)direction + 1]) * blendFactor;
    }
    public static Vector3 TerraceLerp(Vector3 a, Vector3 b, int step)
    {
        float h = step * HexMetrics.horizontalTerraceStepSize;
        a.x += (b.x - a.x) * h;
        a.z += (b.z - a.z) * h;
        float v = ((step + 1) / 2) * HexMetrics.verticalTerraceStepSize;
        a.y += (b.y - a.y) * v;
        return a;
    }
    public static Color TerraceLerp(Color a, Color b, int step)
    {
        float h = step * HexMetrics.horizontalTerraceStepSize;
        return Color.Lerp(a, b, h);
    }
    public static HexEdgeType GetEdgeType(int elevation1, int elevation2)
    {
        if (elevation1 == elevation2)
        {
            return HexEdgeType.Flat;
        }
        int delta = elevation2 - elevation1;
        if (delta == 1 || delta == -1)
        {
            return HexEdgeType.Slope;
        }
        return HexEdgeType.Cliff;
    }
    
}
