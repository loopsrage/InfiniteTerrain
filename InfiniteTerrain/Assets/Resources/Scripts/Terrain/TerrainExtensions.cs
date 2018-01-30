using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class TerrainExtensions 
{
    public enum Direction
    {
        UP,
        Down,
        Left,
        Right
    }
    public static GameObject CreateTerrain(Vector3 Position,
        Terrain PreviousTerrain = null,
        Direction direction = Direction.UP,
        Dictionary<Direction,GameObject> Neighbors = null
        )
    {
        TerrainData NewTerrainData = new TerrainData();
        // TerrainData settings
        NewTerrainData.heightmapResolution = GameMaster.gameMaster.terrainSettings.HeightMapResolution;
        NewTerrainData.size = GameMaster.gameMaster.terrainSettings.MapSize;
        NewTerrainData.GenerateHeights(PreviousTerrain,direction);

        GameObject NewTerrain = Terrain.CreateTerrainGameObject(NewTerrainData);
        // GameObject Settings
        NewTerrain.transform.position = Position;
        NewTerrain.AddComponent<TerrainScript>();
        // Terrain Settings
        Terrain ThisTerrain = NewTerrain.GetComponent<Terrain>();

        GameMaster.gameMaster.terrainManager.ExistingTerrains.Add(Position);
        GameMaster.gameMaster.terrainManager.Terrains.Add(NewTerrain);
        return NewTerrain;
    }
    public static float[,] GetHeights(this Terrain terrain)
    {
        TerrainData terrainData = terrain.terrainData;
        return terrainData.GetHeights(0,0,terrainData.heightmapWidth,terrainData.heightmapHeight);
    }
    public static void GenerateHeights(this TerrainData terrainData,
        Terrain PreviousTerrain = null,
        Direction direction = Direction.UP
        )
    {
        float[,] NewHeights = new float[terrainData.heightmapWidth,terrainData.heightmapHeight];
        float[,] PreviousHeights;
        float[,] UpHeight;
        if (PreviousTerrain != null)
        {
            PreviousHeights = PreviousTerrain.GetHeights();
        }
        else
        {
            PreviousHeights = new float[0, 0];
        }
        float RandomNoise = Random.Range(50f,300f);
        for (int x = 0; x <= terrainData.heightmapResolution - 1; x++)
        {
            for (int z = 0; z <= terrainData.heightmapResolution - 1; z++)
            {
                float NewHeight;
                if (PreviousTerrain != null)
                {
                    switch (direction)
                    {
                        case Direction.UP:
                            NewHeights[0, z] = PreviousHeights[terrainData.heightmapResolution - 1, z];
                            break;
                        case Direction.Down:
                            NewHeights[terrainData.heightmapResolution - 1, z] = PreviousHeights[0, z];
                            break;
                        case Direction.Left:
                            NewHeights[z, terrainData.heightmapResolution - 1] = PreviousHeights[z, 0];
                            break;
                        case Direction.Right:
                            NewHeights[z, 0] = PreviousHeights[z, terrainData.heightmapResolution - 1];
                            break;
                        default:
                            break;
                    }
                    if (x <= terrainData.heightmapResolution - 2 && z <= terrainData.heightmapResolution - 2)
                    {
                        NewHeight = Mathf.PerlinNoise(x / RandomNoise, z / RandomNoise);
                        float Height1 = NewHeights[x, z];
                        float Height2 = Mathf.SmoothStep(Height1,1f,0.0005f);
                        NewHeights[x + 1, z + 1] = Mathf.Clamp(Mathf.Lerp(Height2,Mathf.PerlinNoise(Height1 + x / RandomNoise, Height1 + z / RandomNoise),Height1),0.1f,1f);
                    }

                }
                else if (PreviousTerrain == null)
                {
                   NewHeight = Mathf.PerlinNoise(x / RandomNoise, z  / RandomNoise);
                   NewHeights[x, z] = NewHeight;
                }
            }
        }
        terrainData.SetHeights(0,0,NewHeights);
    }
}
