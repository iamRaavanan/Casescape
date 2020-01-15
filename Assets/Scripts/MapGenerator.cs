using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour {

    public int m_width;
    public int m_height;

    public string m_seed;
    public bool m_useRandomSeed;

    [Range(0, 100)]
    public int m_randomFillPercent;
    int[,] map;

    void Start() {
        GenerateMap();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //Destroy(GetComponentInChildren<MeshCollider>());
            //GenerateMap();
        }
    }

    public void UpdateLevel() {
        Destroy(GetComponentInChildren<MeshCollider>());
        GenerateMap();
    }

    void GenerateMap() {
        map = new int[m_width, m_height];
        RandomFillMap();

        for (int i = 0; i < 5; i++)
        {
            SmoothMap();
        }
        ProcessMap ();
        int borderSize = 1;
        int[,] borderMap = new int[m_width + borderSize * 2, m_height + borderSize * 2];

        for (int x = 0; x < borderMap.GetLength(0); x++)
        {
            for (int y = 0; y < borderMap.GetLength(1); y++)
            {
                if (x > borderSize && x < (m_width + borderSize) && y > borderSize && y < (m_height + borderSize))
                    borderMap[x, y] = map[x - borderSize, y - borderSize];
                else
                    borderMap[x, y] = 1;
            }
        }
        MeshGenerator meshgen = GetComponent<MeshGenerator>();
        meshgen.GenerateMesh(borderMap, 1);
    }

    private void ProcessMap ()
    {
        List<List<Coord>> wallRegions = GetRegions(1);
        int wallThreshold = 200;
        foreach (List<Coord> wallRegion in wallRegions)
        {
            if(wallRegion.Count < wallThreshold)
            {
                foreach (Coord tile in wallRegion)
                {
                    map[tile.tileX, tile.tileY] = 0;
                }
            }
        }

        List<List<Coord>> roomRegions = GetRegions(0);
        List<Room> availableRooms = new List<Room>();
        int roomThreshold = 50;
        foreach (List<Coord> roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomThreshold)
            {
                foreach (Coord tile in roomRegion)
                {
                    map[tile.tileX, tile.tileY] = 1;
                }
            }
            else
            {
                availableRooms.Add(new Room(roomRegion, map));
            }
        }

        availableRooms.Sort();
        availableRooms[0].isMainRoom = true;
        availableRooms[0].isAccessibleFromMainRoom = true;

        ConnectCloseRoom(availableRooms);
    }

    private void ConnectCloseRoom (List<Room> Rooms, bool forceAccessibleFromMainRoom = false)
    {
        List<Room> roomListA = new List<Room>();
        List<Room> roomListB = new List<Room>();

        if (forceAccessibleFromMainRoom)
        {
            foreach (Room room in Rooms)
            {
                if (room.isAccessibleFromMainRoom)
                    roomListB.Add(room);
                else
                    roomListA.Add(room);
            }
        }
        else
            roomListA = roomListB = Rooms;

        int bestDistance = 0;
        Coord bestTileA = new Coord();
        Coord bestTileB = new Coord();
        Room bestRoomA = new Room();
        Room bestRoomB = new Room();
        bool bestConnectionFound = false;

        foreach (Room RoomA in roomListA)
        {
            if(!forceAccessibleFromMainRoom)
            {
                bestConnectionFound = false;
                if (RoomA.connectedRooms.Count > 0)
                    continue;
            }
                
            foreach (Room RoomB in roomListB)
            {
                if (RoomA == RoomB || RoomA.IsConnected (RoomB))
                    continue;
                /*
                if (RoomA.IsConnected(RoomB))
                {
                    bestConnectionFound = false;
                    break;
                }
                */
                for(int tileIndexA = 0; tileIndexA < RoomA.edgeTiles.Count; tileIndexA++)
                {
                    for (int tileIndexB = 0; tileIndexB < RoomB.edgeTiles.Count; tileIndexB++)
                    {
                        Coord tileA = RoomA.edgeTiles[tileIndexA];
                        Coord tileB = RoomB.edgeTiles[tileIndexB];
                        int distanceBWRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) + Mathf.Pow(tileA.tileY - tileB.tileY, 2));

                        if(distanceBWRooms < bestDistance || !bestConnectionFound)
                        {
                            bestDistance = distanceBWRooms;
                            bestConnectionFound = true;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = RoomA;
                            bestRoomB = RoomB;
                        }
                    }
                }
            }
            if (bestConnectionFound && !forceAccessibleFromMainRoom)
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
        }
        if (bestConnectionFound && forceAccessibleFromMainRoom)
        {
            CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            ConnectCloseRoom(Rooms, true);
        }
        if (!forceAccessibleFromMainRoom)
            ConnectCloseRoom(Rooms, true);
    }

    private void CreatePassage (Room roomA, Room roomB, Coord tileA, Coord tileB)
    {
        Room.ConnectedRoom(roomA, roomB);
        //Debug.DrawLine(CoordWorldToPoint(tileA), CoordWorldToPoint(tileB), Color.green, 100);

        List<Coord> line = GetLine(tileA, tileB);
        foreach (Coord c in line)
        {
            DrawCircle(c, 5);
        }
    }

    private void DrawCircle (Coord c, int r)
    {
        for (int x = -r; x < r; x++)
        {
            for (int y = -r; y < r; y++)
            {
                if(x*x + y*y <= r*r)
                {
                    int drawX = c.tileX + x;
                    int drawy = c.tileY + y;
                    if (IsInMapRange(drawX, drawy))
                        map[drawX, drawy] = 0;
                }
            }
        }
    }

    private List<Coord> GetLine (Coord from, Coord to)
    {
        List<Coord> line = new List<Coord>();
        int x = from.tileX;
        int y = from.tileY;
        int dx = to.tileX - from.tileX;
        int dy = to.tileY - from.tileY;

        bool isInverted = false;
        int step = Math.Sign(dx);
        int gradientStep = Math.Sign(dy);

        int longest = Math.Abs(dx);
        int shortest = Math.Abs(dy);

        if(longest < shortest)
        {
            isInverted = true;
            longest = Math.Abs(dy);
            shortest = Math.Abs(dx);
            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
        }

        int gradientAccumulation = longest / 2;
        for (int i = 0; i < longest; i++)
        {
            line.Add(new Coord(x, y));
            if (isInverted)
                y += step;
            else
                x += step;

            gradientAccumulation += shortest;
            if(gradientAccumulation >= longest)
            {
                if (isInverted)
                    x += gradientStep;
                else
                    y += gradientStep;
                gradientAccumulation -= longest;
            }
        }
        return line;
    }

    private Vector3 CoordWorldToPoint (Coord tile)
    {
        return new Vector3(-m_width / 2 + 0.5f + tile.tileX, 2, -m_height / 2 + 0.5f + tile.tileY);
    }

    List<List<Coord>> GetRegions (int tileType)
    {
        List<List<Coord>> regions = new List<List<Coord>>();
        int[,] mapFlags = new int[m_width, m_height];

        for (int x = 0; x < m_width; x++)
        {
            for (int y = 0; y < m_height; y++)
            {
                if(mapFlags[x,y] == 0 && map[x,y] == tileType)
                {
                    List<Coord> newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);
                    foreach (Coord tiles in newRegion)
                    {
                        mapFlags[tiles.tileX, tiles.tileY] = 1;
                    }
                }
            }
        }
        return regions;
    }

    List<Coord> GetRegionTiles (int startX, int startY)
    {
        List<Coord> tiles = new List<Coord>();
        int[,] mapFlags = new int[m_width, m_height];
        int tileType = map[startX, startY];

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX, startY] = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            tiles.Add(tile);
            for (int x = tile.tileX-1; x <= tile.tileX+1; x++)
            {
                for (int y = tile.tileY-1; y <= tile.tileY+1; y++)
                {
                    if(IsInMapRange (x,y) && (x == tile.tileX || y == tile.tileY))
                    {
                        if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                        {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new Coord(x,y));
                        }
                    }
                }
            }
        }
        return tiles;
    }


    private bool IsInMapRange (int x, int y)
    {
        return x >= 0 && x < m_width && y >= 0 && y < m_height;
    }
    void RandomFillMap()
    {
        if (m_useRandomSeed)
        {
            m_seed = Time.time.ToString();
        }
        System.Random prng = new System.Random(m_seed.GetHashCode());

        for (int i = 0; i < m_width; i++)
        {
            for (int j = 0; j < m_height; j++)
            {
                if (i == 0 || i == m_width - 1 || j == 0 || j == m_height - 1)
                    map[i, j] = 1;
                else
                    map[i, j] = (prng.Next(0, 100) < m_randomFillPercent) ? 1 : 0;
            }
        }
    }

    void SmoothMap()
    {
        for (int i = 0; i < m_width; i++)
        {
            for (int j = 0; j < m_height; j++)
            {
                int neighbourWallcount = GetSurroundingWallCount(i, j);

                if (neighbourWallcount > 4)
                    map[i, j] = 1;
                else if (neighbourWallcount < 4)
                    map[i, j] = 0;
            }
        }
    }

    int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int neighboutX = gridX - 1; neighboutX <= gridX + 1; neighboutX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (IsInMapRange (neighboutX, neighbourY))
                {
                    if (neighboutX != gridX || neighbourY != gridY)
                    {
                        wallCount += map[neighboutX, neighbourY];
                    }
                }
                else
                {
                    wallCount++;
                }
            }
        }
        return wallCount;
    }

    struct Coord
    {
        public int tileX;
        public int tileY; 

        public Coord (int x, int y)
        {
            tileX = x;
            tileY = y;
        }
    }

    class Room : IComparable<Room>
    {
        public List<Coord> tiles;
        public List<Coord> edgeTiles;
        public List<Room> connectedRooms;
        public int roomSize;
        public bool isAccessibleFromMainRoom;
        public bool isMainRoom;

        public Room()
        {

        }
        public Room (List<Coord> roomTiles, int[,] map)
        {
            tiles = roomTiles;
            roomSize = tiles.Count;
            edgeTiles = new List<Coord>();
            connectedRooms = new List<Room>();

            foreach (Coord tile in tiles)
            {
                for (int x = tile.tileX-1; x <= tile.tileX + 1; x++)
                {
                    for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                    {
                        if (x == tile.tileX || y == tile.tileY)
                        {
                            if (map[x, y] == 1)
                                edgeTiles.Add(tile);
                        }
                    }
                }
            }
        }

        public void SetAccessibleFromMainRoom ()
        {
            if (!isAccessibleFromMainRoom)
            {
                isAccessibleFromMainRoom = true;
                foreach (Room rooms in connectedRooms)
                {
                    rooms.SetAccessibleFromMainRoom();
                }
            }
        }

        public static void ConnectedRoom (Room RoomA, Room RoomB)
        {
            if (RoomA.isAccessibleFromMainRoom)
                RoomB.SetAccessibleFromMainRoom();
            else if (RoomB.isAccessibleFromMainRoom)
                RoomA.SetAccessibleFromMainRoom();

                RoomA.connectedRooms.Add(RoomB);
            RoomB.connectedRooms.Add(RoomA);
        }
        public bool IsConnected (Room otherRoom)
        {
            return connectedRooms.Contains(otherRoom);
        }

        public int CompareTo (Room otherRoom)
        {
            return otherRoom.roomSize.CompareTo(roomSize);
        }
    }
}