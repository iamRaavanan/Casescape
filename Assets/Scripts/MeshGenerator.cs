using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshGenerator : MonoBehaviour {

    public SquareGrid squareGrid;
    public MeshFilter walls;
    public MeshFilter cave;
    public bool is2D;

    List<Vector3> vertices;
    List<int> triangles;
    Dictionary<int, List<Triangle>> triangleDict = new Dictionary<int, List<Triangle>>();

    List<List<int>> outlines = new List<List<int>>();
    HashSet<int> checkedVertices = new HashSet<int>();

    public void GenerateMesh (int[,] map, float squareSize)
    {
        triangleDict.Clear();
        outlines.Clear();
        checkedVertices.Clear();
        squareGrid = new SquareGrid(map, squareSize);
        vertices = new List<Vector3>();
        triangles = new List<int>();
        for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
        {
            for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
            {
                TriangulateSquare (squareGrid.squares[x,y]);
            }
        }
        Mesh mesh = new Mesh();
        cave.mesh = mesh;
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        if (!is2D)
            CreateWallMesh();
    }


    private void CreateWallMesh ()
    {
        CalculateMeshOutlines();
        List<Vector3> wallVertices = new List<Vector3>();
        List<int> wallTriangles = new List<int>();
        Mesh wallMesh = new Mesh();
        float wallHeight = 5;
        
        foreach (List<int> outline in outlines)
        {
            for (int i = 0; i < outline.Count - 1; i++)
            {
                int startedIndex = wallVertices.Count;
                wallVertices.Add(vertices[outline[i]]);     // Left
                wallVertices.Add(vertices[outline[i+1]]);   // Right
                wallVertices.Add(vertices[outline[i]] - Vector3.up * wallHeight);     // Bottom Left
                wallVertices.Add(vertices[outline[i+1]] - Vector3.up * wallHeight);     // Bottom Right

                wallTriangles.Add(startedIndex + 0);
                wallTriangles.Add(startedIndex + 2);
                wallTriangles.Add(startedIndex + 3);

                wallTriangles.Add(startedIndex + 3);
                wallTriangles.Add(startedIndex + 1);
                wallTriangles.Add(startedIndex + 0);
            }
        }
        wallMesh.vertices = wallVertices.ToArray();
        wallMesh.triangles = wallTriangles.ToArray();
        walls.mesh = wallMesh;
        wallMesh.RecalculateNormals();

        MeshCollider wallCollider = walls.gameObject.AddComponent<MeshCollider>();
        wallCollider.sharedMesh = wallMesh;
    }

    private void TriangulateSquare (Square square)
    {
        switch (square.configuration)
        {
            case 0:
                break;
                // Single Points 
            case 1:
                MeshFromPoints(square.centreLeft, square.centreBottom, square.bottomLeft);
                break;
            case 2:
                MeshFromPoints(square.bottomRight, square.centreBottom, square.centreRight);
                break;
            case 4:
                MeshFromPoints(square.topRight, square.centreRight, square.centreTop);
                break;
            case 8:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreLeft);
                break;
            // Double Points 
            case 3:
                MeshFromPoints(square.centreRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                break;
            case 6:
                MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.centreBottom);
                break;
            case 9:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreBottom, square.bottomLeft);
                break;
            case 12:
                MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreLeft);
                break;
            case 5:
                MeshFromPoints(square.centreTop, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft, square.centreLeft);
                break;
            case 10:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.centreBottom, square.centreLeft);
                break;

            // Three Points
            case 7:
                MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                break;
            case 11:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.bottomLeft);
                break;
            case 13:
                MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft);
                break;
            case 14:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.centreBottom, square.centreLeft);
                break;

            // Fours Points
            case 15:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
                checkedVertices.Add(square.topLeft.vertexIndex);
                checkedVertices.Add(square.topRight.vertexIndex);
                checkedVertices.Add(square.bottomRight.vertexIndex);
                checkedVertices.Add(square.bottomLeft.vertexIndex);
                break;
        }
    }

    private void MeshFromPoints (params Node[] node)
    {
        AssignVertices(node);
        if (node.Length >= 3)
            CreateTriangles(node[0], node[1], node[2]);
        if (node.Length >= 4)
            CreateTriangles(node[0], node[2], node[3]);
        if (node.Length >= 5)
            CreateTriangles(node[0], node[3], node[4]);
        if (node.Length >= 6)
            CreateTriangles(node[0], node[4], node[5]);
    }
    
    private void AssignVertices (Node[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            if(points[i].vertexIndex == -1)
            {
                points[i].vertexIndex = vertices.Count;
                vertices.Add(points[i].position);
            }
        }
    }

    private void CreateTriangles (Node a, Node b, Node c)
    {
        triangles.Add(a.vertexIndex);
        triangles.Add(b.vertexIndex);
        triangles.Add(c.vertexIndex);
        Triangle triangle = new Triangle(a.vertexIndex, b.vertexIndex, c.vertexIndex);
        AddTriangleToDict(triangle.vertexIndexA, triangle);
        AddTriangleToDict(triangle.vertexIndexB, triangle);
        AddTriangleToDict(triangle.vertexIndexC, triangle);
    }

    private void AddTriangleToDict (int vertexIndex, Triangle triangle)
    {
        if(triangleDict.ContainsKey (vertexIndex))
        {
            triangleDict[vertexIndex].Add(triangle);
        }
        else
        {
            List<Triangle> tris = new List<Triangle>();
            tris.Add(triangle);
            triangleDict.Add(vertexIndex, tris);
        }
    }

    private void CalculateMeshOutlines ()
    {
        for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++)
        {
            if(!checkedVertices.Contains (vertexIndex))
            {
                int newOutlineVertex = GetConnectedOutlineVertex(vertexIndex);
                if(newOutlineVertex != -1)
                {
                    checkedVertices.Add(vertexIndex);
                    List<int> newOutline = new List<int>();
                    newOutline.Add(vertexIndex);
                    outlines.Add(newOutline);
                    FollowOutline(newOutlineVertex, outlines.Count - 1);
                    outlines[outlines.Count - 1].Add(vertexIndex);
                }
            }
        }
    }

    private void FollowOutline (int vertexIndex, int outlineIndex)
    {
        outlines[outlineIndex].Add(vertexIndex);
        checkedVertices.Add(vertexIndex);
        int nextVertexIndex = GetConnectedOutlineVertex(vertexIndex);
        if (nextVertexIndex != -1)
            FollowOutline(nextVertexIndex, outlineIndex);   
    }

    private int GetConnectedOutlineVertex (int vertexIndex)
    {
        List<Triangle> trianglesContainingVertex = triangleDict[vertexIndex];
        for (int i = 0; i < trianglesContainingVertex.Count; i++)
        {
            Triangle triangle = trianglesContainingVertex[i];
            for (int j = 0; j < 3; j++)
            {
                int vertexB = triangle[j];
                if(vertexB != vertexIndex && !checkedVertices.Contains (vertexB))
                {
                    if (IsOutlineEdge(vertexIndex, vertexB))
                    {
                        return vertexB;
                    }
                }
            }            
        }
        return -1;
    }

    private bool IsOutlineEdge (int vertexA, int vertexB)
    {
        List<Triangle> triangleContainVertexA = triangleDict[vertexA];
        int sharedTriangleCount = 0;

        for (int i = 0; i < triangleContainVertexA.Count; i++)
        {
            if (triangleContainVertexA[i].Contains(vertexB))
                sharedTriangleCount++;
            if (sharedTriangleCount > 1)
                break;
        }
        return sharedTriangleCount == 1;
    }

    struct Triangle
    {
        public int vertexIndexA;
        public int vertexIndexB;
        public int vertexIndexC;
        int[] vertices;
        public Triangle (int a, int b, int c)
        {
            vertexIndexA = a;
            vertexIndexB = b;
            vertexIndexC = c;
            vertices = new int[3];
            vertices[0] = a;
            vertices[1] = b;
            vertices[2] = c;
        }

        public int this[int i]
        {
            get
            {
                return vertices[i];
            }
        }

        public bool Contains (int vertex)
        {
            return vertex == vertexIndexA || vertex == vertexIndexB || vertex == vertexIndexC;
        }
    }
    public class Node
    {
        public Vector3 position;
        public int vertexIndex = -1;

        public Node (Vector3 m_pos)
        {
            position = m_pos;
        }
    }

    public class ControlNode : Node
    {
        public bool active;
        public Node above, right;

        public ControlNode (Vector3 m_pos, bool m_active, float squaresize) : base (m_pos)
        {
            active = m_active;
            above = new Node(position + Vector3.forward * squaresize / 2);
            right = new Node(position + Vector3.right * squaresize / 2);
        }
    }

    public class Square
    {
        public ControlNode topRight, topLeft, bottomRight, bottomLeft;
        public Node centreTop, centreRight, centreBottom, centreLeft;
        public int configuration;

        public Square (ControlNode m_topLeft, ControlNode m_topRight, ControlNode m_bottomRight, ControlNode m_bottomLeft)
        {
            topLeft = m_topLeft;
            topRight = m_topRight;
            bottomRight = m_bottomRight;
            bottomLeft = m_bottomLeft;

            centreTop = topLeft.right;
            centreRight = bottomRight.above;
            centreBottom = bottomLeft.right;
            centreLeft = bottomLeft.above;

            if (topLeft.active)
                configuration += 8;
            if (topRight.active)
                configuration += 4;
            if (bottomRight.active)
                configuration += 2;
            if (bottomLeft.active)
                configuration += 1;
        }
    }

    public class SquareGrid
    {
        public Square[,] squares;
        public SquareGrid (int[,] map, float squareSize)
        {
            int nodeCountX = map.GetLength(0);
            int nodeCountY = map.GetLength(1);
            float mapWidth = nodeCountX * squareSize;
            float mapHeight = nodeCountY * squareSize;

            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];

            for (int x = 0; x < nodeCountX; x++)
            {
                for (int y = 0; y < nodeCountY; y++)
                {
                    Vector3 position = new Vector3(-mapWidth / 2 + x * squareSize + squareSize / 2, 0, -mapHeight / 2 + y * squareSize + squareSize / 2);
                    controlNodes[x, y] = new ControlNode(position, map[x, y] == 1, squareSize);
                }
            }

            squares = new Square[nodeCountX - 1, nodeCountY - 1];
            for (int x = 0; x < nodeCountX-1; x++)
            {
                for (int y = 0; y < nodeCountY-1; y++)
                {
                    squares[x, y] = new Square(controlNodes[x, y + 1], controlNodes[x + 1, y + 1], controlNodes[x + 1, y], controlNodes[x, y]);
                }
            }
        }
    }
}
