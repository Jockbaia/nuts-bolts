using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxLogic : MonoBehaviour
{
    AudioSource audioSrc;

    public bool isHeavy = false;

    [SerializeField]
    private float moveSpeed = 5f;

    private Transform movePoint;

    private enum Position
    {
        Up,
        Down,
        Left,
        Right,
        None
    }

    // Nearby position of the player w.r.t. box
    private Position playerPos;

    // Nearby tiles
    private bool upTileFree = false;
    private bool downTileFree = false;
    private bool leftTileFree = false;
    private bool rightTileFree = false;    
    
    // 2 spaces away tiles
    private bool upTileFreex2 = false;
    private bool downTileFreex2 = false;
    private bool leftTileFreex2 = false;
    private bool rightTileFreex2 = false;

    // Check if special action button is pressed
    private PlayerLogic playerLogic = null;

    MapGenerator mapGenerator;
    private int mapZlen = 0;
    private int mapXlen = 0;
    private int mapXoffset = 0;

    void Start()
    {
        audioSrc = GetComponent<AudioSource>();

        movePoint = transform.Find("Box Move Point");
        movePoint.parent = transform.parent;

        (mapZlen, mapXlen, mapXoffset) = GetMapBounds();
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, movePoint.position, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, movePoint.position) == 0f)
        {
            CheckCollisions();

            if (playerLogic == null) return;
            
            if (playerLogic.specialAction)
            {
                var selectedPower = playerLogic.GetComponent<RobotPowers>().selectedPower;
                
                if (isHeavy && selectedPower != RobotPowers.PowerSelector.PushPullHeavy)
                    return;
                if (!isHeavy && selectedPower != RobotPowers.PowerSelector.PushPull && selectedPower != RobotPowers.PowerSelector.PushPullHeavy) 
                    return;

                // Detect Push
                if (playerPos == Position.Down && playerLogic.movementInput.y > 0.9f && upTileFree 
                    && playerLogic.transform.rotation.eulerAngles.y == 0f
                    && transform.position.z < mapZlen - 1)
                { // Push -> Up
                    movePoint.position += new Vector3(0, 0, 1);
                    playerLogic.movePoint.position += new Vector3(0, 0, 1);
                    audioSrc.PlayOneShot(audioSrc.clip);
                }
                else if (playerPos == Position.Right && playerLogic.movementInput.x < -0.9f && leftTileFree
                    && playerLogic.transform.rotation.eulerAngles.y == 270f
                    && transform.position.x > mapXoffset)
                { // Push -> Left
                    movePoint.position += new Vector3(-1, 0, 0);
                    playerLogic.movePoint.position += new Vector3(-1, 0, 0);
                    audioSrc.PlayOneShot(audioSrc.clip);
                }
                else if (playerPos == Position.Left && playerLogic.movementInput.x > 0.9f && rightTileFree
                    && playerLogic.transform.rotation.eulerAngles.y == 90f
                    && transform.position.x < mapXlen + mapXoffset - 1)
                { // Push -> Right
                    movePoint.position += new Vector3(1, 0, 0);
                    playerLogic.movePoint.position += new Vector3(1, 0, 0);
                    audioSrc.PlayOneShot(audioSrc.clip);
                }
                else if (playerPos == Position.Up && playerLogic.movementInput.y < -0.9f && downTileFree
                    && playerLogic.transform.rotation.eulerAngles.y == 180f
                    && transform.position.z > 0)
                { // Push -> Down
                    movePoint.position += new Vector3(0, 0, -1);
                    playerLogic.movePoint.position += new Vector3(0, 0, -1);
                    audioSrc.PlayOneShot(audioSrc.clip);
                }

                // Detect Drag
                else if (playerPos == Position.Down && playerLogic.movementInput.y < -0.9f && downTileFreex2
                    && playerLogic.transform.rotation.eulerAngles.y == 0f
                    && transform.position.z > 1)
                { // Pull -> Down
                    movePoint.position += new Vector3(0, 0, -1);
                    audioSrc.PlayOneShot(audioSrc.clip);
                }
                else if (playerPos == Position.Up && playerLogic.movementInput.y > 0.9f && upTileFreex2
                    && playerLogic.transform.rotation.eulerAngles.y == 180f
                    && transform.position.z < mapZlen - 2)
                { // Pull -> Up
                    movePoint.position += new Vector3(0, 0, 1);
                    audioSrc.PlayOneShot(audioSrc.clip);
                }
                else if (playerPos == Position.Left && playerLogic.movementInput.x < -0.9f && leftTileFreex2
                    && playerLogic.transform.rotation.eulerAngles.y == 90f
                    && transform.position.x > mapXoffset + 1)
                { // Pull -> Left
                    movePoint.position += new Vector3(-1, 0, 0);
                    audioSrc.PlayOneShot(audioSrc.clip);
                }
                else if (playerPos == Position.Right && playerLogic.movementInput.x > 0.9f && rightTileFreex2
                    && playerLogic.transform.rotation.eulerAngles.y == 270f
                    && transform.position.x < mapXoffset + mapXlen - 2)
                { // Pull -> Right
                    movePoint.position += new Vector3(1, 0, 0);
                    audioSrc.PlayOneShot(audioSrc.clip);
                }
            }
        }
    }

    (int, int, int) GetMapBounds()
    {
        GameObject map;
        string mapName;

        // Since P1Map and P2Map have 50 Xoffset
        if (transform.position.x < 25) // Belongs to P1Map
        {
            mapName = "P1Map";
        }
        else // Belongs to P2Map
        {
            mapName = "P2Map";
        }

        map = GameObject.Find(mapName);
        mapGenerator = map.GetComponent<MapGenerator>();
        int zLen = mapGenerator.room.Count;
        int xLen = mapGenerator.room[0].Count;
        int xOff = mapGenerator.XOffset;

        return (zLen, xLen, xOff);
    }

    bool checkCollision(Collider collider)
    {
        if (collider.name.StartsWith("TallBox") 
            || collider.name.StartsWith("Wall") 
            || collider.name.StartsWith("Transparent") 
            || collider.name.StartsWith("MagneticBox")
            || collider.name.StartsWith("Bolt")
            || collider.name.StartsWith("Door")
            || collider.name.StartsWith("Exch"))
        {
            return true;
        }

        return false;
    }

    void CheckCollisions()
    {
        playerPos = Position.None;
        upTileFree = true;
        downTileFree = true;
        leftTileFree = true;
        rightTileFree = true;        
        upTileFreex2 = true;
        downTileFreex2 = true;
        leftTileFreex2 = true;
        rightTileFreex2 = true;

        Collider[] up = Physics.OverlapSphere(transform.position + new Vector3(0, 0, 1), 0.01f);
        Collider[] dw = Physics.OverlapSphere(transform.position + new Vector3(0, 0, -1), 0.01f);
        Collider[] sx = Physics.OverlapSphere(transform.position + new Vector3(-1, 0, 0), 0.01f);
        Collider[] rx = Physics.OverlapSphere(transform.position + new Vector3(1, 0, 0), 0.01f);
        
        Collider[] upx2 = Physics.OverlapSphere(transform.position + new Vector3(0, 0, 2), 0.01f);
        Collider[] dwx2 = Physics.OverlapSphere(transform.position + new Vector3(0, 0, -2), 0.01f);
        Collider[] sxx2 = Physics.OverlapSphere(transform.position + new Vector3(-2, 0, 0), 0.01f);
        Collider[] rxx2 = Physics.OverlapSphere(transform.position + new Vector3(2, 0, 0), 0.01f);

        foreach (Collider collider in up)
        {
            if (collider.name.StartsWith("Player"))
            {
                playerPos = Position.Up;
                playerLogic = collider.GetComponent("PlayerLogic") as PlayerLogic;
            }
            else if (checkCollision(collider))
            {
                upTileFree = false;
            }
        }

        foreach (Collider collider in dw)
        {
            if (collider.name.StartsWith("Player"))
            {
                playerPos = Position.Down;
                playerLogic = collider.GetComponent("PlayerLogic") as PlayerLogic;
            }
            else if (checkCollision(collider))
            {
                downTileFree = false;
            }
        }

        foreach (Collider collider in sx)
        {
            if (collider.name.StartsWith("Player"))
            {
                playerPos = Position.Left;
                playerLogic = collider.GetComponent("PlayerLogic") as PlayerLogic;
            }
            else if (checkCollision(collider))
            {
                leftTileFree = false;
            }
        }

        foreach (Collider collider in rx)
        {
            if (collider.name.StartsWith("Player"))
            {
                playerPos = Position.Right;
                playerLogic = collider.GetComponent("PlayerLogic") as PlayerLogic;
            }
            else if (checkCollision(collider))
            {
                rightTileFree = false;
            }
        }
        
        foreach (Collider collider in upx2)
        {
            if (checkCollision(collider))
            {
                upTileFreex2 = false;
            }
        }

        foreach (Collider collider in dwx2)
        {
            if (checkCollision(collider))
            {
                downTileFreex2 = false;
            }
        }

        foreach (Collider collider in sxx2)
        {
            if (checkCollision(collider))
            {
                leftTileFreex2 = false;
            }
        }

        foreach (Collider collider in rxx2)
        {
            if (checkCollision(collider))
            {
                rightTileFreex2 = false;
            }
        }
    }
}
