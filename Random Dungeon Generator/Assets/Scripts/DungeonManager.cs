using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum DungeonType { Caverns, Rooms, Winding }

public class DungeonManager : MonoBehaviour
{
    public GameObject[] randomItems, randomEnemies;
    public GameObject floorPrefab, wallPrefab, tilePrefab, exitPrefab;
    public int totalFloorCount;
    [Range(0,100)] public int itemSpawnPercent;
    [Range(0,100)] public int enemySpawnPercent;
    //public bool useRoundedEdges;
    public DungeonType dungeonType;
    [Range(0,100)] public int windingHallPercent;
    

   [HideInInspector] public float minX, maxX, minY, maxY;

   List<Vector3> floorList = new List<Vector3>();
   LayerMask floorMask, wallMask;

   void Start()
   {
       floorMask = LayerMask.GetMask("Floor");
       wallMask = LayerMask.GetMask("Wall");
       switch(dungeonType)
       {
           case DungeonType.Caverns: RandomWalker(); break;
           case DungeonType.Rooms: RoomWalker(); break;
           case DungeonType.Winding: WindingWalker(); break;

       }
       RandomWalker();
   }
   void Update()
   {
       if(Application.isEditor && Input.GetKeyDown(KeyCode.Backspace))
       {
           SceneManager.LoadScene(SceneManager.GetActiveScene().name);
       }
   }

   void RandomWalker()
   {
       Vector3 curPos = Vector3.zero;
       floorList.Add(curPos);
       while(floorList.Count < totalFloorCount)
       {
            curPos += RandomDirection();
 
            if(!InFloorList(curPos))
                {
                    floorList.Add(curPos);
                }
       }


        StartCoroutine(DelayProgress());
   }

    void RoomWalker()
    {
        Vector3 curPos = Vector3.zero;
       floorList.Add(curPos);
       while(floorList.Count < totalFloorCount)
       {
           Vector3 walkDir = RandomDirection();
           int walkLength = Random.Range(9, 18);

            for(int i=0; i< walkLength; i++)
            {
                if(!InFloorList(curPos + walkDir))
                    {
                        floorList.Add(curPos + walkDir);
                    }
                curPos += walkDir;
            }
            RandomRoom(curPos);
       }

        StartCoroutine(DelayProgress());
    }


//    void RandomWalker()
//    {
//        Vector3 curPos = Vector3.zero;
//        floorList.Add(curPos);
//        while(floorList.Count < totalFloorCount)
//        {
//             curPos += RandomDirection();
 
//             if(!InFloorList(curPos))
//                 {
//                     floorList.Add(curPos);
//                 }
//        }


//         StartCoroutine(DelayProgress());
//    }

    void WindingWalker()
    {
        Vector3 curPos = Vector3.zero;
       floorList.Add(curPos);
       while(floorList.Count < totalFloorCount)
       {
            curPos = takeAHike(curPos);
            int roll = Random.Range(0, 101);
            if(roll > windingHallPercent){
                RandomRoom(curPos);
            }
            
       }

        StartCoroutine(DelayProgress());
    }

Vector3 takeAHike(Vector3 myPos )
{
               Vector3 walkDir = RandomDirection();
           int walkLength = Random.Range(9, 18);

            for(int i=0; i< walkLength; i++)
            {
                if(!InFloorList(myPos + walkDir))
                    {
                        floorList.Add(myPos+ walkDir);
                    }
                myPos += walkDir;
            }
    return myPos;
}
 void RandomRoom(Vector3 myPos)
 {
                 //random room at end of long wlak
            int width = Random.Range(1,5);
            int height = Random.Range(1, 5);
            for(int w = -width; w<= width; w++)
            {
                for(int h = -height; h <= height; h++)
                {
                 Vector3 offSet = new Vector3(w, h, 0);
                 if(!InFloorList(myPos + offSet))
                    {
                        floorList.Add(myPos + offSet);
                    }   
                }
            }
 }


