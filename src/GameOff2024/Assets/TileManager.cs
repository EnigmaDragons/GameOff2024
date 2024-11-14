using System.Collections.Generic;
using UnityEngine;
using DunGen;
using System.Xml.Serialization;
using System.Linq;
using System.Collections;
using DunGen.Adapters;
public class TileManager : MonoBehaviour
{
    public static TileManager instance;
    TileGraph graph;
    RuntimeDungeon runtimeDungeon;
    DungeonGenerator dungeonGenerator;
    Tile[] allTiles;
    Tile[] activeTiles = new Tile[2];
    bool isReadyToRumble = false;
    private void Awake()
    {
        // Ensure only one instance of DoorGraph exists
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnEnable()
    {
        runtimeDungeon = GetComponent<RuntimeDungeon>();
        graph = new TileGraph();
        if (runtimeDungeon != null)
        {
            dungeonGenerator = runtimeDungeon.Generator;
            GetComponent<UnityNavMeshAdapter>().OnNavmeshBaked += Instance_OnNavmeshBaked;
        }
    }

    private void Instance_OnNavmeshBaked(object sender, System.EventArgs e)
    {
        allTiles = runtimeDungeon.Root.GetComponentsInChildren<Tile>();
        graph.InitializeConnections(allTiles);
        activeTiles[0] = allTiles[0];
        activeTiles[1] = allTiles[0];
        isReadyToRumble = true;
        SetNewActiveTile(allTiles[0], false);
    }

    protected virtual void OnDisable()
    {
        UnityNavMeshAdapter.instance.OnNavmeshBaked -= Instance_OnNavmeshBaked;

    }
    private void Clear()
    {
        graph.Clear();
    }

    public void SetNewActiveTile(Tile tile, bool isPlayer)
    {
        if (isPlayer)
        {
            activeTiles[0] = tile;
        }
        else
        {
            activeTiles[1] = tile;
        }
        if(isReadyToRumble)
            StartCoroutine("SetActiveRooms");
    }
    private IEnumerator SetActiveRooms()
    {
        DoorConnections playerConnections = graph.GetConnections(activeTiles[0]);
        DoorConnections spyConnections = graph.GetConnections(activeTiles[1]);
        List<Tile> allActiveRooms = playerConnections.PrimaryConnections
            .Union(spyConnections.PrimaryConnections)
            .Union(playerConnections.SecondaryConnections)
            .Union(spyConnections.SecondaryConnections)
            .Union(activeTiles)
            .ToList();

        for(int i = 0; i<allTiles.Length; i++)
        {
            Tile tile = allTiles[i];
            if(tile != null) 
            {
                if(allActiveRooms.Contains(tile))
                {
                    tile.LoadInterior();
                }
                else
                {
                    tile.UnloadInterior();
                }
            }
            if(i%5 == 0)
            {
                yield return null;
            }
        }
    }

}
public class TileGraph
{
    // Dictionary to store connections for each Tile
    private Dictionary<Tile, DoorConnections> tileConnections = new Dictionary<Tile, DoorConnections>();

    

    protected DungeonGenerator dungeonGenerator;
    RuntimeDungeon runtimeDungeon;

    public void Clear()
    {
        tileConnections.Clear();
    }

    // Method to initialize connections for a list of tiles
    public void InitializeConnections(Tile[] tiles)
    {
        foreach (var tile in tiles)
        {
            if (!tileConnections.ContainsKey(tile))
            {
                tileConnections[tile] = new DoorConnections(tile);
            }

            // Set primary connections based on the tile's doorways
            SetPrimaryConnections(tile);
            Debug.Log(tileConnections[tile].PrimaryConnections.Count());
        }

        // After primary connections are set, add secondary connections
        foreach (var tile in tiles)
        {
            SetSecondaryConnections(tile);
        }
    }

    private void SetPrimaryConnections(Tile tile)
    {
        DoorConnections connections = tileConnections[tile];
        connections.PrimaryConnections.Clear();

        // Find all tiles that connect directly to this tile via doorways
        foreach (var doorway in tile.AllDoorways)
        {
            if (doorway.ConnectedDoorway != null && doorway.ConnectedDoorway.Tile != tile)
            {
                Tile connectedTile = doorway.ConnectedDoorway.Tile;
                if (!connections.PrimaryConnections.Contains(connectedTile))
                {
                    connections.PrimaryConnections.Add(connectedTile);
                }
            }
        }
    }

    private void SetSecondaryConnections(Tile tile)
    {
        DoorConnections connections = tileConnections[tile];
        connections.SecondaryConnections.Clear();

        // Find secondary connections (tiles that connect to primary connections)
        foreach (var primaryTile in connections.PrimaryConnections)
        {
            DoorConnections primaryConnections = tileConnections[primaryTile];

            foreach (var secondaryTile in primaryConnections.PrimaryConnections)
            {
                // Exclude the base tile and primary connections from secondary connections
                if (secondaryTile != tile && !connections.PrimaryConnections.Contains(secondaryTile) && !connections.SecondaryConnections.Contains(secondaryTile))
                {
                    connections.SecondaryConnections.Add(secondaryTile);
                }
            }
        }
    }

    // Method to get DoorConnections for a specific tile
    public DoorConnections GetConnections(Tile tile)
    {
        return tileConnections.TryGetValue(tile, out var connections) ? connections : null;
    }
}

// Data structure to store connections for each tile
public class DoorConnections
{
    public Tile Tile { get; private set; }
    public List<Tile> PrimaryConnections { get; private set; }
    public List<Tile> SecondaryConnections { get; private set; }

    public DoorConnections(Tile tile)
    {
        Tile = tile;
        PrimaryConnections = new List<Tile>();
        SecondaryConnections = new List<Tile>();
    }
}
