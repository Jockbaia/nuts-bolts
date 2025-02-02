using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public int XOffset = 0;

    public TextAsset levelFile;

    public Transform tilePrefab;
    public Transform tallboxPrefab;
    public Transform tallboxHeavyPrefab;
    public Transform wallPrefab;
    public Transform wallCornerPrefab;
    public Transform magneticBoxPrefab;
    public Transform boltPrefab;

    public List<List<char>> room;

    void Awake()
    {
        room = ReadLevelFile();
    }

    void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        room = ReadLevelFile();

        string holderName = "Generated Map";
        if (transform.Find(holderName))
        {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        (int zLen, int xLen) = MapLength();

        for (int z = zLen-1; z >= 0; z--)
        {
            for (int x = 0; x < xLen; x++)
            {
                Vector3 tilePosition = new Vector3(x + XOffset, 0, z);

                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;
                newTile.parent = mapHolder;

                if (room[z][x] == '1') // TallBox
                {
                    Vector3 tallboxPosition = new Vector3(x + XOffset, 1, z);
                    Transform newTallbox = Instantiate(tallboxPrefab, tallboxPosition, Quaternion.Euler(Vector3.zero)) as Transform;

                    newTallbox.Rotate(new Vector3(0f, Mathf.Floor(UnityEngine.Random.Range(0, 2)) * 90f, 0f));

                    newTallbox.parent = mapHolder;
                    // Scripts
                    newTallbox.gameObject.AddComponent<BoxLogic>();
                    newTallbox.GetComponent<BoxLogic>().isHeavy = false;
                    // Move point
                    Transform newTallboxMovePoint = new GameObject("Box Move Point").transform;
                    newTallboxMovePoint.transform.position = newTallbox.transform.position;
                    newTallboxMovePoint.parent = newTallbox;
                }
                
                if (room[z][x] == 'h') // TallBoxHeavy
                {
                    Vector3 tallboxPosition = new Vector3(x + XOffset, 1, z);
                    Transform newTallbox = Instantiate(tallboxHeavyPrefab, tallboxPosition, Quaternion.Euler(Vector3.zero)) as Transform;
                    newTallbox.parent = mapHolder;
                    // Scripts
                    newTallbox.gameObject.AddComponent<BoxLogic>();
                    newTallbox.GetComponent<BoxLogic>().isHeavy = true;
                    // Move point
                    Transform newTallboxMovePoint = new GameObject("Box Move Point").transform;
                    newTallboxMovePoint.transform.position = newTallbox.transform.position;
                    newTallboxMovePoint.parent = newTallbox;
                }
                
                if (room[z][x] == 'w') // WallVertical
                {
                    Vector3 position = new Vector3(x + XOffset, 2, z);
                    Transform newObj = Instantiate(wallPrefab, position, Quaternion.Euler(Vector3.zero)) as Transform;
                    newObj.parent = mapHolder;
                    // Scripts
                    //newObj.gameObject.AddComponent<BoxLogic>();
                }                
                
                if (room[z][x] == 'W') // WallCorner
                {
                    Vector3 position = new Vector3(x + XOffset, 2, z);
                    Transform newObj = Instantiate(wallCornerPrefab, position, Quaternion.Euler(Vector3.zero)) as Transform;
                    newObj.parent = mapHolder;
                    // Scripts
                    //newObj.gameObject.AddComponent<BoxLogic>();
                }
                
                if (room[z][x] == '^') // WallHorizontal
                {
                    Vector3 position = new Vector3(x + XOffset, 2, z);
                    Transform newObj = Instantiate(wallPrefab, position, Quaternion.Euler(new Vector3(0, 90, 0))) as Transform;
                    newObj.parent = mapHolder;
                    // Scripts
                    //newObj.gameObject.AddComponent<BoxLogic>();
                }
                
                if (room[z][x] == 'm') // MagneticBox
                {
                    Vector3 position = new Vector3(x + XOffset, 0.5f, z);
                    Transform newObj = Instantiate(magneticBoxPrefab, position, Quaternion.Euler(Vector3.zero)) as Transform;
                    newObj.parent = mapHolder;
                }
                
                if (room[z][x] == 'b') // Bolt
                {
                    Vector3 position = new Vector3(x + XOffset, 0f, z);
                    Transform newObj = Instantiate(boltPrefab, position, Quaternion.Euler(Vector3.zero)) as Transform;
                    newObj.parent = mapHolder;
                }
            }
        }
    }

    public (int, int) MapLength()
    {
        // zLen, xLen
        return (room.Count, room[0].Count);
    }

    List<List<char>> ReadLevelFile()
    {
        List<List<char>> room = new List<List<char>>();

        string[] rows = levelFile.text.Split('\n');

        foreach (string row in rows)
        {
            string[] cols = row.Split(',');
            List<char> tmp = new List<char>();
            foreach (string c in cols)
            {
                tmp.Add(c.ToCharArray()[0]);
            }
            room.Add(tmp);
        }
        room.Reverse();
        return room;
    }
}
