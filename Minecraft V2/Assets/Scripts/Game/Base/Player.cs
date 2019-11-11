using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public bool isGrounded;
    public bool isSprinting;

    private Transform cam;
    private World world;

    private float horizontal;
    private float vertical;
    private float mouseHorizontal;
    private float mouseVertical;
    private Vector3 velocity;
    private float verticalMomentum = 0;
    private bool jumpRequest;
    private byte OldBlockIndex;

    public float gravity = -9.8f;
    public float walkSpeed = 4f;
    public float sprintSpeed = 6f;
    public float jumpForce = 5;

    public float playerWidth = 0.0f;
    public float boundsTolerance = 0.1f;

    public Transform highlightBlock;
    public Transform placeBlock;
    public float checkIncrement = 0.1f;
    public float reach = 6f;
    public ToolBar toolbar;

    public List<BlocksToSave> PlayersBlocksPlaced = new List<BlocksToSave>();
    public VoxelData.GameModes GameMode;
    public bool AllowCheats;

    private void Start()
    {
        cam = GameObject.Find("Main Camera").transform;
        world = GameObject.Find("World").GetComponent<World>();

        world.inUI = false;
        world.inPauseScreen = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            world.inUI = !world.inUI;
        }

        if (!world.inUI && !world.inPauseScreen)
        {
            GetPlayerInput();
            placeCursorBlocks();
        }
    }

    private void FixedUpdate()
    {
        if (!world.inUI)
        {  
            if (jumpRequest)
                Jump();
        }
        else
        {
            mouseHorizontal = 0;
            mouseVertical = 0;
        }

        CalcualteVelocity();

        transform.Rotate(Vector3.up * mouseHorizontal * world.settings.mouseSensitivity / 2);
        cam.Rotate(Vector3.right * -mouseVertical * world.settings.mouseSensitivity / 2);
        transform.Translate(velocity, Space.World);
    }

    private void CalcualteVelocity()
    {
        //Affect vertical momentum
        if (verticalMomentum > gravity)
            verticalMomentum += Time.fixedDeltaTime * gravity;

        if (isSprinting)
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * sprintSpeed;
        else
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * walkSpeed;

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

        if (isGrounded && Input.GetButtonDown("Jump"))
            jumpRequest = true;

        if (highlightBlock.gameObject.activeSelf || placeBlock.gameObject.activeSelf)
        {
            if (Input.GetMouseButtonDown(0)) //Destroy
            {
                int slotindex = 0;
                foreach (UIItemSlots s in toolbar.slots)
                {
                    if (!s.itemslot.HasItem)
                    {
                        toolbar.slots[slotindex].itemslot.InsertStack(new ItemStack(world.getChunkFromVector3(highlightBlock.position).GetVoxelFromGlobalVector3(highlightBlock.position).id, 1));
                        break;
                    }
                    if (s.itemslot.stack.ID == world.getChunkFromVector3(highlightBlock.position).GetVoxelFromGlobalVector3(highlightBlock.position).id && s.itemslot.stack.amount < 64)
                    {
                        toolbar.slots[slotindex].itemslot.Add(1);
                        break;
                    }
                    else
                        slotindex++;
                }

                world.getChunkFromVector3(highlightBlock.position).EditVoxel(highlightBlock.position, 0);

                Vector3Int BlockPos = Helpers.Vector3ToVector3Int(highlightBlock.position);
                CheckIfBlockWasPlacedBefore(BlockPos);
                PlayersBlocksPlaced.Add(new BlocksToSave(BlockPos, 0));
            }

            if (Input.GetMouseButtonDown(1)) //Place
            {
                if (toolbar.slots[toolbar.slotIndex].HasItem)
                {
                    world.getChunkFromVector3(placeBlock.position).EditVoxel(placeBlock.position, toolbar.slots[toolbar.slotIndex].itemslot.stack.ID);
                    toolbar.slots[toolbar.slotIndex].itemslot.Take(1);

                    Vector3Int BlockPos = Helpers.Vector3ToVector3Int(placeBlock.position);
                    CheckIfBlockWasPlacedBefore(BlockPos);
                    PlayersBlocksPlaced.Add(new BlocksToSave(BlockPos, toolbar.slots[toolbar.slotIndex].itemslot.stack.ID));
                }
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
        if (world.CheckForVoxel(new Vector3(Mathf.FloorToInt(transform.position.x - playerWidth), transform.position.y + 1.8f + upSpeed, Mathf.FloorToInt(transform.position.z - playerWidth))) ||
            world.CheckForVoxel(new Vector3(Mathf.FloorToInt(transform.position.x + playerWidth), transform.position.y + 1.8f + upSpeed, Mathf.FloorToInt(transform.position.z - playerWidth))) ||
            world.CheckForVoxel(new Vector3(Mathf.FloorToInt(transform.position.x + playerWidth), transform.position.y + 1.8f + upSpeed, Mathf.FloorToInt(transform.position.z + playerWidth))) ||
            world.CheckForVoxel(new Vector3(Mathf.FloorToInt(transform.position.x - playerWidth), transform.position.y + 1.8f + upSpeed, Mathf.FloorToInt(transform.position.z + playerWidth))))
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

public class BlocksToSave
{
    public Vector3Int pos;
    public byte id; //If id == 0 block got removed

    public BlocksToSave(Vector3Int _pos, byte _id)
    {
        pos = _pos;
        id = _id;
    }
}
