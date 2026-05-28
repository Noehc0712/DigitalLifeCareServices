using UnityEngine;

public class MenuSpawner : MonoBehaviour
{
    public GameObject[] menuPrefabs;   // 1~8번 메뉴 프리팹
    public Transform[] spawnPoints;    // 1~8번 생성 위치

    private GameObject[] spawnedObjects;

    void Start()
    {
        spawnedObjects = new GameObject[menuPrefabs.Length];
    }

    void Update()
    {
        CheckSpawnKey(KeyCode.Alpha1, 0);
        CheckSpawnKey(KeyCode.Alpha2, 1);
        CheckSpawnKey(KeyCode.Alpha3, 2);
        CheckSpawnKey(KeyCode.Alpha4, 3);
        CheckSpawnKey(KeyCode.Alpha5, 4);
        CheckSpawnKey(KeyCode.Alpha6, 5);
        CheckSpawnKey(KeyCode.Alpha7, 6);
        CheckSpawnKey(KeyCode.Alpha8, 7);
    }

    void CheckSpawnKey(KeyCode key, int index)
    {
        if (Input.GetKeyDown(key))
        {
            SpawnMenu(index);
        }
    }

    void SpawnMenu(int index)
    {
        if (index >= menuPrefabs.Length || index >= spawnPoints.Length)
            return;

        // 이미 생성된 오브젝트가 있으면 삭제 후 다시 생성
        if (spawnedObjects[index] != null)
        {
            Destroy(spawnedObjects[index]);
        }

        spawnedObjects[index] = Instantiate(
            menuPrefabs[index],
            spawnPoints[index].position,
            spawnPoints[index].rotation
        );
    }
}