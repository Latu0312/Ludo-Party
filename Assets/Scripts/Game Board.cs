using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
  
    public static GameBoard Instance;

   
    public List<Transform> commonPath = new List<Transform>();

   
    public int redStartIndex;
    public int blueStartIndex;
    public int greenStartIndex;
    public int yellowStartIndex;

    public Transform redSpawnPoint;
    public Transform blueSpawnPoint;
    public Transform greenSpawnPoint;
    public Transform yellowSpawnPoint;

   
    public List<Transform> redHomeSlots = new List<Transform>();  
    public List<Transform> blueHomeSlots = new List<Transform>();
    public List<Transform> greenHomeSlots = new List<Transform>();
    public List<Transform> yellowHomeSlots = new List<Transform>();

    
    public List<Transform> redGoalPath = new List<Transform>();    
    public List<Transform> blueGoalPath = new List<Transform>();
    public List<Transform> greenGoalPath = new List<Transform>();
    public List<Transform> yellowGoalPath = new List<Transform>();

    public GameObject highlightPrefab;
    public float highlightOffsetY = 3f;

    private Dictionary<Transform, GameObject> highlightMap = new Dictionary<Transform, GameObject>();


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SpawnHighlights(commonPath);
        SpawnHighlights(redGoalPath);
        SpawnHighlights(blueGoalPath);
        SpawnHighlights(greenGoalPath);
        SpawnHighlights(yellowGoalPath);
        
    }

    private void SpawnHighlights(List<Transform> path)
    {
        foreach (Transform tile in path)
        {
            Vector3 spawnPos = GetTileCenterPosition(tile, highlightOffsetY);
            GameObject hl = Instantiate(highlightPrefab, spawnPos, Quaternion.Euler(90f, 0f, 0f), tile);
            hl.SetActive(false); 
            highlightMap[tile] = hl;
        }
    }

    private Vector3 GetTileCenterPosition(Transform tile, float offsetY)
    {
        Collider col = tile.GetComponent<Collider>();
        Vector3 center = col != null ? col.bounds.center : tile.position;
        return center + Vector3.up * offsetY;
    }

    public void HighlightTiles(List<Transform> tiles, bool state)
    {
        foreach (Transform tile in tiles)
        {
            if (highlightMap.TryGetValue(tile, out GameObject hl))
            {
                hl.SetActive(state);
            }
        }
    }

  
    public void HideAllHighlights()
    {
        foreach (var kvp in highlightMap)
        {
            kvp.Value.SetActive(false);
        }
    }
}
