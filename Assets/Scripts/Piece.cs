using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static Fusion.Sockets.NetBitBuffer;

public class Piece : NetworkBehaviour
{
   
    [Networked] public PieceState State { get; set; } = PieceState.InBase;
    
    [Networked] public int CurrentTileIndex { get; set; } = -1;
    
    [Networked] public PlayerColor Color { get; set; } = PlayerColor.Red;

    private GameBoard Board => GameBoard.Instance;
    public List<Transform> commonPath => Board.commonPath;

    TurnManager turnManager;
    DiceManager diceManager;
    GameManager gameManager;
    PlayerTurnState playerTurnState;
    RankingManager rankingManager; 
    SoundManager soundManager; 

   
    private Vector3 initialBasePosition;

  
    public GameObject highlightObject; 
    public GameObject finishedObject; 
    public GameObject highlightObject2; 

 
    public GameObject ShieldObject; 
    [Networked] public bool HasShield { get; set; } = false;

   
    public GameObject BombObject; 
    [Networked] public bool HasBomb { get; set; } = false; 

    private bool isSpinning = false;

    Rigidbody rb;

   
    public Material highlightMaterial;
    public GameObject pieceModel; 
    private Material originalMaterial; 

    public override void Spawned()
    {
        rb = GetComponent<Rigidbody>();

        if (Object.HasStateAuthority) 
        {
            Color = Object.InputAuthority.PlayerId switch
            {
                1 => PlayerColor.Red,
                2 => PlayerColor.Blue,
                3 => PlayerColor.Green,
                4 => PlayerColor.Yellow,
                _ => PlayerColor.Red
            };
        }

        originalMaterial = pieceModel.GetComponent<Renderer>().material;

        StartCoroutine(RegisterWhenReady());

       
        initialBasePosition = transform.position + new Vector3(0,1,0);

        highlightObject2.SetActive(false);
    }

    private IEnumerator RegisterWhenReady()
    {
        yield return new WaitUntil(() => gameManager != null);

        gameManager.RegisterPiece(this);
    }

    public void Update()
    {
        if (turnManager == null)
        {
            turnManager = GameObject.FindFirstObjectByType<TurnManager>();
        }
        if (diceManager == null)
        {
            diceManager = GameObject.FindFirstObjectByType<DiceManager>();
        }
        if (gameManager == null)
        {
            gameManager = GameObject.FindFirstObjectByType<GameManager>();
        }
        if (playerTurnState == null)
        {
            playerTurnState = GameObject.FindFirstObjectByType<PlayerTurnState>();
        }
        if (rankingManager == null)
        {
            rankingManager = GameObject.FindFirstObjectByType<RankingManager>();
        }
        if (soundManager == null)
        {
            soundManager = GameObject.FindFirstObjectByType<SoundManager>();
        }
    }

    private int StartIndex => Object.InputAuthority.PlayerId switch
    {
        1 => Board.redStartIndex,
        2 => Board.blueStartIndex,
        3 => Board.greenStartIndex,
        4 => Board.yellowStartIndex,
        _ => 0
    };

    private List<Transform> GoalPath => Object.InputAuthority.PlayerId switch
    {
        1 => Board.redGoalPath,
        2 => Board.blueGoalPath,
        3 => Board.greenGoalPath,
        4 => Board.yellowGoalPath,
        _ => null
    };

    public void TryMove(int diceValue)
    {
        if (!Object.HasInputAuthority)
        {
            Debug.LogWarning("Không có quyền điều khiển quân này!");
            return;
        }

        if (diceManager.LastDiceValue == 6)
        {
            playerTurnState.RPC_SetMoved(false); 
        }

        diceManager.LastDiceValue = 0; 

        
        if (playerTurnState != null && playerTurnState.HasMoved)
        {
            Debug.Log("Bạn đã di chuyển trong lượt này rồi!");
            return;
        }

        if (!CanMove(diceValue))
        {
            Debug.Log("Không thể di chuyển do có vật cản.");
            return;
        }

       
        if (diceManager.LastDiceValue != 6 && playerTurnState.HasMoved) return;

        
        HidePossiblePath();

        if (State == PieceState.InBase && diceValue == 6)
        {
            SpawnFromBase();
        }
        else if (State == PieceState.OnPath)
        {
            MoveForward(diceValue);
        }

        
        playerTurnState.RPC_SetMoved(true);
        bool hasMoved = true;

       
        var allPieces = FindObjectsByType<Piece>(FindObjectsSortMode.None);
        foreach (var piece in allPieces)
        {
            piece.SetHighlight(false);
        }

        if (diceValue != 6 && hasMoved == true)
        {
            Debug.Log("[P2] Đã di chuyển và không roll 6 → Gửi yêu cầu chuyển lượt");

            if (turnManager.HasStateAuthority)
            {
                turnManager.NextTurnWhenDone(); 
            }
            else
            {
                turnManager.RPC_RequestEndTurnFromClient(); 
            }
        }
    }


