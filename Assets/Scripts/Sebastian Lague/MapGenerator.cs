using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MapGenerator : MonoBehaviour
{
    public int width;
    public int height;

    public string seed;
    public bool useRandomSeed;

    [Range(0, 100)]
    public int randomFillPercent;

    int[,] map;

    void Start()
    {
        GenerateMap();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GenerateMap();
        }
    }

    void GenerateMap(){
        map = new int[width, height];
        RandomFillMap();

        for (int i = 0; i < 4; i++)
        {
            SmoothMap();
        }

        ProcessMap();

        int borderSize = 5;
        int[,] borderedMap = new int[width + borderSize * 2, height + borderSize * 2];

        for (int x = 0; x < borderedMap.GetLength(0); x++)
        {
            for (int y = 0; y < borderedMap.GetLength(1); y++)
            {
                if ( x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize) {
                    borderedMap[x,y] = map[x - borderSize, y - borderSize];
                }
                else
                {
                    borderedMap[x,y] = 1;
                }
            }
        }

        MeshGenerator meshGenerator = GetComponent<MeshGenerator>();
        meshGenerator.GenerateMesh(borderedMap, 1);

    }

    void ProcessMap(){
        List<List<Coord>> wallRegions = GetRegions(1);
        int wallThresholdSize = 50;

        foreach (List<Coord> wallRegion in wallRegions)
        {
            if (wallRegion.Count < wallThresholdSize) // "Se a quantidade de tiles parede agrupadas for menor do que o threshold"
            {
                // Pinta todas as paredes como buracos
                foreach (Coord tile in wallRegion)
                {
                    map[tile.tileX,tile.tileY] = 0;
                }
            }
        }

        List<List<Coord>> roomRegions = GetRegions(0);
        int roomThresholdSize = 50;
        List<Room> survivingRooms = new List<Room>();

        foreach (List<Coord> roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomThresholdSize) // "Se a quantidade de tiles parede agrupadas for menor do que o threshold"
            {
                // Pinta todas as paredes como buracos
                foreach (Coord tile in roomRegion)
                {
                    map[tile.tileX,tile.tileY] = 1;
                }
            }
            else // No caso de não irmos remover os quartinhos, vamos guardar ele na lista de quartos vivos
            {
                survivingRooms.Add(new Room(roomRegion, map));
            }
        }

        survivingRooms.Sort();
        foreach (Room item in survivingRooms)
        {
            Debug.Log(item.roomSize);   
        }
        ConnectClosestRooms(survivingRooms);
    }

    void ConnectClosestRooms (List<Room> allRooms){
        int bestDistance = 0;
        Coord bestTileA = new Coord();
        Coord bestTileB = new Coord();
        Room bestRoomA = new Room();
        Room bestRoomB = new Room();
        bool possibleConnectionFound = false;

        foreach (Room roomA in allRooms){
            possibleConnectionFound = false;

            foreach (Room roomB in allRooms){
                if (roomA == roomB)
                {
                    continue;
                }
                if (roomA.IsConnected(roomB))
                {
                    possibleConnectionFound = false;
                    break;
                }
                // Vai comparar cada pontinho das bordas de cada "quarto", e a menor distância encontrada vai ser conectada
                for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA++){
                    for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++){
                        Coord tileA = roomA.edgeTiles[tileIndexA];
                        Coord tileB = roomB.edgeTiles[tileIndexB];
                        int distanceBetweenRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) + Mathf.Pow(tileA.tileY - tileB.tileY, 2));

                        if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
                        {
                            possibleConnectionFound = true;
                            bestDistance = distanceBetweenRooms;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = roomA;
                            bestRoomB = roomB;
                        }
                    }
                }
            }

            if (possibleConnectionFound)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }
    }

    void CreatePassage(Room roomA, Room roomB, Coord tileA, Coord tileB){
        Room.ConnectRooms(roomA, roomB); // Por isso que o método é estático!

        Debug.DrawLine(CoordToWorldPoint(tileA), CoordToWorldPoint(tileB), Color.blue, 100);
    }

    Vector3 CoordToWorldPoint(Coord tile){
        return new Vector3 (-width / 2 + .5f + tile.tileX, 2, -height / 2 + .5f + tile.tileY);
    }

    void RandomFillMap(){
        if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }

        System.Random pseudoRandom = new System.Random(seed.GetHashCode()); // Pseudo random number generator
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1) // Verificação pra preencher as bordas
                {
                    map [x, y] = 1;
                } else{
                    map[x, y] = pseudoRandom.Next(0, 100) < randomFillPercent ? 1 : 0;
                }
            }
        }
    }

    void SmoothMap(){
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neighbourWallTiles = GetSurroundingWallCount(x, y);

                if (neighbourWallTiles > 4)
                {
                    map[x, y] = 1;
                }
                else if (neighbourWallTiles < 4){
                    map[x, y] = 0;
                }
            }
        }
    }

    int GetSurroundingWallCount(int gridX, int gridY){
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (IsInMapRange(neighbourX, neighbourY))
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                        wallCount += map[neighbourX, neighbourY];
                    }
                } else{
                    wallCount++;
                }
            }
        }
        return wallCount;
    }

    struct Coord {
        public int tileX;
        public int tileY;

        public Coord (int x, int y) {
            tileX = x;
            tileY = y;
        }
    }

    List<Coord> GetRegionTiles(int startX, int startY){ // Parâmetro pega posição aleatória no mapa e preenche baseado no "pixel" escolhido (balde de tinta)
        List<Coord> tiles = new List<Coord>();
        int[,] mapFlags = new int[width, height];
        int tileType = map[startX, startY]; // O pixel aleatório escolhido foi chão ou parede?

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX, startY] = 1; // "ja olhei"

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue(); 
            tiles.Add(tile);

            for (int x = tile.tileX -1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY -1; y <= tile.tileY + 1; y++)
                {
                    if (IsInMapRange(x, y) && (y == tile.tileY || x == tile.tileX)) // GARANTINDO QUE NÃO FORMAM DIAGONAIS
                    {
                        if (mapFlags[x,y] == 0 && map[x,y] == tileType) // garantindo que não olhei pra o tile ainda, e que ele faz parte do mesmo grupo de coisas que quero pintar
                        {
                            mapFlags[x,y] = 1;
                            queue.Enqueue(new Coord(x,y));
                        }
                    }
                }
            }
        }
        return tiles;
    }

    List<List<Coord>> GetRegions(int tileType){ // Pega todas as regiões existentes de balde de tinta e coloca numa lista de regioes
        List<List<Coord>> regions = new List<List<Coord>>();
        int [,] mapFlags = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (mapFlags[x,y] == 0 && map[x,y] == tileType)
                {
                    List<Coord> newRegion = GetRegionTiles(x,y);
                    regions.Add(newRegion);

                    foreach (Coord tile in newRegion)
                    {
                        mapFlags[tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }

        return regions;
    }

    bool IsInMapRange(int x, int y){
        return x >= 0 && x < width && y >=0 && y < height;
    }

    class Room : IComparable<Room> { // Ao aplicar essa interface, agora os quartos são ordenáveis!!
        public List<Coord> tiles;
        public List<Coord> edgeTiles;
        public List<Room> connectedRooms;
        public int roomSize;

        public Room(){}
        public Room(List<Coord> roomTiles, int[,] map){
            tiles = roomTiles;
            roomSize = tiles.Count;
            connectedRooms = new List<Room>();

            edgeTiles = new List<Coord>();
            foreach (Coord tile in tiles){
                for (int x = tile.tileX - 1; x < tile.tileX + 1; x++){ // Loop que pega o tile antes do tile, e o tile depois do tile
                    for (int y = tile.tileY - 1; y < tile.tileY + 1; y++)
                    {
                        if (y == tile.tileY || x == tile.tileX) // Garantindo que não tá pegando diagonais
                        {
                            if (map[x,y] == 1) // Se encontrar uma parede
                            {
                                edgeTiles.Add(tile); // Significa que esse tile é uma borda
                            }
                        }
                    }
                }
            }
        }

        public static void ConnectRooms(Room roomA, Room roomB){
            roomA.connectedRooms.Add(roomB);
            roomB.connectedRooms.Add(roomA);
        }

        public bool IsConnected(Room otherRoom){
            return connectedRooms.Contains(otherRoom);
        }

        public int CompareTo(Room otherRoom) {
            return otherRoom.roomSize.CompareTo(roomSize); // Comparando e ordenando com base nessa comparação
        }
    }
}
