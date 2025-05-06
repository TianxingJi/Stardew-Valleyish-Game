using System.Collections;
using System.Collections.Generic;
using TFarm.AStar;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using TFarm.Save;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class NPCMovement : MonoBehaviour, ISaveable
{
    public ScheduleDataList_SO scheduleData;
    private SortedSet<ScheduleDetails> scheduleSet;
    private ScheduleDetails currentSchedule;
    //Store information temporarily
    [SerializeField] private string currentScene;
    private string targetScene;
    private Vector3Int currentGridPosition;
    private Vector3Int targetGridPosition;
    private Vector3Int nextGridPosition;
    private Vector3 nextWorldPosition;

    public string StartScene { set => currentScene = value; }

    [Header("Movement Properties")]
    public float normalSpeed = 2f;
    private float minSpeed = 1;
    private float maxSpeed = 3;
    private Vector2 dir;
    public bool isMoving;
    public bool interactable;

    //Components
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D coll;
    private Animator anim;
    private Grid grid;

    private Stack<MovementStep> movementSteps;
    private Coroutine npcMoveRoutine;

    private bool isInitialised;
    private bool npcMove;
    private bool sceneLoaded;
    public bool isFirstLoad;
    private Season currentSeason;

    // Animation Counter
    private float animationBreakTime;
    private bool canPlayStopAnimation;
    private AnimationClip stopAnimationClip;
    public AnimationClip blankAnimationClip;
    private AnimatorOverrideController animOverride;

    private TimeSpan GameTime => TimeManager.Instance.GameTime;

    public string GUID => GetComponent<DataGUID>().guid;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        coll = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
        movementSteps = new Stack<MovementStep>();

        animOverride = new AnimatorOverrideController(anim.runtimeAnimatorController);
        anim.runtimeAnimatorController = animOverride;
        scheduleSet = new SortedSet<ScheduleDetails>();

        foreach (var schedule in scheduleData.scheduleLists)
        {
            scheduleSet.Add(schedule);
        }
    }

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;

        EventHandler.GameMinuteEvent += OnGameMinuteEvent;
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        EventHandler.EndGameEvent += OnEndGameEvent;

    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;

        EventHandler.GameMinuteEvent -= OnGameMinuteEvent;
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
        EventHandler.EndGameEvent -= OnEndGameEvent;

    }

    private void Start()
    {
        ISaveable saveable = this;
        saveable.RegisterSaveable();
    }

    private void Update()
    {
        if (sceneLoaded)
        {
            SwitchAnimation();
        }

        animationBreakTime -= Time.deltaTime;
        canPlayStopAnimation = animationBreakTime <= 0;
    }

    private void FixedUpdate()
    {
        if(sceneLoaded)
            Movement();
    }

    private void OnStartNewGameEvent(int obj)
    {
        isInitialised = false;
        isFirstLoad = true;
    }

    private void OnEndGameEvent()
    {
        sceneLoaded = false;
        npcMove = false;
        if(npcMoveRoutine != null)
        {
            StopCoroutine(npcMoveRoutine);
        }
    }

    private void OnGameMinuteEvent(int minute, int hour, int day, Season season)
    {
        int time = (hour * 100) + minute;
        currentSeason = season;

        ScheduleDetails matchSchedule = null;

        foreach(var schedule in scheduleSet)
        {
            if(schedule.Time == time)
            {
                if (schedule.day != day && schedule.day != 0)
                    continue;
                if(schedule.season != season)
                    continue;
             matchSchedule = schedule;
            }
            else if(schedule.Time > time)
            {
                break;
            }
        }

        if (matchSchedule != null)
            BuildPath(matchSchedule);
    }

    private void OnBeforeSceneUnloadEvent()
    {
        sceneLoaded = false;
    }

    private void OnAfterSceneLoadedEvent()
    {
        grid = FindObjectOfType<Grid>();
        CheckVisiable();

        if (!isInitialised)
        {
            InitNPC();
            isInitialised = true;
        }

        sceneLoaded = true;

        if (!isFirstLoad)
        {
            currentGridPosition = grid.WorldToCell(transform.position);
            var schedule = new ScheduleDetails(0, 0, 0, 0, currentSeason, targetScene, (Vector2Int)targetGridPosition, stopAnimationClip, interactable);
            BuildPath(schedule);
            isFirstLoad = true;
        }
    }

    private void CheckVisiable()
    {
        if (currentScene == SceneManager.GetActiveScene().name) // if the current scene of NPC
                                                                // equal to the active scene right now
            SetActiveInScene(); // we set the NPC is visible
        else
            SetInactiveInScene(); // if not, we set the NPC is invisible
    }

    private void InitNPC()
    {
        targetScene = currentScene;

        // Make sure the NPC is in the center of the grid
        currentGridPosition = grid.WorldToCell(transform.position);

        transform.position = new Vector3(currentGridPosition.x + Settings.gridCellSize / 2f, currentGridPosition.y + Settings.gridCellSize / 2f, 0);

        targetGridPosition = currentGridPosition;
    }


    private void Movement() // npc movement, which will be put in Update() method, which then does not need to be loop
    {
        if (!npcMove)
        {
            if (movementSteps.Count > 0) // if there are steps to move
            {
                MovementStep step = movementSteps.Pop(); // pop one step

                currentScene = step.sceneName; // record the step scene, where the step should be implemented

                CheckVisiable(); // if the current Scene is not in this scene, we close the npc to simulate a sense of chaning scene

                nextGridPosition = (Vector3Int)step.gridCoordinate; // record this gird position
                TimeSpan stepTime = new TimeSpan(step.hour, step.minute, step.second); // get the time to compare whether there is enough time to move

                MoveToGridPosition(nextGridPosition, stepTime); // move according to the time that we still have
            }
            else if(!isMoving && canPlayStopAnimation) //  if it cannot move, then stop animation
            {
                StartCoroutine(SetStopAnimation());
            }
        }
    }

    private void MoveToGridPosition(Vector3Int gridPos, TimeSpan stepTime) // Combined with IEnumerator instead of a direct IEnumerator method, it can be added more functions.
    {
        npcMoveRoutine = StartCoroutine(MoveRoutine(gridPos, stepTime));
    }

    private IEnumerator MoveRoutine(Vector3Int gridPos, TimeSpan stepTime)
    {
        npcMove = true;
        nextWorldPosition = GetWorldPosition(gridPos); // Get World Position is used to calculate the center point

        // Still have time to Move
        if (stepTime > GameTime)
        {
            // The time used to move, which is measured by seconds
            float timeToMove = (float)(stepTime.TotalSeconds - GameTime.TotalSeconds);
            // Real move distance
            float distance = Vector3.Distance(transform.position, nextWorldPosition);
            // Max Real move speed
            float speed = Mathf.Max(minSpeed, (distance / timeToMove / Settings.secondThreshold));

            if (speed <= maxSpeed) // if there is still a long time to move, we can adjust the position gradually
            {
                while (Vector3.Distance(transform.position, nextWorldPosition) > Settings.pixelSize) // if the difference between current position and target position is bigger
                                                                                                     // than the pixel size. Then, we have to continue move
                {                                                                                    
                    dir = (nextWorldPosition - transform.position).normalized; // get the direction

                    Vector2 posOffset = new Vector2(dir.x * speed * Time.fixedDeltaTime, dir.y * speed * Time.fixedDeltaTime); // continuing get the position to move
                    rb.MovePosition(rb.position + posOffset); // moves the npc's rigidbody, namely the npc
                    yield return new WaitForFixedUpdate(); // wait for next update, which is quite a small time, which only cause a little change
                }
            }
        }

        // if we do not have too much time to move, where the required spped is much more than maxSpeed we set, then just move the npc directly.
        rb.position = nextWorldPosition;
        currentGridPosition = gridPos;
        nextGridPosition = currentGridPosition;

        npcMove = false;
    }


    /// <summary>
    /// Build the path according to the give NPC schedule
    /// </summary>
    /// <param name="schedule"></param>
    public void BuildPath(ScheduleDetails schedule)
    {
        movementSteps.Clear();
        currentSchedule = schedule;
        targetScene = schedule.targetScene;
        targetGridPosition = (Vector3Int)schedule.targetGridPosition;
        stopAnimationClip = schedule.clipAtStop;
        this.interactable = schedule.interactable;

        if (schedule.targetScene == currentScene)
        {
            AStar.Instance.BuildPath(schedule.targetScene, (Vector2Int)currentGridPosition, schedule.targetGridPosition, movementSteps);
        }
        else if (schedule.targetScene != currentScene)
        {
            SceneRoute sceneRoute = NPCManager.Instance.GetSceneRoute(currentScene, schedule.targetScene);

            if(sceneRoute != null)
            {
                for(int i = 0; i < sceneRoute.scenePathList.Count; i++)
                {
                    Vector2Int fromPos, gotoPos;
                    ScenePath path = sceneRoute.scenePathList[i];

                    if(path.fromGridCell.x >= Settings.maxGridSize) // if the fromGrid Cell is 99999 bigger than maxGridSize
                    {
                        fromPos = (Vector2Int)currentGridPosition; // then it means the from position is the current position of NPC
                    }
                    else // otherwise, it needs to take the from grid cell position in the settings
                    {
                        fromPos = path.fromGridCell;
                    }

                    if(path.gotoGridCell.x >= Settings.maxGridSize) // if the gotoGrid Cell is 99999 bigger than maxGridSize
                    {
                        gotoPos = schedule.targetGridPosition; // then it means it should take the target grid position
                    }
                    else // if not
                    {
                        gotoPos = path.gotoGridCell; // it should go the position in settings
                    }
                    Debug.Log(path.sceneName);
                    AStar.Instance.BuildPath(path.sceneName, fromPos, gotoPos, movementSteps); // generate the movement steps
                }
            }
        }

        if (movementSteps.Count > 1)
        {
            // update the time according to each step
            UpdateTimeOnPath();
        }
    }


    private void UpdateTimeOnPath() // To record the time for every step
    {
        MovementStep previousSetp = null; // create a previous step to determine

        TimeSpan currentGameTime = GameTime; // load the GameTime from Time Manager

        foreach (MovementStep step in movementSteps) // for all the movement steps generated by A star algorithm, we should record time for each step
        {
            if (previousSetp == null) // if this is the first step, then it will be assigned to previous step
                previousSetp = step;

            step.hour = currentGameTime.Hours; // record the current Game Time Calculated by the below code
            step.minute = currentGameTime.Minutes;
            step.second = currentGameTime.Seconds;

            TimeSpan gridMovementStepTime; // record the time it needs to move

            if (MoveInDiagonal(step, previousSetp)) // if moves in Diagonal, it should compute in a different way
                gridMovementStepTime = new TimeSpan(0, 0, (int)(Settings.gridCellDiagonalSize / normalSpeed / Settings.secondThreshold));
            else // otherwise it should compute as normal
                gridMovementStepTime = new TimeSpan(0, 0, (int)(Settings.gridCellSize / normalSpeed / Settings.secondThreshold));

            // Continuous addition to get the next step
            currentGameTime = currentGameTime.Add(gridMovementStepTime);
            // loop
            previousSetp = step;
        }
    }

    /// <summary>
    /// Judge whether it goes to the diagonal
    /// </summary>
    /// <param name="currentStep"></param>
    /// <param name="previousStep"></param>
    /// <returns></returns>
    private bool MoveInDiagonal(MovementStep currentStep, MovementStep previousStep)
    {
        return (currentStep.gridCoordinate.x != previousStep.gridCoordinate.x) && (currentStep.gridCoordinate.y != previousStep.gridCoordinate.y);
    }

    private Vector3 GetWorldPosition(Vector3Int gridPos)
    {
        Vector3 worldPos = grid.CellToWorld(gridPos);

        return new Vector3(worldPos.x + Settings.gridCellSize / 2f, worldPos.y + Settings.gridCellSize / 2f); // dicided by 2 because we only need the center point
    }

    private void SwitchAnimation()
    {
        isMoving = transform.position != GetWorldPosition(targetGridPosition);

        anim.SetBool("isMoving", isMoving);
        if (isMoving)
        {
            anim.SetBool("Exit", true);
            anim.SetFloat("DirX", dir.x);
            anim.SetFloat("DirY", dir.y);
        }
        else
        {
            anim.SetBool("Exit", false);
        }
    }

    private IEnumerator SetStopAnimation()
    {
        // Force to face to the Camera
        anim.SetFloat("DirX", 0);
        anim.SetFloat("DirY", -1);

        animationBreakTime = Settings.animationBreakTime;
        if (stopAnimationClip != null)
        {
            animOverride[blankAnimationClip] = stopAnimationClip;
            anim.SetBool("EventAnimation", true);
            yield return null;
            anim.SetBool("EventAnimation", false);
        }
        else
        {
            animOverride[stopAnimationClip] = blankAnimationClip;
            anim.SetBool("EventAnimation", false);
        }
    }

    #region Set the Display properties of NPC
    private void SetActiveInScene()
    {
        spriteRenderer.enabled = true;
        coll.enabled = true;
        transform.GetChild(0).gameObject.SetActive(true);
    }

    private void SetInactiveInScene()
    {
        spriteRenderer.enabled = false;
        coll.enabled = false;
        transform.GetChild(0).gameObject.SetActive(false);
    }

    public GameSaveData GenerateSaveData()
    {
        GameSaveData saveData = new GameSaveData();
        saveData.characterPosDict = new Dictionary<string, SerializableVector3>();
        saveData.characterPosDict.Add("targetGridPosition", new SerializableVector3(targetGridPosition));
        saveData.characterPosDict.Add("currentPosition", new SerializableVector3(transform.position));
        saveData.dataSceneName = currentScene;
        saveData.targetScene = this.targetScene;
        if (stopAnimationClip != null)
        {
            saveData.animationInstanceID = stopAnimationClip.GetInstanceID();
        }
        saveData.interactable = this.interactable;
        saveData.timeDict = new Dictionary<string, int>();
        saveData.timeDict.Add("currentSeason", (int)currentSeason);
        return saveData;
    }

    public void RestoreData(GameSaveData saveData)
    {
        isInitialised = true;
        isFirstLoad = false;

        currentScene = saveData.dataSceneName;
        targetScene = saveData.targetScene;

        Vector3 pos = saveData.characterPosDict["currentPosition"].ToVector3();
        Vector3Int gridPos = (Vector3Int)saveData.characterPosDict["targetGridPosition"].ToVector2Int();

        transform.position = pos;
        targetGridPosition = gridPos;

        if (saveData.animationInstanceID != 0)
        {
            this.stopAnimationClip = Resources.InstanceIDToObject(saveData.animationInstanceID) as AnimationClip;
        }

        this.interactable = saveData.interactable;
        this.currentSeason = (Season)saveData.timeDict["currentSeason"];
    }
    #endregion


}