    private void SpawnFromBase()
    {
        int index = StartIndex;
        Transform tile = commonPath[index];
        Vector3 targetPos = GetTileCenterPosition(tile);

        var piecesAtSpawn = gameManager.GetPiecesAtTile(index, this);
        foreach (var p in piecesAtSpawn)
        {
            if (p.Color != this.Color)
            {
                if (p.HasShield)
                {
                    p.HasShield = false;
                    p.RPC_RequestSetShieldBlockKickHighlight(false);
                    CurrentTileIndex = -1;
                    State = PieceState.InBase;
                    return; 
                }

                p.RPC_ReturnToBase();

                var opponentWithBomb = CheckOpponentHasBomb(CurrentTileIndex);
                
                if (opponentWithBomb != null)
                {
                    HandleBombEffect(this, opponentWithBomb);
                }
            }
        }

        soundManager.PlayMarchSound(); 
        CurrentTileIndex = 0;
        State = PieceState.OnPath;

        StopAllCoroutines();
        StartCoroutine(JumpToSpawnPosition(targetPos, true)); 

        
        int nextIndex = (StartIndex + 1) % commonPath.Count;
        Vector3 nextTilePos = GetTileCenterPosition(commonPath[nextIndex], 0f); 
        Vector3 dir = (nextTilePos - targetPos).normalized;
        Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = Quaternion.Euler(0f, lookRot.eulerAngles.y, 0f); 
    }

    private IEnumerator JumpToSpawnPosition(Vector3 targetPos, bool endStanding)
    {
        rb.isKinematic = true; 

        Vector3 start = transform.position;
        float duration = 0.5f;
        float height = 20f;
        float elapsed = 0f;

        Vector3 dir = (targetPos - start).normalized;
        Quaternion targetYawRot = Quaternion.LookRotation(dir, Vector3.up);

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            Vector3 horizontal = Vector3.Lerp(start, targetPos, t);
            float verticalOffset = 4 * height * t * (1 - t);
            float pitchAngle = Mathf.Sin(t * Mathf.PI) * 20f * -1;
            Quaternion fullRot = Quaternion.Euler(pitchAngle, targetYawRot.eulerAngles.y, 0f);

            transform.position = new Vector3(horizontal.x, horizontal.y + verticalOffset, horizontal.z);
            transform.rotation = fullRot;

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;

        
        int nextIndex = (StartIndex + 1) % commonPath.Count;
        Vector3 nextTilePos = GetTileCenterPosition(commonPath[nextIndex], 0f);
        Vector3 nextDir = (nextTilePos - targetPos).normalized;
        Quaternion lookRot = Quaternion.LookRotation(nextDir, Vector3.up);

        if (endStanding)
            transform.rotation = Quaternion.Euler(0f, lookRot.eulerAngles.y, 0f); 
        else
            transform.rotation = Quaternion.Euler(-20f, lookRot.eulerAngles.y, 0f); 

        

        yield return null;

        rb.isKinematic = false; 
    }

    public void MoveForward(int steps)
    {
        StopAllCoroutines(); 
        StartCoroutine(MoveStepByStep(steps));
    }

    private IEnumerator MoveStepByStep(int steps)
    {
        rb.isKinematic = true; 

        for (int i = 0; i < steps; i++)
        {
            bool isLastStep = (i == steps - 1);
            CurrentTileIndex++;

            soundManager.PlayStepSound(); 

            int maxMainPath = commonPath.Count;

            if (CurrentTileIndex >= maxMainPath)
            {
               
                int goalStep = CurrentTileIndex - maxMainPath;

                if (goalStep >= GoalPath.Count)
                {
                    Debug.Log($"[Lỗi] GoalStep vượt quá giới hạn: {goalStep}");
                    yield break;
                }

                Vector3 goalPos = GetTileCenterPosition(GoalPath[goalStep]);

                if (isLastStep)
                {
                    KickOpponentIfAny(goalStep); 
                }

                yield return JumpToPosition(goalPos, 0.5f, 2f, isLastStep);

                if (goalStep == GoalPath.Count - 1)
                {
                    soundManager.PlayFinishSound(); 

                    RPC_ReturnToBaseWhenFinish();

                    yield return new WaitForSeconds(0.5f);

                    RPC_SetFinishedHighlight(true); 

                    if (Object.HasStateAuthority && rankingManager != null)
                    {
                        rankingManager.RequestPieceFinish(this);
                    }
                }
            }
            else
            {
                
                int realIndex = (StartIndex + CurrentTileIndex) % commonPath.Count;

                if (isLastStep)
                {
                    
                    var opponentWithBomb = CheckOpponentHasBomb(CurrentTileIndex);
                   
                    if (opponentWithBomb != null)
                    {
                        HandleBombEffect(this,opponentWithBomb);

                        if (opponentWithBomb.State != PieceState.InBase && State != PieceState.InBase)
                        {
                            CurrentTileIndex--; 
                            realIndex = (StartIndex + CurrentTileIndex) % commonPath.Count;
                            Vector3 targetPosition = GetTileCenterPosition(commonPath[realIndex]);
                            yield return JumpToPosition(targetPosition, 0.5f, 2f, true);
                        }
                        else if (State != PieceState.InBase) 
                        {
                            
                            Vector3 targetPosition = GetTileCenterPosition(commonPath[realIndex]);
                            yield return JumpToPosition(targetPosition, 0.5f, 2f, true);
                        }

                        yield break;
                    }

                    var opponentWithShield = CheckOpponentHasShield(CurrentTileIndex);
                    if (opponentWithShield != null)
                    {
                        
                        KickOpponentIfAny(CurrentTileIndex);

                       
                        CurrentTileIndex--;
                        int newIndex = (StartIndex + CurrentTileIndex) % commonPath.Count;
                        Vector3 standPos = GetTileCenterPosition(commonPath[newIndex]);
                        yield return JumpToPosition(standPos, 0.5f, 2f, true);
                        transform.rotation = Quaternion.Euler(0, 0, 0);

                        
                        yield break;
                    }


                    
                    KickOpponentIfAny(CurrentTileIndex);
                }
                
                Vector3 targetPos = GetTileCenterPosition(commonPath[realIndex]);
                yield return JumpToPosition(targetPos, 0.5f, 2f, isLastStep);
            }
        }

        rb.isKinematic = false; 
    }

