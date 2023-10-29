using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    public SquareGrid squareGrid;
    List <Vector3> vertices;
    List <int> triangles;

    Dictionary<int,List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>>();
    public void GenerateMesh(int [,] map, float squareSize) {
        vertices = new List<Vector3>();
        triangles = new List<int>();
        squareGrid = new SquareGrid(map, squareSize);

        for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
        {
            for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
            {
                TriangulateSquare(squareGrid.squares[x,y]);
            }
        }

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }

    void TriangulateSquare(Square square) {
        switch (square.configuration) {
            case 0:
                break;
            
            // 1 active nodes
            case 1:
                MeshFromPoints(square.centerLeft, square.centerBottom, square.bottomLeft);
                break;
            case 2:
                MeshFromPoints(square.bottomRight, square.centerBottom, square.centerRight);
                break;
            case 4:
                MeshFromPoints(square.topRight, square.centerRight, square.centerTop);
                break;
            case 8:
                MeshFromPoints(square.topLeft, square.centerTop, square.centerLeft);
                break;
            
            // 2 active nodes
            case 3:
                MeshFromPoints(square.centerRight, square.bottomRight, square.bottomLeft, square.centerLeft);
                break;
            case 6:
                MeshFromPoints(square.centerTop, square.topRight, square.bottomRight, square.centerBottom);
                break;
            case 9:
                MeshFromPoints(square.topLeft, square.centerTop, square.centerBottom, square.bottomLeft);
                break;
            case 12:
                MeshFromPoints(square.topLeft, square.topRight, square.centerRight, square.centerLeft);
                break;
            case 5:
                MeshFromPoints(square.centerTop, square.topRight, square.centerRight, square.centerBottom, square.bottomLeft, square.centerLeft);
                break;
            case 10:
                MeshFromPoints(square.topLeft, square.centerTop, square.centerRight, square.bottomRight, square.centerBottom, square.centerLeft);
                break;

            // 3 active nodes
            case 7:
                MeshFromPoints(square.centerTop, square.topRight, square.bottomRight, square.bottomLeft, square.centerLeft);
                break;
            case 11:
                MeshFromPoints(square.topLeft, square.centerTop, square.centerRight, square.bottomRight, square.bottomLeft);
                break;
            case 13:
                MeshFromPoints(square.topLeft, square.topRight, square.centerRight, square.centerBottom, square.bottomLeft);
                break;
            case 14:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.centerBottom, square.centerLeft);
                break;

            // 4 active nodes
            case 15:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
                break;
        }
    }

    void MeshFromPoints(params Node[] points) { // param -> use quando não sabe exatamente quanta coisa vai entrar
        AssignVertices(points);

        if (points.Length >= 3)
        {
            CreateTriangle(points[0], points[1], points[2]);
        }
        if (points.Length >= 4)
        {
            CreateTriangle(points[0], points[2], points[3]);
        }
        if (points.Length >= 5)
        {
            CreateTriangle(points[0], points[3], points[4]);
        }
        if (points.Length >= 6)
        {
            CreateTriangle(points[0], points[4], points[5]);
        }
    }

    void AssignVertices(Node[] points){
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].vertexIndex == -1) // Não recebeu nada ainda
            {
                points[i].vertexIndex = vertices.Count;
                vertices.Add(points[i].position);
            }
        }
    }

    void CreateTriangle(Node a, Node b, Node c){
        triangles.Add(a.vertexIndex);
        triangles.Add(b.vertexIndex);
        triangles.Add(c.vertexIndex);

        Triangle triangle = new Triangle(a.vertexIndex, b.vertexIndex, c.vertexIndex);
        AddTriangleToDictionary(triangle.vertexIndexA, triangle);
        AddTriangleToDictionary(triangle.vertexIndexB, triangle);
        AddTriangleToDictionary(triangle.vertexIndexC, triangle);
    }

    int GetConnectedOutlineVertex(int vertexIndex){
        List<Triangle> trianglesContainingVertex = triangleDictionary[vertexIndex];

        for (int i = 0; i < trianglesContainingVertex.Count; i++)
        {
            Triangle triangle = trianglesContainingVertex[i];

            for (int j = 0; j < 3; j++)
            {
                int vertexB = triangle[j]; // Fazendo uso do indexer

                if (vertexB != vertexIndex)
                {
                    if (IsOutlineEdge(vertexIndex, vertexB)){
                        return vertexB;
                    }   
                }
            }
        }
        
        return -1;
    }

    bool IsOutlineEdge(int vertexA, int vertexB){
        List<Triangle> trianglesWithVertexA = triangleDictionary [vertexA];
        int sharedTriangleCount = 0;

        for (int i = 0; i < trianglesWithVertexA.Count; i++)
        {
            if (trianglesWithVertexA[i].Contains(vertexB))
            {
                sharedTriangleCount++;
                if (sharedTriangleCount > 1)
                {
                    break;
                }
            }
        }

        return sharedTriangleCount == 1;
    }

    void AddTriangleToDictionary(int vertexIndexKey, Triangle triangle){
        if (triangleDictionary.ContainsKey(vertexIndexKey))
        {
            triangleDictionary[vertexIndexKey].Add(triangle);
        } else
        {
            List<Triangle> triangleList = new List<Triangle>();
            triangleList.Add(triangle);
            triangleDictionary.Add(vertexIndexKey, triangleList);
        }
    }

    struct Triangle {
        public int vertexIndexA;
        public int vertexIndexB;
        public int vertexIndexC;

        int[] vertices;

        public Triangle (int a, int b, int c){
            vertexIndexA = a;
            vertexIndexB = b;
            vertexIndexC = c;

            vertices =  new int[3];
            vertices[0] = a;
            vertices[1] = b;
            vertices[2] = c;
        }

        // Indexer
        public int this[int i]{
            get {
                return vertices[i];
            }
        }

        public bool Contains(int vertexIndex){
            return vertexIndex == vertexIndexA || vertexIndex == vertexIndexB || vertexIndex == vertexIndexC;
        }
    }

    public class SquareGrid {
        public Square[,] squares;

        public SquareGrid(int [,] map, float squareSize){ // Vai receber o mapa do map gen
            int nodeCountX = map.GetLength(0);
            int nodeCountY = map.GetLength(1);
            //GetLength(Int32) Method is used to find the total number of elements present in the specified dimension of the Array
            float mapWidth = nodeCountX * squareSize;
            float mapHeight = nodeCountY * squareSize;

            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];

            for (int x = 0; x < nodeCountX; x++)
            {
                for (int y = 0; y < nodeCountY; y++)
                {
                    Vector3 pos = new Vector3(-mapWidth/2 + x * squareSize + squareSize/2, 0, -mapWidth/2 + y * squareSize + squareSize/2);
                    controlNodes[x,y] = new ControlNode(pos, map[x,y] == 1, squareSize); //ControlNode(Vector3 _pos, bool _active, float squareSize)
                }
            }

            squares = new Square[nodeCountX - 1, nodeCountY - 1];
            for (int x = 0; x < nodeCountX -1; x++)
            {
                for (int y = 0; y < nodeCountY -1; y++)
                {
                    squares[x,y] = new Square(controlNodes[x,y+1], controlNodes[x+1, y+1], controlNodes[x+1, y], controlNodes[x, y]);
                }
            }
        }
    }

    public class Square {
        public ControlNode topLeft, topRight, bottomRight, bottomLeft;
        public Node centerTop, centerRight, centerLeft, centerBottom;
        public int configuration;

        public Square (ControlNode _topLeft, ControlNode _topRight, ControlNode _bottomRight, ControlNode _bottomLeft){
            topLeft = _topLeft;
            bottomLeft = _bottomLeft;
            bottomRight = _bottomRight;
            topRight = _topRight;

            centerTop = topLeft.right; // Centro horizontal de cima
            centerRight = bottomRight.above; // Centro vertical direito
            centerBottom = bottomLeft.right; // Centro horizontal de baixo
            centerLeft = bottomLeft.above; // Centro vertical esquero

            if (topLeft.active)
            {
                configuration += 8;
            }
            if (topRight.active)
            {
                configuration += 4;
            }
            if (bottomRight.active){
                configuration += 2;
            }
            if (bottomLeft.active)
            {
                configuration += 1;
            }
        }
        /*
        - . -
        . . .
        - . -

        a . b
        . . .
        d . c

        a = 1000 (binary) = 8 = 2^3
        b = 0100 (binary) = 4 = 2^2
        c = 0010 (binary) = 2 = 2^1
        d = 0001 (binary) = 1 = 2^0
        */
    }

    public class Node {
        public Vector3 position;
        public int vertexIndex = -1;

        public Node(Vector3 _pos) {
            position = _pos;
        }
    }

    public class ControlNode : Node {
        public bool active;
        public Node above, right;

        public ControlNode(Vector3 _pos, bool _active, float squareSize) : base(_pos){
            active = _active;
            above = new Node(position + Vector3.forward * squareSize/2f);
            right = new Node(position + Vector3.right * squareSize/2f);
        }
    }

}
