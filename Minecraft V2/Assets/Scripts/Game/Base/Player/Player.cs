using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public bool isGrounded;
    public bool isSprinting;
    public bool isCrouching;

    public int health = 20; //20 - full

    private Transform cam;
    private World world;

    private bool _inUI = false;

    [SerializeField] private GameObject PausePanel;
    private bool _inPauseScreen = false;

    private bool _inConsole = false;
    private bool _isDead = false;

    public GameObject creativeInventoryWindow;
    public GameObject survivalInventoryWindow;
    public GameObject cursorSlot;
    public GameObject MainCamera;
    public ToolBar toolbar;

    private float horizontal;
    private float vertical;
    private float mouseHorizontal;
    private float mouseVertical;
    private Vector3 velocity;
    private float verticalMomentum = 0;
    private bool jumpRequest;
    private byte OldBlockIndex;

    public float gravity = -19f;
    public float walkSpeed = 4f;
    public float sprintSpeed = 6f;
    public float jumpForce = 6.5f;
    public float crouchSpeed = 2f;

    public float playerWidth = 0.01f;
    public float playerHeight = 1.8f;

    public Transform highlightBlock;
    public Transform placeBlock;
    public float checkIncrement = 0.1f;
    public float reach = 6f;
    public float clampAngle = 90.0f;
    private float rotY = 0.0f; // rotation around the up/y axis
    private float rotX = 0.0f; // rotation around the right/x axis

    public List<BlocksToSave> PlayersBlocksPlaced = new List<BlocksToSave>();
    public VoxelData.GameModes GameMode;
    public bool AllowCheats;
    public Vector3 campos;

    private void Start()
    {
        cam = MainCamera.transform;
        world = GameObject.Find("World").GetComponent<World>();
        Helpers.toolbar = toolbar;
        Vector3 rot = cam.transform.localRotation.eulerAngles;
        rotY = rot.y;
        rotX = rot.x;
        campos = Camera.main.transform.position;

        inUI = false;
        inPauseScreen = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            inUI = !inUI;
        }

        if (!inUI && !inPauseScreen && !inConsole)
        {
            GetPlayerInput();
            placeCursorBlocks();
        }

        EffectsHandler();
    }

    bool lastIsSprinting;
    bool lastIsCrouching;
    void EffectsHandler()
    {
        if (isSprinting != lastIsSprinting && isSprinting)
            Camera.main.fieldOfView += 5;
        else if (isSprinting != lastIsSprinting && !isSprinting)
            Camera.main.fieldOfView -= 5;

        if (isCrouching && isCrouching != lastIsCrouching)
            Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y - 0.5f, Camera.main.transform.position.z);
        else if (!isCrouching && isCrouching != lastIsCrouching)
            Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y + 0.5f, Camera.main.transform.position.z);

        lastIsSprinting = isSprinting;
        lastIsCrouching = isCrouching;
    }

    private void FixedUpdate()
    {
        CalcualteVelocity();

        if (!inUI && !inPauseScreen && !inConsole)
        {  
            if (jumpRequest)
                Jump();
        }
        else
        {
            mouseHorizontal = 0;
            mouseVertical = 0;
            if (isGrounded)
            {
                velocity.x = 0;
                velocity.z = 0;
            }
        }

        rotY += mouseHorizontal * world.settings.mouseSensitivity * 0.1f * Time.fixedDeltaTime; //Xaxis
        rotX += -mouseVertical * world.settings.mouseSensitivity * 0.1f * Time.fixedDeltaTime; //Yaxis
        rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle); //Clamp Y-axis
        cam.rotation = Quaternion.Euler(rotX, rotY, 0);
        transform.rotation = Quaternion.Euler(0, rotY, 0);

        transform.Translate(velocity, Space.World);
    }

    private void CalcualteVelocity()
    {
        //Affect vertical momentum
        if (verticalMomentum > gravity)
            verticalMomentum += Time.fixedDeltaTime * gravity;

        velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime;

        if(isCrouching && isSprinting || !isSprinting && isCrouching)
            velocity *= crouchSpeed;
        else if(isSprinting && !isCrouching)
            velocity *= sprintSpeed;
        else
            velocity *= walkSpeed;

        //Apply vertical momentum (falling and jumping)
        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;

        if ((velocity.z > 0 && front) || (velocity.z < 0 && back)) //If is movement then check
            velocity.z = 0;
        if ((velocity.x > 0 && right) || (velocity.x < 0 && left)) //If is movement then check
            velocity.x = 0;

        if (velocity.y < 0) //Is falling
            velocity.y = checkDownSpeed(velocity.y);
        else if (velocity.y > 0)
            velocity.y = checkUpSpeed(velocity.y);
    }

    void Jump()
    {
        verticalMomentum = jumpForce;
        isGrounded = false;
        jumpRequest = false;
    }

    private void GetPlayerInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        mouseHorizontal = Input.GetAxis("Mouse X") / Time.fixedDeltaTime;
        mouseVertical = Input.GetAxis("Mouse Y") / Time.fixedDeltaTime;

        if (Input.GetButtonDown("Sprint")) //Pressed
            isSprinting = true;
        if (Input.GetButtonUp("Sprint"))
            isSprinting = false;
        if (Input.GetKeyDown(KeyCode.LeftControl))
            isCrouching = true;
        if (Input.GetKeyUp(KeyCode.LeftControl))
            isCrouching = false;

        if (isGrounded && Input.GetButtonDown("Jump"))
            jumpRequest = true;

        HandleBlockPlacingAndDestroying();
    }

    void HandleBlockPlacingAndDestroying()
    {
        if (highlightBlock.gameObject.activeSelf || placeBlock.gameObject.activeSelf)
        {
            if (Input.GetMouseButtonDown(0)) //Destroy
            {
                int slotindex = 0;
                bool inToolbarFoundSlot = false;
                Chunk thisChunk = world.getChunkFromVector3(highlightBlock.position);
                VoxelState voxel = thisChunk.GetVoxelFromGlobalVector3(highlightBlock.position);
                foreach (UIItemSlots s in Helpers.toolbar.slots)
                {
                    if (!s.itemslot.HasItem)
                    {
                        Helpers.toolbar.slots[slotindex].itemslot.InsertStack(new ItemStack(voxel.id, 1));
                        inToolbarFoundSlot = true;
                        break;
                    }
                    if (s.itemslot.stack.ID == voxel.id && s.itemslot.stack.amount < 64)
                    {
                        Helpers.toolbar.slots[slotindex].itemslot.Add(1);
                        inToolbarFoundSlot = true;
                        break;
                    }
                    else
                        slotindex++;
                }

                if (GameMode == VoxelData.GameModes.Survival && !inToolbarFoundSlot) //Surivival Inventory
                {
                    bool blockInEQ = false;
                    foreach (UIItemSlots item in Helpers.itemslots) //Loop to see if any of slots has this item
                    {
                        if (item.HasItem && item.itemslot.stack.ID == voxel.id && item.itemslot.stack.amount < 64)
                        {
                            item.itemslot.Add(1);
                            item.UpdateSlot();

                            blockInEQ = true;
                            break;
                        }
                    }

                    if (!blockInEQ) //Loop for the first free slot in eq
                    {
                        foreach (UIItemSlots item in Helpers.itemslots)
                        {
                            if (!item.HasItem)
                            {
                                ItemStack stack = new ItemStack(voxel.id, 1);
                                ItemSlot newSlot = new ItemSlot(item, stack);
                                item.itemslot = newSlot;
                                item.UpdateSlot();
                                break;
                            }
                        }
                    }
                }

                thisChunk.EditVoxel(highlightBlock.position, 0);

                Vector3Int BlockPos = Helpers.Vector3ToVector3Int(highlightBlock.position);
                CheckIfBlockWasPlacedBefore(BlockPos);
                PlayersBlocksPlaced.Add(new BlocksToSave(BlockPos, 0));
            }

            if (Input.GetMouseButtonDown(1)) //Place
            {
                if (Helpers.toolbar.slots[Helpers.toolbar.slotIndex].HasItem)
                {
                    world.getChunkFromVector3(placeBlock.position).EditVoxel(placeBlock.position, Helpers.toolbar.slots[Helpers.toolbar.slotIndex].itemslot.stack.ID);

                    if(GameMode == VoxelData.GameModes.Survival)
                        Helpers.toolbar.slots[Helpers.toolbar.slotIndex].itemslot.Take(1);

                    Vector3Int BlockPos = Helpers.Vector3ToVector3Int(placeBlock.position);
                    CheckIfBlockWasPlacedBefore(BlockPos);
                    PlayersBlocksPlaced.Add(new BlocksToSave(BlockPos, Helpers.toolbar.slots[Helpers.toolbar.slotIndex].itemslot.stack.ID));
                }
            }
        }
    }


    public bool inUI
    {
        get { return _inUI; }
        set
        {
            _inUI = value;
            if (_inUI)
            {
                if (!_inConsole)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    if (GameMode == VoxelData.GameModes.Creative)
                        creativeInventoryWindow.SetActive(true);
                    else
                        survivalInventoryWindow.SetActive(true);

                    Helpers.toolbar.transform.localScale = Helpers.toolbarScaleOpenedSurivivalInevntory;
                    Helpers.toolbar.transform.position = Helpers.toolbarPosOpenedSurivivalInevntory;

                    cursorSlot.SetActive(true);
                }
            }
            else
            {
                if (!_inConsole)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    creativeInventoryWindow.SetActive(false);
                    survivalInventoryWindow.SetActive(false);

                    Helpers.toolbar.transform.localScale = Helpers.toolbarScaleClosedSurivivalInevntory;
                    Helpers.toolbar.transform.position = Helpers.toolbarPosClosedSurivivalInevntory;
                    cursorSlot.SetActive(false);
                }
            }
        }
    }

    public bool inPauseScreen
    {
        get { return _inPauseScreen; }
        set
        {
            _inPauseScreen = value;
            if (_inPauseScreen)
            {
                if (!_inConsole)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    PausePanel.SetActive(true);
                }
            }
            else
            {
                if (!_inConsole)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    PausePanel.SetActive(false);
                    world.SettingsPauseScreenPanel.SetActive(false);
                }
            }
        }
    }

    public bool inConsole
    {
        get { return _inConsole; }
        set
        {
            _inConsole = value;
            if (_inConsole)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    public bool isDead
    {
        get { return _isDead; }
        set
        {
            _isDead = value;
            if (_isDead)
            {
                health = 0;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    bool CheckIfBlockWasPlacedBefore(Vector3Int pos)
    {
        bool placedBefore = false;

        foreach(BlocksToSave b in PlayersBlocksPlaced)
        {
            if (b.pos == pos)
            {
                PlayersBlocksPlaced.Remove(b);
                placedBefore = true;
                break;
            }    
        }

        return placedBefore;
    }

    private void placeCursorBlocks()
    {
        float step = checkIncrement;
        Vector3 lastPos = new Vector3();

        while (step < reach)
        {
            Vector3 pos = cam.position + (cam.forward * step);
            if (world.CheckForVoxel(pos))
            {
                highlightBlock.position = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
                placeBlock.position = lastPos;

                highlightBlock.gameObject.SetActive(true);
                placeBlock.gameObject.SetActive(true);

                return;
            }

            lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
            step += checkIncrement;
        }

        highlightBlock.gameObject.SetActive(false);
        placeBlock.gameObject.SetActive(false);
    }

    #region SpeedChecks
    private float checkDownSpeed(float downSpeed)
    {
        if(world.CheckForVoxel(new Vector3(Mathf.FloorToInt(transform.position.x - playerWidth), transform.position.y + downSpeed, Mathf.FloorToInt(transform.position.z - playerWidth))) ||
           world.CheckForVoxel(new Vector3(Mathf.FloorToInt(transform.position.x + playerWidth), transform.position.y + downSpeed, Mathf.FloorToInt(transform.position.z - playerWidth))) ||
           world.CheckForVoxel(new Vector3(Mathf.FloorToInt(transform.position.x + playerWidth), transform.position.y + downSpeed, Mathf.FloorToInt(transform.position.z + playerWidth))) ||
           world.CheckForVoxel(new Vector3(Mathf.FloorToInt(transform.position.x - playerWidth), transform.position.y + downSpeed, Mathf.FloorToInt(transform.position.z + playerWidth))))
        {
            isGrounded = true;
            return 0;
        }
        else
        {
            isGrounded = false;
            return downSpeed;
        }
    }

    private float checkUpSpeed(float upSpeed)
    {
        if (world.CheckForVoxel(new Vector3(Mathf.FloorToInt(transform.position.x - playerWidth), transform.position.y + playerHeight + upSpeed, Mathf.FloorToInt(transform.position.z - playerWidth))) ||
            world.CheckForVoxel(new Vector3(Mathf.FloorToInt(transform.position.x + playerWidth), transform.position.y + playerHeight + upSpeed, Mathf.FloorToInt(transform.position.z - playerWidth))) ||
            world.CheckForVoxel(new Vector3(Mathf.FloorToInt(transform.position.x + playerWidth), transform.position.y + playerHeight + upSpeed, Mathf.FloorToInt(transform.position.z + playerWidth))) ||
            world.CheckForVoxel(new Vector3(Mathf.FloorToInt(transform.position.x - playerWidth), transform.position.y + playerHeight + upSpeed, Mathf.FloorToInt(transform.position.z + playerWidth))))
        {
            return 0;
        }
        else
        {
            return upSpeed;
        }
    }

    public bool front
    {
        get {
            if (world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z + playerWidth)) ||
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z + playerWidth))) //To check the block on head level
                return true;
            else
                return false;
        }
    }

    public bool back
    {
        get
        {
            if (world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z - playerWidth)) ||
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z - playerWidth))) //To check the block on head level
                return true;
            else
                return false;
        }
    }

    public bool left
    {
        get
        {
            if (world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y, transform.position.z )) ||
                world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 1f, transform.position.z))) //To check the block on head level
                return true;
            else
                return false;
        }
    }

    public bool right
    {
        get
        {
            if (world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y, transform.position.z )) ||
                world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 1f, transform.position.z ))) //To check the block on head level
                return true;
            else
                return false;
        }
    }
    #endregion
}