    private Piece CheckOpponentHasShield(int tileIndex)
    {
        int realIndex = (StartIndex + tileIndex) % commonPath.Count;
        var piecesOnTile = gameManager.GetPiecesAtTile(realIndex, this);
        foreach (var piece in piecesOnTile)
        {
            if (piece.Color != this.Color && piece.HasShield)
            {
                return piece; 
            } 
        } 
        return null; 
    }
    private Piece CheckOpponentHasBomb(int tileIndex)
    {
        int realIndex = (StartIndex + tileIndex) % commonPath.Count;
        var piecesOnTile = gameManager.GetPiecesAtTile(realIndex, this);
        foreach (var piece in piecesOnTile)
        {
            if (piece.Color != this.Color && piece.HasBomb)
            {
                return piece; 
            }
        }
        return null;
    }


    private IEnumerator JumpToPosition(Vector3 target, float duration, float height, bool endStanding)
    {
        rb.isKinematic = true; 

       
        Vector3 dir = (target - transform.position).normalized;
        Quaternion targetYawRot = Quaternion.LookRotation(dir, Vector3.up);

        Vector3 start = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

           
            Vector3 horizontal = Vector3.Lerp(start, target, t);

            float verticalOffset = 4 * height * t * (1 - t);

            
            float pitchAngle = Mathf.Sin(t * Mathf.PI) * 20f;

          
            pitchAngle *= -1;

            Quaternion fullRot = Quaternion.Euler(pitchAngle, targetYawRot.eulerAngles.y, 0f);

            transform.position = new Vector3(horizontal.x, horizontal.y + verticalOffset, horizontal.z);
            transform.rotation = fullRot;

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = target;

        transform.rotation = endStanding
        ? Quaternion.Euler(0f, targetYawRot.eulerAngles.y, 0f) 
        : Quaternion.Euler(-20f, targetYawRot.eulerAngles.y, 0f); 

        rb.isKinematic = false; 
    }


    private Vector3 GetTileCenterPosition(Transform tile, float offsetY = 3f)
    {
        Collider col = tile.GetComponent<Collider>();
        Vector3 center = col != null ? col.bounds.center : tile.position;
        return center + Vector3.up * offsetY;
    }

    public bool CanMove(int steps)
    {
        var path = commonPath;
        int maxMainPath = path.Count;
        int doorIndex = maxMainPath - 1;

       
        if (State == PieceState.Finished)
        {
            Debug.Log($"[Chặn] Quân {Color} đã về đích, không thể di chuyển.");
            return false;
        }

      
        if (State == PieceState.InBase)
        {
            if (steps != 6)
                return false;

            int spawnIndex = StartIndex;
            var piecesOnSpawn = gameManager.GetPiecesAtTile(spawnIndex, this);
            Debug.Log($"Kiểm tra xuất quân {Color} tại ô spawn {spawnIndex}. Quân tại ô: {string.Join(", ", piecesOnSpawn.Select(p => p.Color.ToString()))}");
            return !piecesOnSpawn.Any(p => p.Color == this.Color);  
        }

        
        int targetIndex = CurrentTileIndex + steps;

       
        if (CurrentTileIndex < maxMainPath)
        {
            
            if (targetIndex > doorIndex && CurrentTileIndex < doorIndex)
            {
                int stepsToDoor = doorIndex - CurrentTileIndex;
                if (steps != stepsToDoor)
                {
                    Debug.Log($"Chặn di chuyển: Phải roll chính xác {stepsToDoor} để đến ô cửa chuồng {doorIndex} từ ô {CurrentTileIndex}");
                    return false;
                }
            }

          
            for (int i = 1; i <= steps; i++)
            {
                int checkIndex = CurrentTileIndex + i;
                if (checkIndex >= maxMainPath)
                {
                    int goalStep = checkIndex - maxMainPath;
                    if (goalStep >= GoalPath.Count)
                    {
                        Debug.Log($"Di chuyển vượt quá goal path tại index {goalStep}");
                        return false;
                    }
                    var piecesOnGoal = gameManager.GetPiecesAtTile(goalStep, this, true);
                    if (i < steps && piecesOnGoal.Any())
                    {
                        Debug.Log($"Chặn bởi quân tại goal index {goalStep}: {string.Join(", ", piecesOnGoal.Select(p => p.Color.ToString()))}");
                        return false;
                    }
                    if (i == steps && piecesOnGoal.Any(p => p.Color == this.Color))
                    {
                        Debug.Log($"Chặn bởi quân đồng minh tại goal index {goalStep}: {string.Join(", ", piecesOnGoal.Select(p => p.Color.ToString()))}");
                        return false;
                    }
                }
                else
                {
                    int realIndex = (StartIndex + checkIndex) % maxMainPath;
                    var piecesOnTile = gameManager.GetPiecesAtTile(realIndex, this);
                    if (i < steps && piecesOnTile.Any())
                    {
                        Debug.Log($"Chặn bởi quân tại ô {realIndex}: {string.Join(", ", piecesOnTile.Select(p => p.Color.ToString()))}");
                        return false;
                    }
                    if (i == steps && piecesOnTile.Any(p => p.Color == this.Color))
                    {
                        Debug.Log($"Chặn bởi quân đồng minh tại ô {realIndex}: {string.Join(", ", piecesOnTile.Select(p => p.Color.ToString()))}");
                        return false;
                    }
                }
            }
        }
    
        else
        {
            int currentGoalStep = CurrentTileIndex - maxMainPath;
            int targetGoalStep = currentGoalStep + steps;

            if (targetGoalStep >= GoalPath.Count)
            {
                Debug.Log($"[Chặn] Di chuyển vượt quá GoalPath: {targetGoalStep} >= {GoalPath.Count}");
                return false;
            }

            for (int i = 1; i <= steps; i++)
            {
                int checkStep = currentGoalStep + i;
                if (checkStep >= GoalPath.Count) return false;

                var piecesOnTile = gameManager.GetPiecesAtTile(checkStep, this, true);
                if (i < steps && piecesOnTile.Any())
                {
                    Debug.Log($"[Chặn] Trên đường tới GoalPath[{checkStep}] có quân cản");
                    return false;
                }
                if (i == steps && piecesOnTile.Any(p => p.Color == this.Color))
                {
                    Debug.Log($"[Chặn] Ô đích GoalPath[{checkStep}] có quân đồng minh");
                    return false;
                }
            }

            return true;
        }

        return true;
    }

