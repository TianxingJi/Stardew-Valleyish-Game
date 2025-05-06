using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TFarm.Inventory;
using TFarm.Save;

public class Player : MonoBehaviour, ISaveable
{
    // set the variable to store regidbody of the player
    private Rigidbody2D rigidbodyPlayer;

    // set variables to controll the movement of the player
    public float speed;
    private float inputX;
    private float inputY;

    // combine above variables to set a movement input variable
    private Vector2 movementInput;

    // Get all of the animators to controll the animations
    private Animator[] animators;
    private bool isMoving;

    private bool inputDisable;

    // Animation Parameters
    private float mouseX;
    private float mouseY;
    private bool useTool;

    public string GUID => GetComponent<DataGUID>().guid;

    // get the rigidbody
    private void Awake()
    {
        rigidbodyPlayer = GetComponent<Rigidbody2D>();
        animators = GetComponentsInChildren<Animator>();
        inputDisable = true;
    }

    private void Start()
    {
        ISaveable saveable = this; // The current script is the saveable
        saveable.RegisterSaveable(); //  and register it
    }

    private void OnEnable()
    {
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
        EventHandler.MoveToPosition += onMoveToPosition;
        EventHandler.MouseClickedEvent += OnMouseClickedEvent;
        EventHandler.UpdateGameStateEvent += OnUpdateGameStateEvent;
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        EventHandler.EndGameEvent += OnEndGameEvent;
    }

    private void OnDisable()
    {
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
        EventHandler.MoveToPosition -= onMoveToPosition;
        EventHandler.MouseClickedEvent -= OnMouseClickedEvent;
        EventHandler.UpdateGameStateEvent -= OnUpdateGameStateEvent;
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
        EventHandler.EndGameEvent -= OnEndGameEvent;
    }

    // read the input continuously
    private void Update()
    {
        if (!inputDisable)// if the input is not disable, the player can move
            PlayerInput();
        else // if the input is disable, the player cannot move
            isMoving = false;
        SwitchAnimation();
    }

    // this method is used to move rigidbody (update) per certain fixed time
    private void FixedUpdate()
    {
        if (!inputDisable)
        {
            Movement();
        }

    }

    private void OnStartNewGameEvent(int obj) // When the game is just started, we do not want the player to move
    {
        inputDisable = false;
        transform.position = Settings.playerStartPos;
    }

    private void OnEndGameEvent()
    {
        inputDisable = true;
    }


    private void OnUpdateGameStateEvent(GameState gameState)
    {
        // The value is changed in NPCFunction Script
        switch (gameState)
        {
            case GameState.GamePlay:
                inputDisable = false;
                break;

            case GameState.Pause:
                inputDisable = true;
                break;
        }
    }

    private void OnMouseClickedEvent(Vector3 mouseWorldPos, ItemDetails itemDetails)
    {
        if (useTool) return;

        // TODO: Finish Animation
        if(itemDetails.itemType != ItemType.Seed && itemDetails.itemType != ItemType.Commodity && itemDetails.itemType != ItemType.Funiture)
        {
            mouseX = mouseWorldPos.x - transform.position.x;
            mouseY = mouseWorldPos.y - (transform.position.y + 0.85f); // 0.85 is Player's Height

            if (Mathf.Abs(mouseX) > Mathf.Abs(mouseY)) // Determine the facing direction of Player
                mouseY = 0;
            else
                mouseX = 0;

            StartCoroutine(UseToolRoutine(mouseWorldPos, itemDetails));
        }
        else
        {
            EventHandler.CallExecuteActionAfterAnimation(mouseWorldPos, itemDetails);
        }
    }

    private IEnumerator UseToolRoutine(Vector3 mouseWorldPos, ItemDetails itemDetails)
    {
        useTool = true;
        inputDisable = true;
        yield return null;
        foreach (var anim in animators)
        {
            anim.SetTrigger("useTool");
            //the diretion where the player face
            anim.SetFloat("InputX", mouseX);
            anim.SetFloat("InputY", mouseY);
        }
        yield return new WaitForSeconds(0.45f);
        EventHandler.CallExecuteActionAfterAnimation(mouseWorldPos, itemDetails);
        yield return new WaitForSeconds(0.25f);
        //wait for the end of the animation
        useTool = false;
        inputDisable = false;
    }

    

    private void onMoveToPosition(Vector3 targetPosition)
    {
        transform.position = targetPosition;
    }

    private void OnAfterSceneLoadedEvent()
    {
        inputDisable = false;
    }

    private void OnBeforeSceneUnloadEvent()
    {
        inputDisable = true;
    }

    // set a method to supervise the input on keyboard
    private void PlayerInput()
    {
        // read the input from keyboard including horizontal and vertical positions
        inputX = Input.GetAxisRaw("Horizontal");
        inputY = Input.GetAxisRaw("Vertical");

        // if X and Y are pressed simultaneously, which means the movement is not straight
        if(inputX != 0 && inputY != 0)
        {
            // set a limited speed
            inputX *= 0.6f;
            inputY *= 0.6f;
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            inputX *= 0.5f;
            inputY *= 0.5f;
        }
        // combine inputX and inputY
        movementInput = new Vector2(inputX, inputY);

        isMoving = movementInput != Vector2.zero; // Judge whether the player is moving
    }

    // use the rigidbody to move the player visually
    private void Movement()
    {
        // In the view of this farming game, instead of adding force, we just change the values of axis value by adding the position change
        // Time.deltaTime is to match different fps
        rigidbodyPlayer.MovePosition(rigidbodyPlayer.position + movementInput * speed * Time.deltaTime);
    }

    private void SwitchAnimation()
    {
        foreach (var anim in animators) // loop all the animators of Player parts (like Hair and Body)
        {
            anim.SetBool("isMoving", isMoving); // Set Animation parameters to trigger animations
            anim.SetFloat("mouseX", mouseX);
            anim.SetFloat("mouseY", mouseY);

            if (isMoving)
            {
                anim.SetFloat("InputX", inputX);
                anim.SetFloat("InputY", inputY);
            }
        }
    }

    public GameSaveData GenerateSaveData()
    {
        GameSaveData saveData = new GameSaveData();
        saveData.characterPosDict = new Dictionary<string, SerializableVector3>();
        saveData.characterPosDict.Add(this.name, new SerializableVector3(transform.position));

        return saveData;
    }

    public void RestoreData(GameSaveData saveData)
    {
        var targetPosition = saveData.characterPosDict[this.name].ToVector3();

        transform.position = targetPosition;
    }
}