    bool InFloorList(Vector3 myPos)
    {
     for(int i = 0; i<floorList.Count; i++)
        {
            if(Vector3.Equals(myPos, floorList[i]))
            {
                return true;
            }
        }  
        return false;
    }

    Vector3 RandomDirection()
    {
         switch(Random.Range(1, 5))
                {
                    case 1:
                        return Vector3.up;
                    case 2:
                        return Vector3.right;
                       case 3:
                        return Vector3.down;
                    case 4:
                        return Vector3.left;
                }
                return Vector3.zero;
    }

   IEnumerator DelayProgress()
   {
        for(int i = 0; i<floorList.Count; i++)
       {
           GameObject goTile = Instantiate(tilePrefab, floorList[i], Quaternion.identity) as GameObject;
           goTile.name = tilePrefab.name;
           goTile.transform.SetParent(transform);
       }

       while(FindObjectsOfType<TileSpawner>().Length >0)
       {
           yield return null;
       }
       ExiltDoorway();  
       Vector2 hitSize = Vector2.one * 0.8f;
       for(int x = (int)minX - 2; x  <= (int)maxX + 2; x++)
       {
           for(int y = (int)minY - 2; y  <= (int)maxY + 2; y++)
           {
               Collider2D hitFloor = Physics2D.OverlapBox(new Vector2(x, y), hitSize, 0, floorMask);
               if(hitFloor)
               {
                   if(!Vector2.Equals(hitFloor.transform.position, floorList[floorList.Count -1]))
                   {
                       Collider2D hitTop = Physics2D.OverlapBox(new Vector2(x, y + 1), hitSize, 0, wallMask);
                       Collider2D hitRight = Physics2D.OverlapBox(new Vector2(x + 1, y), hitSize, 0, wallMask);
                       Collider2D hitBottom = Physics2D.OverlapBox(new Vector2(x, y - 1), hitSize, 0, wallMask);
                       Collider2D hitLeft = Physics2D.OverlapBox(new Vector2(x - 1, y), hitSize, 0, wallMask);
                       RandomItems(hitFloor, hitTop, hitRight, hitBottom, hitLeft);
                       RandomEnemies(hitFloor, hitTop, hitRight, hitBottom, hitLeft);
                   }
               }
           }
       }
   }

    void RandomEnemies(Collider2D hitFloor, Collider2D hitTop, Collider2D hitRight, Collider2D hitBottom, Collider2D hitLeft)
    {
        if (hitFloor.transform.position == Vector3.zero) { return; };
        if(!hitTop && !hitRight && !hitBottom && !hitLeft)
        {
            int roll = Random.Range(1,101);
            if(roll <= enemySpawnPercent)
            {
                int enemyIndex = Random.Range(0, randomEnemies.Length);
                GameObject goEnemy = Instantiate(randomEnemies[enemyIndex], hitFloor.transform.position, Quaternion.identity) as GameObject;
                goEnemy.name = randomEnemies[enemyIndex].name;
                goEnemy.transform.SetParent(hitFloor.transform);
            }
        }
    }

    void RandomItems(Collider2D hitFloor, Collider2D hitTop, Collider2D hitRight, Collider2D hitBottom, Collider2D hitLeft)
    {
        if((hitTop || hitRight || hitBottom || hitLeft) && !(hitTop && hitBottom) && !(hitLeft && hitRight))
        {
            int roll = Random.Range(1,101);
            if(roll <= itemSpawnPercent)
            {
                int itemIndex = Random.Range(0, randomItems.Length);
                 GameObject goItem = Instantiate(randomItems[itemIndex], hitFloor.transform.position, Quaternion.identity) as GameObject;
                goItem.name = randomItems[itemIndex].name;
                 goItem.transform.SetParent(hitFloor.transform);
            }
        }
    }

   void ExiltDoorway()
   {
        Vector3 doorPos = floorList[floorList.Count -1];
        GameObject goDoor = Instantiate(exitPrefab, doorPos, Quaternion.identity) as GameObject;
        goDoor.name = exitPrefab.name;
        goDoor.transform.SetParent(transform);
   }
}