    private void KickOpponentIfAny(int tileIndex)
    {
        if (CurrentTileIndex >= commonPath.Count)
        {
            Debug.Log("[INFO] Không đá quân trong GoalPath vì đó là vùng an toàn.");
            return;
        }
        else
        {
            int realIndex = (StartIndex + tileIndex) % commonPath.Count;
            var piecesOnTile = gameManager.GetPiecesAtTile(realIndex, this);
            foreach (var piece in piecesOnTile)
            {
                if (piece.Color != this.Color)
                {
                    Debug.Log($"[Đá] Quân {piece.Color} bị {this.Color} đá tại ô {realIndex}!");
                    soundManager.PlayKickSound(); 
                    piece.RPC_ReturnToBase();
                }
            }
        }
    }


    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ReturnToBase()
    {
        ReturnToBase(); 
    }

    
    public void ReturnToBase()
    {
        if (this.HasShield)
        {
            this.HasShield = false;
            this.RPC_RequestSetShieldBlockKickHighlight(false);
            Debug.Log("Khiên đã chặn cú đá!");
            return; 
        }
        else
        {
            State = PieceState.InBase;
            CurrentTileIndex = -1;
            StartCoroutine(JumpToPosition(initialBasePosition,1,5, true)); 
            Debug.Log($"{name} đã bị đá về base tại {initialBasePosition}");
        } 
    }

    public int GetRealTileIndex()
    {
        if (State != PieceState.OnPath) return -1;

        int realIndex = (StartIndex + CurrentTileIndex) % commonPath.Count;
        return realIndex;
    }

