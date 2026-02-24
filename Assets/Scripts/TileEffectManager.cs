using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;

public class TileEffectManager : NetworkBehaviour
{
    public static TileEffectManager Instance { get; private set; }

   
    [SerializeField] private int numberOfEffectsToSpawn = 20;

    [SerializeField] private GameObject effectObject;

    private List<NetworkObject> spawnedEffects = new List<NetworkObject>();

   
    [Networked] public int totalExpectedPlayers { get; private set; } = 2;

    [Networked] public EffectType effectType { get; set; }

  
    private GameManager gameManager;

    public override void Spawned()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

      
        FetchTotalExpectedPlayers();

        if (!Object.HasStateAuthority || GameModeManager.Instance.IsClassicMode())
        {
            Debug.Log("[TileEffectManager] Không có StateAuthority hoặc ở chế độ Classic, bỏ qua spawn");
            return;
        }

        StartCoroutine(WaitUntilAllPlayersThenSpawn());
    }

    private void FetchTotalExpectedPlayers()
    {
        string roomId = UserSession.RoomId; 
        if (string.IsNullOrEmpty(roomId))
        {
            Debug.LogWarning("[TileEffectManager] Không tìm thấy roomId, dùng mặc định 2 players");
            totalExpectedPlayers = 2;
            return;
        }

        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        dbRef.Child("rooms").Child(roomId).Child("players")
            .GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.Result == null)
                {
                    Debug.LogWarning("[TileEffectManager] Lỗi đọc số người chơi, để mặc định 2");
                    totalExpectedPlayers = 2;
                }
                else if (task.IsCompleted)
                {
                    int playerCount = (int)task.Result.ChildrenCount;
                    totalExpectedPlayers = playerCount > 0 ? playerCount : 2;
                    Debug.Log($"[TileEffectManager] Gán totalExpectedPlayers = {totalExpectedPlayers}");
                }
            });
    }

    public void Update()
    {
        if (gameManager == null)
        {
           
            gameManager = FindFirstObjectByType<GameManager>();
        }
    }

    private IEnumerator WaitUntilAllPlayersThenSpawn()
    {
        Debug.Log("[TileEffectManager] Đang đợi tất cả người chơi tham gia...");

        while (!IsAllPlayersReady())
        {
            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log("[TileEffectManager] Tất cả người chơi đã sẵn sàng, bắt đầu spawn hiệu ứng.");
        yield return new WaitForSeconds(0.5f);

        StartCoroutine(SpawnEffectTiles());
        StartCoroutine(MonitorEffects());
    }

    private bool IsAllPlayersReady()
    {
        return Runner.ActivePlayers.Count() >= totalExpectedPlayers;
    }

    private bool IsInvalidTile(Transform tile)
    {
        string tileName = tile.name.ToLower();
        return tileName.Contains("start point") || tileName.Contains("finish point");
    }

    private bool HasPlayerOnTile(int tileIndex)
    {
        if (gameManager == null)
        {
            Debug.LogWarning("[TileEffectManager] GameManager is null, cannot check for pieces!");
            return false;
        }

        List<Piece> pieces = gameManager.GetPiecesAtTile(tileIndex, null, false);
        if (pieces.Count > 0)
        {
            Debug.Log($"[TileEffectManager] Ô {tileIndex} có {pieces.Count} quân cờ, không spawn hiệu ứng.");
            return true;
        }
        return false;
    }

    IEnumerator SpawnEffectTiles()
    {
        yield return new WaitForSeconds(0.2f);

        if (GameBoard.Instance == null || GameBoard.Instance.commonPath == null)
        {
            Debug.LogError("[TileEffectManager] GameBoard hoặc commonPath là null!");
            yield break;
        }

        var path = GameBoard.Instance.commonPath;
        int totalTiles = path.Count;

        if (totalTiles == 0)
        {
            Debug.LogError("[TileEffectManager] CommonPath rỗng hoặc chưa được khởi tạo!");
            yield break;
        }

        List<int> usedIndices = new List<int>();

        for (int i = 0; i < numberOfEffectsToSpawn; i++)
        {
            int randomIndex;
            Transform tile;
            do
            {
                randomIndex = Random.Range(0, totalTiles);
                tile = path[randomIndex];
            } while (usedIndices.Contains(randomIndex) || IsInvalidTile(tile) || HasPlayerOnTile(randomIndex));

            usedIndices.Add(randomIndex);

            Vector3 spawnPos = GetTileCenterPosition(tile, 0f);
            NetworkObject obj = Runner.Spawn(effectObject, spawnPos, Quaternion.identity, inputAuthority: null);

            spawnedEffects.Add(obj);

            EffectType randomEffect = GetRandomEffectType1();
            CheckTrigger trigger = obj.GetComponent<CheckTrigger>();
            if (trigger != null)
            {
                trigger.SetEffectType(randomEffect);
                Debug.Log($"[TileEffectManager] Gán hiệu ứng: {randomEffect} cho object {obj.name} tại ô {randomIndex}");
            }
            else
            {
                Debug.LogWarning("[TileEffectManager] Không tìm thấy CheckTrigger trên effectObject!");
            }
        }
    }

    private IEnumerator MonitorEffects()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f);

            spawnedEffects.RemoveAll(obj => obj == null || !obj.IsValid);

            if (spawnedEffects.Count < numberOfEffectsToSpawn / 2)
            {
                Debug.Log("[TileEffectManager] Số hiệu ứng còn lại dưới một nửa, đang spawn lại...");
                yield return StartCoroutine(RespawnEffectTiles());
            }
        }
    }

    private IEnumerator RespawnEffectTiles()
    {
        if (GameBoard.Instance == null || GameBoard.Instance.commonPath == null)
        {
            Debug.LogError("[TileEffectManager] GameBoard hoặc commonPath là null!");
            yield break;
        }

        var path = GameBoard.Instance.commonPath;
        int totalTiles = path.Count;

        if (totalTiles == 0)
        {
            Debug.LogError("[TileEffectManager] CommonPath rỗng hoặc chưa được khởi tạo!");
            yield break;
        }

        int effectsToSpawn = numberOfEffectsToSpawn - spawnedEffects.Count;
        List<int> usedIndices = new List<int>();

        foreach (var effect in spawnedEffects)
        {
            if (effect != null && effect.IsValid)
            {
                Vector3 effectPos = effect.transform.position;
                for (int i = 0; i < path.Count; i++)
                {
                    if (Vector3.Distance(GetTileCenterPosition(path[i], 0f), effectPos) < 0.1f)
                    {
                        usedIndices.Add(i);
                        break;
                    }
                }
            }
        }

        for (int i = 0; i < effectsToSpawn; i++)
        {
            int randomIndex;
            Transform tile;
            do
            {
                randomIndex = Random.Range(0, totalTiles);
                tile = path[randomIndex];
            } while (usedIndices.Contains(randomIndex) || IsInvalidTile(tile) || HasPlayerOnTile(randomIndex));

            usedIndices.Add(randomIndex);

            Vector3 spawnPos = GetTileCenterPosition(tile, 0f);
            NetworkObject obj = Runner.Spawn(effectObject, spawnPos, Quaternion.identity, inputAuthority: null);

            spawnedEffects.Add(obj);

            EffectType randomEffect = GetRandomEffectType1();
            CheckTrigger trigger = obj.GetComponent<CheckTrigger>();
            if (trigger != null)
            {
                trigger.SetEffectType(randomEffect);
                Debug.Log($"[TileEffectManager] Spawn lại hiệu ứng: {randomEffect} cho object {obj.name} tại ô {randomIndex}");
            }
            else
            {
                Debug.LogWarning("[TileEffectManager] Không tìm thấy CheckTrigger trên effectObject!");
            }
        }
    }

    private Vector3 GetTileCenterPosition(Transform tile, float offsetY)
    {
        Collider col = tile.GetComponent<Collider>();
        Vector3 center = col != null ? col.bounds.center : tile.position;
        return center + Vector3.up * offsetY;
    }

    private EffectType GetRandomEffectType()
    {
        System.Array values = System.Enum.GetValues(typeof(EffectType));
        return (EffectType)values.GetValue(Random.Range(0, values.Length));
    }

    private EffectType GetRandomEffectType1()
    {
        EffectType[] positiveEffects = new EffectType[]
        {
            EffectType.MoveForward1,
            EffectType.MoveForward2,
            EffectType.MoveForward3,
            EffectType.MoveForward4,
            EffectType.MoveForward5,
            EffectType.AutoSpawn1Piece,
            EffectType.HasShieldBlockKick,
            EffectType.StartRotatePiece,
            EffectType.RandomOpponentMoveFoward1,
            EffectType.RandomOpponentMoveFoward2,
            EffectType.RandomOpponentMoveFoward3,
            EffectType.RandomOpponentMoveFoward4,
            EffectType.RandomOpponentMoveFoward5,
        };

        EffectType[] negativeEffects = new EffectType[]
        {
            EffectType.MoveBackward1,
            EffectType.MoveBackward2,
            EffectType.MoveBackward3,
            EffectType.MoveBackward4,
            EffectType.MoveBackward5,
            EffectType.SendToBase,
            EffectType.KickRandomOpponent,
            EffectType.ReturnToSpawnPoint,
            EffectType.SpawnAndSwapRandomOpponentToBase,
            EffectType.Effect_HasBomb,
            EffectType.RandomOpponentMoveBackward1,
            EffectType.RandomOpponentMoveBackward2,
            EffectType.RandomOpponentMoveBackward3,
            EffectType.RandomOpponentMoveBackward4,
            EffectType.RandomOpponentMoveBackward5,
            EffectType.SwapRandomOpponent,
            EffectType.FlyToGoalDoor,
        };

        List<EffectType> weightedEffects = new List<EffectType>();
        for (int i = 0; i < 8; i++) weightedEffects.AddRange(positiveEffects);
        for (int i = 0; i < 2; i++) weightedEffects.AddRange(negativeEffects);

        return weightedEffects[Random.Range(0, weightedEffects.Count)];
    }

    private EffectType GetRandomEffectType2()
    {
        Dictionary<EffectType, float> effectWeights = new Dictionary<EffectType, float>
        {
            { EffectType.MoveForward1, 0.10f },
            { EffectType.MoveForward2, 0.10f },
            { EffectType.MoveForward3, 0.10f },
            { EffectType.MoveForward4, 0.10f },
            { EffectType.MoveForward5, 0.10f },
            { EffectType.AutoSpawn1Piece, 0.10f },
            { EffectType.HasShieldBlockKick, 0.15f },
            { EffectType.StartRotatePiece, 0.15f },

            { EffectType.MoveBackward1, 0.02f },
            { EffectType.MoveBackward2, 0.02f },
            { EffectType.MoveBackward3, 0.02f },
            { EffectType.MoveBackward4, 0.02f },
            { EffectType.MoveBackward5, 0.02f },
            { EffectType.SendToBase, 0.02f },
            { EffectType.KickRandomOpponent, 0.01f },
            { EffectType.ReturnToSpawnPoint, 0.01f },
            { EffectType.SwapRandomOpponent, 0.01f },
            { EffectType.SpawnAndSwapRandomOpponentToBase, 0.01f },
            { EffectType.Effect_HasBomb, 0.01f },
            { EffectType.RandomOpponentMoveFoward1, 0.01f },
            { EffectType.RandomOpponentMoveFoward2, 0.01f },
            { EffectType.RandomOpponentMoveFoward3, 0.01f },
            { EffectType.RandomOpponentMoveFoward4, 0.01f },
            { EffectType.RandomOpponentMoveFoward5, 0.01f },
            { EffectType.RandomOpponentMoveBackward1, 0.01f },
            { EffectType.RandomOpponentMoveBackward2, 0.01f },
            { EffectType.RandomOpponentMoveBackward3, 0.01f },
            { EffectType.RandomOpponentMoveBackward4, 0.01f },
            { EffectType.RandomOpponentMoveBackward5, 0.01f }
        };

        float totalWeight = effectWeights.Values.Sum();
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        foreach (var pair in effectWeights)
        {
            currentWeight += pair.Value;
            if (randomValue <= currentWeight)
            {
                return pair.Key;
            }
        }

        return EffectType.MoveForward1;
    }
}

public enum EffectType
{
    MoveForward1,
    MoveForward2,
    MoveForward3,
    MoveForward4,
    MoveForward5,
    MoveBackward1,
    MoveBackward2,
    MoveBackward3,
    MoveBackward4,
    MoveBackward5,
    SendToBase,
    KickRandomOpponent,
    AutoSpawn1Piece,
    ReturnToSpawnPoint,
    HasShieldBlockKick,
    StartRotatePiece,
    SwapRandomOpponent,
    SpawnAndSwapRandomOpponentToBase,
    Effect_HasBomb,
    RandomOpponentMoveFoward1,
    RandomOpponentMoveFoward2,
    RandomOpponentMoveFoward3,
    RandomOpponentMoveFoward4,
    RandomOpponentMoveFoward5,
    RandomOpponentMoveBackward1,
    RandomOpponentMoveBackward2,
    RandomOpponentMoveBackward3,
    RandomOpponentMoveBackward4,
    RandomOpponentMoveBackward5,
    FlyToGoalDoor,
}