    public void SetHighlight(bool active)
    {
        if (highlightObject != null)
            highlightObject.SetActive(active);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetHighlight(bool active)
    {
        SetHighlight(active);
    }

    public void SetFinishedHighlight(bool active)
    {
        if (highlightObject != null)
            finishedObject.SetActive(active);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetFinishedHighlight(bool active)
    {
        SetFinishedHighlight(active);
    }


    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ReturnToBaseWhenFinish()
    {
        ReturnToBaseWhenFinish(); 
    }

 
    public void ReturnToBaseWhenFinish()
    {
        State = PieceState.Finished;
        CurrentTileIndex = -1;

        RPC_RequestSetShieldBlockKickHighlight(false); 
        RPC_RequestTurnOfBomb(); 

        StartCoroutine(JumpToPosition(initialBasePosition, 1, 5, true)); 

       
        Debug.Log($"{name} đã hoàn thành về đích và quay về vị trí chờ tại {initialBasePosition}");
    }

   
    public void ShowPossiblePath()
    {
        List<Transform> tilesToHighlight = new List<Transform>();
        int diceValue = diceManager.DiceResult;

        if (State == PieceState.InBase)
        {
            if (diceValue == 6)
            {
                Transform spawnTile = commonPath[StartIndex];
                if (spawnTile != null) tilesToHighlight.Add(spawnTile);
            }
        }
        else if (State == PieceState.OnPath)
        {
            int maxMainPath = commonPath.Count;

          
            int physicalStartIndex;
            if (CurrentTileIndex >= maxMainPath)
            {
                
                physicalStartIndex = CurrentTileIndex;
            }
            else
            {
                
                physicalStartIndex = (StartIndex + CurrentTileIndex) % maxMainPath;
            }

            tilesToHighlight = GetPathTiles(physicalStartIndex, diceValue);
        }

        if (CanMove(diceValue) && tilesToHighlight.Count > 0)
        {
            GameBoard.Instance.HighlightTiles(tilesToHighlight, true);
            soundManager.PlayHoverPieceSound();
            if (highlightObject2 != null) highlightObject2.SetActive(true);
            pieceModel.GetComponent<Renderer>().material = highlightMaterial; 
        }
    }

    public void HidePossiblePath()
    {
        List<Transform> tilesToHighlight = new List<Transform>();
        int diceValue = diceManager.DiceResult;

        if (State == PieceState.InBase)
        {
            if (diceValue == 6)
            {
                Transform spawnTile = commonPath[StartIndex];
                if (spawnTile != null) tilesToHighlight.Add(spawnTile);
            }
        }
        else if (State == PieceState.OnPath)
        {
            int maxMainPath = commonPath.Count;

          
            int physicalStartIndex;
            if (CurrentTileIndex >= maxMainPath)
            {
                physicalStartIndex = CurrentTileIndex;
            }
            else
            {
                physicalStartIndex = (StartIndex + CurrentTileIndex) % maxMainPath;
            }

            tilesToHighlight = GetPathTiles(physicalStartIndex, diceValue);
        }

        GameBoard.Instance.HighlightTiles(tilesToHighlight, false);
        if (highlightObject2 != null) highlightObject2.SetActive(false);
        pieceModel.GetComponent<Renderer>().material = originalMaterial; 
    }

    private List<Transform> GetPathTiles(int physicalStartIndex, int steps)
    {
        var path = GameBoard.Instance.commonPath;
        int maxMainPath = path.Count;
        var tiles = new List<Transform>();
        if (steps <= 0) return tiles;

       
        if (physicalStartIndex >= maxMainPath)
        {
            int currentGoalStep = physicalStartIndex - maxMainPath; 
            for (int i = 1; i <= steps && (currentGoalStep + i) < GoalPath.Count; i++)
            {
                tiles.Add(GoalPath[currentGoalStep + i]);
            }
            return tiles;
        }

        
        int doorPhysical = (StartIndex + maxMainPath - 1) % maxMainPath; 
        int distToDoor = (doorPhysical - physicalStartIndex + maxMainPath) % maxMainPath;

       
        if (physicalStartIndex == doorPhysical)
        {
            for (int i = 0; i < steps && i < GoalPath.Count; i++)
                tiles.Add(GoalPath[i]);
            return tiles;
        }

       
        if (steps <= distToDoor)
        {
            for (int i = 1; i <= steps; i++)
                tiles.Add(path[(physicalStartIndex + i) % maxMainPath]);
        }
        else
        {
            
        }

        return tiles;
    }

  

    private IEnumerator CheckTileEffectCoroutine()
    {
        yield return new WaitForSeconds(0.5f); 

        Transform currentTile = GetCurrentTileTransform();
        if (currentTile == null) yield break;

        TileEffectManager tileEffect = currentTile.GetComponent<TileEffectManager>();
        if (tileEffect == null) yield break;

        Effect_ExecuteEffect(tileEffect.effectType);
    }

    private Transform GetCurrentTileTransform()
    {
        int maxMainPath = commonPath.Count;
        if (CurrentTileIndex < maxMainPath)
        {
            int realIndex = (StartIndex + CurrentTileIndex) % maxMainPath;
            return commonPath[realIndex];
        }
        else
        {
            int goalStep = CurrentTileIndex - maxMainPath;
            if (goalStep >= 0 && goalStep < GoalPath.Count)
            {
                return GoalPath[goalStep];
            }
        }
        return null;
    }

    private void Effect_ExecuteEffect(EffectType effectType)
    {
        if (!Object.HasStateAuthority) return;

        switch (effectType)
        {
            case EffectType.MoveForward1: MoveForward(1); break;
            case EffectType.MoveForward2: MoveForward(2); break;
            case EffectType.MoveForward3: MoveForward(3); break;
            case EffectType.MoveForward4: MoveForward(4); break;
            case EffectType.MoveForward5: MoveForward(5); break;

            case EffectType.MoveBackward1: Effect_MoveBackward(1); break;
            case EffectType.MoveBackward2: Effect_MoveBackward(2); break;
            case EffectType.MoveBackward3: Effect_MoveBackward(3); break;
            case EffectType.MoveBackward4: Effect_MoveBackward(4); break;
            case EffectType.MoveBackward5: Effect_MoveBackward(5); break;

            case EffectType.SendToBase: ReturnToBase(); break;
            case EffectType.KickRandomOpponent: Effect_KickRandomOpponent(); break;
            case EffectType.AutoSpawn1Piece: Effect_AutoSpawn1Piece(); break;
            case EffectType.ReturnToSpawnPoint: Effect_ReturnToSpawnPoint(); break;
            case EffectType.HasShieldBlockKick: Effet_HasShieldBlockKick(); break;

            case EffectType.StartRotatePiece: Effect_StartRotatePiece(); break;
            case EffectType.SwapRandomOpponent: Effect_SwapRandomOpponent(); break;
            case EffectType.SpawnAndSwapRandomOpponentToBase: Effect_SpawnAndSwapRandomOpponentToBase(); break;
            case EffectType.Effect_HasBomb: Effect_HasBomb(); break;
            case EffectType.FlyToGoalDoor: Effect_FlyToGoalDoor(); break;

            case EffectType.RandomOpponentMoveFoward1: Effect_RandomOpponentMoveFoward(1); break;
            case EffectType.RandomOpponentMoveFoward2: Effect_RandomOpponentMoveFoward(2); break;
            case EffectType.RandomOpponentMoveFoward3: Effect_RandomOpponentMoveFoward(3); break;
            case EffectType.RandomOpponentMoveFoward4: Effect_RandomOpponentMoveFoward(4); break;
            case EffectType.RandomOpponentMoveFoward5: Effect_RandomOpponentMoveFoward(5); break;

            case EffectType.RandomOpponentMoveBackward1: Effect_RandomOpponentMoveBackward(1); break;
            case EffectType.RandomOpponentMoveBackward2: Effect_RandomOpponentMoveBackward(2); break;
            case EffectType.RandomOpponentMoveBackward3: Effect_RandomOpponentMoveBackward(3); break;
            case EffectType.RandomOpponentMoveBackward4: Effect_RandomOpponentMoveBackward(4); break;
            case EffectType.RandomOpponentMoveBackward5: Effect_RandomOpponentMoveBackward(5); break;
        }
    }
    public void Effect_MoveBackward(int steps)
    {
        if (CanMoveWhenMoveBackward(steps))
        {
            StopAllCoroutines();

           
            if (CurrentTileIndex > commonPath.Count)
            {
                return;
            }

            int targetIndex = CurrentTileIndex - steps;
            if (targetIndex < 0)
            {
                ReturnToBase();
                return;
            }

            StartCoroutine(Effect_MoveBackwardStepByStep(steps));
        }
    }

    private IEnumerator Effect_MoveBackwardStepByStep(int steps)
    {
        for (int i = 0; i < steps; i++)
        {
            CurrentTileIndex--;
            soundManager.PlayStepSound();

            int realIndex = (StartIndex + CurrentTileIndex) % commonPath.Count;
            Vector3 targetPos = GetTileCenterPosition(commonPath[realIndex]);
            yield return JumpToPosition(targetPos, 0.5f, 2f, i == steps - 1);
        }

        yield return CheckTileEffectCoroutine();
    }

    public void Effect_KickRandomOpponent()
    {
        if (!Object.HasStateAuthority) return;

        Piece[] allPieces = FindObjectsByType<Piece>(FindObjectsSortMode.None);
        List<Piece> opponents = allPieces.Where(p => p.Color != this.Color && p.State == PieceState.OnPath).ToList();

        if (opponents.Count == 0) return;

        Piece target = opponents[Random.Range(0, opponents.Count)];

        if (target.CurrentTileIndex > commonPath.Count)
        {
            return;
        }

        soundManager.PlayKickSound(); 
        target.RPC_ReturnToBase();
    }

    public void Effect_AutoSpawn1Piece()
    {
        if (!Object.HasStateAuthority) return;

        int spawnIndex = StartIndex;

        
        List<Piece> piecesAtSpawn = gameManager.GetPiecesAtTile(spawnIndex, this);
        bool hasAllyAtSpawn = piecesAtSpawn.Any(p => p.Color == this.Color);

        if (hasAllyAtSpawn)
        {
            
            return;
        }

        
        foreach (var enemy in piecesAtSpawn)
        {
            if (enemy.Color != this.Color)
            {
                enemy.RPC_ReturnToBase();
            }
        }

        
        Piece[] allPieces = FindObjectsByType<Piece>(FindObjectsSortMode.None);
        Piece pieceInBase = allPieces.FirstOrDefault(p => p.State == PieceState.InBase && p.Color == this.Color);

        if (pieceInBase != null)
        {
            pieceInBase.SpawnFromBase();
        }
    }

    public void Effect_ReturnToSpawnPoint()
    {
        if (!Object.HasStateAuthority) return;

        int spawnIndex = StartIndex;

        
        List<Piece> piecesAtSpawn = gameManager.GetPiecesAtTile(spawnIndex, this);
        bool hasAllyAtSpawn = piecesAtSpawn.Any(p => p.Color == this.Color);

        if (hasAllyAtSpawn)
        {
            
            return;
        }

       
        foreach (var enemy in piecesAtSpawn)
        {
            if (enemy.Color != this.Color)
            {
                enemy.RPC_ReturnToBase();
            }
        }

        
        if (this.State != PieceState.OnPath) return;

       
        this.CurrentTileIndex = 0;
        this.State = PieceState.OnPath; 

        int index = StartIndex;
        Transform tile = commonPath[index];
        Vector3 targetPos = GetTileCenterPosition(tile);

        
        this.StopAllCoroutines();
        this.StartCoroutine(JumpToPosition(targetPos, 0.5f, 4f, true));

      
        int nextIndex = (StartIndex + 1) % commonPath.Count;
        Vector3 nextTilePos = GetTileCenterPosition(commonPath[nextIndex], 0f); 
        Vector3 dir = (nextTilePos - targetPos).normalized;
        Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = Quaternion.Euler(0f, lookRot.eulerAngles.y, 0f);
    }

    public void Effet_HasShieldBlockKick()
    {
        if (!Object.HasStateAuthority) return;

        if (!HasShield)
        {
            HasShield = true;
            RPC_RequestSetShieldBlockKickHighlight(true); 
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestSetShieldBlockKickHighlight(bool active)
    {
        RPC_ReplySetShieldBlockKickHighlight(active);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ReplySetShieldBlockKickHighlight(bool active)
    {
        if (ShieldObject != null)
            ShieldObject.SetActive(active);
        HasShield = active;
    }

    public void Effect_StartRotatePiece()
    {
        if (!Object.HasStateAuthority || isSpinning) return;
        StartCoroutine(RotatePieceCoroutine(60));
    }

    private IEnumerator RotatePieceCoroutine(float duration)
    {
        isSpinning = true;

        float elapsed = 0f;
        float rotationSpeed = 360f; 

        Quaternion initialRotation = transform.rotation;

        while (elapsed < duration)
        {
            float angle = rotationSpeed * Time.deltaTime;
            transform.rotation *= Quaternion.Euler(0f, angle, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        isSpinning = false;
    }

    public void Effect_SwapRandomOpponent()
    {
        if (!Object.HasStateAuthority) return;

        Piece[] allPieces = FindObjectsByType<Piece>(FindObjectsSortMode.None);
        List<Piece> opponents = allPieces
            .Where(p => p.Color != this.Color && p.State == PieceState.OnPath)
            .ToList();

        if (opponents.Count == 0) return;

        Piece target = opponents[Random.Range(0, opponents.Count)];

        if (target.CurrentTileIndex > commonPath.Count)
        {
            return;
        }

        
        int myPhysicalIndex = ConvertLogicalToPhysical(CurrentTileIndex, this.Color);
        int targetPhysicalIndex = ConvertLogicalToPhysical(target.CurrentTileIndex, target.Color);

      
        Vector3 myNewPos = GetTileCenterPosition(commonPath[targetPhysicalIndex]);
        Vector3 targetNewPos = GetTileCenterPosition(commonPath[myPhysicalIndex]);

        
        CurrentTileIndex = ConvertPhysicalToLogical(targetPhysicalIndex, this.Color);
        StopAllCoroutines();
        StartCoroutine(JumpToTilePosition(myNewPos, 7));

      
        target.RPC_SwapToNewTile(
            ConvertPhysicalToLogical(myPhysicalIndex, target.Color),
            targetNewPos
        );
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_SwapToNewTile(int newTileIndex, Vector3 newPos)
    {
        
        CurrentTileIndex = newTileIndex;

       
        StopAllCoroutines();
        StartCoroutine(JumpToTilePosition(newPos, 4));
    }

    public int ConvertLogicalToPhysical(int logicalIndex, PlayerColor color)
    {
        int startIndex = GetStartIndex(color);
        return (startIndex + logicalIndex) % commonPath.Count;
    }

    public int ConvertPhysicalToLogical(int physicalIndex, PlayerColor color)
    {
        int startIndex = GetStartIndex(color);
        return (physicalIndex - startIndex + commonPath.Count) % commonPath.Count;
    }

    private int GetStartIndex(PlayerColor color)
    {
        switch (color)
        {
            case PlayerColor.Red: return GameBoard.Instance.redStartIndex;
            case PlayerColor.Blue: return GameBoard.Instance.blueStartIndex;
            case PlayerColor.Green: return GameBoard.Instance.greenStartIndex;
            case PlayerColor.Yellow: return GameBoard.Instance.yellowStartIndex;
        }
        return 0;
    }

    private IEnumerator JumpToTilePosition(Vector3 targetPos, float height)
    {
        rb.isKinematic = true;

        Vector3 start = transform.position;
        float duration = 1f;
        float elapsed = 0f;

        
        Vector3 dir = (targetPos - start).normalized;
        Quaternion targetYawRot = Quaternion.LookRotation(dir, Vector3.up);

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            Vector3 horizontal = Vector3.Lerp(start, targetPos, t);
            float verticalOffset = 4 * height * t * (1 - t);
            float pitchAngle = Mathf.Sin(t * Mathf.PI) * 20f * -1;
            Quaternion fullRot = Quaternion.Euler(pitchAngle, targetYawRot.eulerAngles.y, 0f);

            transform.position = new Vector3(horizontal.x, horizontal.y + verticalOffset, horizontal.z);
            transform.rotation = fullRot;

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        rb.isKinematic = false; 
    }

    public void Effect_SpawnAndSwapRandomOpponentToBase()
    {
        if (!Object.HasStateAuthority) return;

        Piece[] allPieces = FindObjectsByType<Piece>(FindObjectsSortMode.None);
        List<Piece> opponents = allPieces
            .Where(p => p.Color != this.Color && p.State == PieceState.OnPath)
            .ToList();

        if (opponents.Count == 0)
        {
            Effect_AutoSpawn1Piece();
            return;
        }
            
        Piece target = opponents[Random.Range(0, opponents.Count)];
        Piece myNewPiece = allPieces.FirstOrDefault(p => p.State == PieceState.InBase && p.Color == this.Color);
        if (myNewPiece == null) return;

        if (target.CurrentTileIndex > commonPath.Count)
        {
            return;
        }
        if (target.HasShield)
        {
            target.HasShield = false;
           
            return;
        }

       
        int targetPhysicalIndex = ConvertLogicalToPhysical(target.CurrentTileIndex, target.Color);

       
        Vector3 myNewPos = GetTileCenterPosition(commonPath[targetPhysicalIndex]);

        
        myNewPiece.CurrentTileIndex = ConvertPhysicalToLogical(targetPhysicalIndex, this.Color);

        StartCoroutine(myNewPiece.JumpToTilePosition(myNewPos, 4));
        myNewPiece.State = PieceState.OnPath; 

        soundManager.PlayKickSound(); 
        
        target.RPC_ReturnToBase();
    }

   
    public void Effect_HasBomb()
    {
        if (!Object.HasStateAuthority) return;

        if (!HasBomb)
        {
            HasBomb = true;
            RPC_SetBombHighlight(true);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestTurnOfBomb()
    {
        RPC_ReplyTurnOfBomb(); 
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ReplyTurnOfBomb()
    {
        if (!Object.HasStateAuthority) return;
        HasBomb = false;
        RPC_SetBombHighlight(false); 
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetBombHighlight(bool active)
    {
        SetBombHighlight(active);
    }

    public void SetBombHighlight(bool active)
    {
        if (BombObject != null)
            BombObject.SetActive(active);
    }

    public void HandleBombEffect(Piece attacker, Piece defender)
    {
        

        if (!defender.HasBomb) return; 

        Debug.Log($"[Bomb] {defender.Color} có bomb, xử lý nổ!");

        
        if (!attacker.HasShield && !defender.HasShield)
        {
            attacker.RPC_ReturnToBase();
            defender.RPC_ReturnToBase();
        }
        
        else if (!attacker.HasShield && defender.HasShield)
        {
            attacker.RPC_ReturnToBase();
            defender.HasShield = false;
            defender.RPC_RequestSetShieldBlockKickHighlight(false);
        }
        
        else if (attacker.HasShield && !defender.HasShield)
        {
            defender.RPC_ReturnToBase();
            attacker.HasShield = false;
            attacker.RPC_RequestSetShieldBlockKickHighlight(false);

        }
        
        else if (attacker.HasShield && defender.HasShield)
        {
            attacker.HasShield = false;
            attacker.RPC_RequestSetShieldBlockKickHighlight(false);

            defender.HasShield = false;
            defender.RPC_RequestSetShieldBlockKickHighlight(false);

        }

       
        defender.RPC_RequestTurnOfBomb();
    }

    public void Effect_RandomOpponentMoveFoward(int steps)
    {
        if (!Object.HasStateAuthority) return;

       
        Piece[] allPieces = FindObjectsByType<Piece>(FindObjectsSortMode.None);
        List<Piece> opponents = allPieces
            .Where(p => p.State == PieceState.OnPath)
            .ToList();

        if (opponents.Count == 0) return;

      
        Piece target = opponents[Random.Range(0, opponents.Count)];

        
        target.RPC_MoveToFowardTile(steps);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_MoveToFowardTile(int steps)
    {
        if (CanMove(steps))
        {
            MoveForward(steps);
        }   
    }

    public void Effect_RandomOpponentMoveBackward(int steps)
    {
        if (!Object.HasStateAuthority) return;

        
        Piece[] allPieces = FindObjectsByType<Piece>(FindObjectsSortMode.None);
        List<Piece> opponents = allPieces
            .Where(p => p.State == PieceState.OnPath)
            .ToList();

        if (opponents.Count == 0) return;

       
        Piece target = opponents[Random.Range(0, opponents.Count)];

      
        target.RPC_MoveToBackTile(steps);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_MoveToBackTile(int steps)
    {
        if (CanMoveWhenMoveBackward(steps))
        {
            Effect_MoveBackward(steps);
        } 
    }

    public void Effect_MoveForward(int steps)
    {
        if (CanMove(steps))
        {
            StopAllCoroutines(); 
            StartCoroutine(MoveStepByStep(steps));
        }
    }

    public bool CanMoveWhenMoveBackward(int steps)
    {
        var path = commonPath;
        int maxMainPath = path.Count;
        int doorIndex = maxMainPath - 1;

       
        if (CurrentTileIndex < maxMainPath)
        {
            int targetIndex = CurrentTileIndex - steps;

          
            if (targetIndex < 0)
            {
                return false;
            }

            
            for (int i = 1; i <= steps; i++)
            {
                int checkIndex = CurrentTileIndex - i;
                if (checkIndex < 0)
                {
                    return false; 
                }

                int realIndex = (StartIndex + checkIndex) % maxMainPath;
                var piecesOnTile = gameManager.GetPiecesAtTile(realIndex, this, false);

               
                if (i < steps && piecesOnTile.Any())
                {
                    return false;
                }

                
                if (i == steps && piecesOnTile.Any())
                {
                    return false;
                }
            }

            return true;
        }
        
        else
        {
            int currentGoalStep = CurrentTileIndex - maxMainPath;
            int targetGoalStep = currentGoalStep - steps;

            
            if (targetGoalStep < 0)
            {
                return false;
            }

           
            for (int i = 1; i <= steps; i++)
            {
                int checkStep = currentGoalStep - i;
                if (checkStep < 0)
                {
                    return false; 
                }

                var piecesOnTile = gameManager.GetPiecesAtTile(checkStep, this, true);

                
                if (i < steps && piecesOnTile.Any())
                {
                    return false;
                }

               
                if (i == steps && piecesOnTile.Any(p => p.Color == this.Color))
                {
                    return false;
                }
            }

            return true;
        }
    }

    public void Effect_FlyToGoalDoor()
    {
        if (!Object.HasStateAuthority) return;

        if (State != PieceState.OnPath) return;

        int maxMainPath = commonPath.Count;
        int doorIndex = maxMainPath - 1; 

        
        CurrentTileIndex = doorIndex;

        
        int realIndex = (StartIndex + doorIndex) % maxMainPath;
        Vector3 targetPos = GetTileCenterPosition(commonPath[realIndex]);

        StopAllCoroutines();
        StartCoroutine(JumpToPosition(targetPos,0.5f,2f,true));
    }
}

public enum PieceState
{
    InBase,      
    OnPath,      
    Finished     
}
public enum PlayerColor
{
    Red,
    Blue,
    Green,
    Yellow
}
