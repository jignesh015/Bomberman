using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Diagnostics.Eventing.Reader;
using Microsoft.Win32;

[CustomEditor(typeof(LevelCreator))]
[CanEditMultipleObjects]
public class LevelEditor : Editor
{
    GameObject borderWallPrefab, nonDestroyableWallPrefab, destroyableWallPrefab;
    List<GameObject> enemyPrefabs;
    List<int> noOfEachEnemies;

    List<string> destroyableWallPositions, enemyPositions;

    bool scriptActive;

    LevelCreator create;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        create = (LevelCreator)target;

        //Saving wall prefab references locally
        borderWallPrefab = create.outerWall;
        nonDestroyableWallPrefab = create.nonDestroyableWall;
        destroyableWallPrefab = create.destroyableWall;

        //Saving enemy prefab references locally
        enemyPrefabs = create.enemies;
        noOfEachEnemies = create.noOfEachEnemies;

        #region "Border creation/deletion"
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Create Border")) {
            if (!scriptActive) {
                BuildBorder();
            }
        }

        if (GUILayout.Button("Destroy Border"))
        {
            if (!scriptActive)
            {
                DestroyAllChild(create.outerWallHolder.transform);
            }
        }
        EditorGUILayout.EndHorizontal();
        #endregion

        #region "Non-Destroyable walls creation/deletion"
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Create Non-Destroyable Walls"))
        {
            if (!scriptActive)
            {
                CreateNonDestroyableWalls();
            }
        }

        if (GUILayout.Button("Destroy Non-Destroyable Walls"))
        {
            if (!scriptActive)
            {
                DestroyAllChild(create.nonDestroyableWallHolder.transform);
            }
        }
        EditorGUILayout.EndHorizontal();
        #endregion

        #region "Destroyable walls creation/deletion"
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Create Destroyable Walls"))
        {
            if (!scriptActive)
            {
                CreateDestroyableWalls();
            }
        }

        if (GUILayout.Button("Destroy Destroyable Walls"))
        {
            if (!scriptActive)
            {
                DestroyAllChild(create.destroyableWallHolder.transform);
            }
        }
        EditorGUILayout.EndHorizontal();
        #endregion

        #region "Entire level creation/deletion"
        if (GUILayout.Button("Create Entire Level"))
        {
            if (!scriptActive)
            {
                CreateEntireLevel();
            }
        }
        if (GUILayout.Button("Destroy Entire Level"))
        {
            if (!scriptActive)
            {
                DestroyEntireLevel();
            }
        }

        #endregion

        EditorGUILayout.Space(20);

        #region "Enemy placement"
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Place Enemies"))
        {
            if (!scriptActive) PlaceEnemies();
        }
        if (GUILayout.Button("Destroy Enemies"))
        {
            if (!scriptActive) DestroyAllChild(create.enemyHolder.transform);
        }
        EditorGUILayout.EndHorizontal();
        #endregion

        EditorGUILayout.Space(20);

        #region "Player placement"
        if (GUILayout.Button("Place Player"))
        {
            if (!scriptActive) PlacePlayer();
        }
        #endregion

    }

    //Check if grid size is valid
    bool IsGridSizeValid()
    {
        if (create.gridSizeX < 5 || create.gridSizeZ < 5)
        {
            Debug.LogError("Grid size must be greater than or equal to 5");
            return false;
        }
        if (create.gridSizeX % 2 == 0 || create.gridSizeZ % 2 == 0)
        {
            Debug.LogError("Grid size must me an odd integer");
            return false;
        }

        return true;
    }

    //Builds the border as per the grid size
    void BuildBorder()
    {
        if (!IsGridSizeValid())
        {
            return;
        }

        scriptActive = true;
        DestroyAllChild(create.outerWallHolder.transform);
        for (int i = 0; i < create.gridSizeX; i++)
        {
            for (int j = 0; j < create.gridSizeZ; j++)
            {
                if (i == 0 || i == create.gridSizeX - 1 || j == 0 || j == create.gridSizeZ - 1)
                {
                    GameObject borderWall = (GameObject)PrefabUtility.InstantiatePrefab(borderWallPrefab);
                    borderWall.transform.position = new Vector3(create.startPos.x + i + create.offset.x,
                        create.startPos.y + create.offset.y, create.startPos.z + j + create.offset.z);
                    borderWall.transform.parent = create.outerWallHolder.transform;
                }
            }
        }
        ResizeGroundPlane();
        scriptActive = false;
    }

    //Resizes the ground plane based on the grid size
    void ResizeGroundPlane() {
        Vector3 scaler = new Vector3((float)create.gridSizeX / 10, 1, (float)create.gridSizeZ / 10);

        //RESIZE
        create.groundPlane.transform.localScale = scaler;

        //SET POSITION
        create.groundPlane.transform.position = new Vector3((float)create.gridSizeX / 2, 0, (float)create.gridSizeZ / 2);

        //SET MATERIAL TILING
        create.groundPlane.GetComponent<MeshRenderer>().sharedMaterial.mainTextureScale = new Vector2(
            create.gridSizeX, create.gridSizeZ);
    }

    //Creates inner indestructible walls
    void CreateNonDestroyableWalls()
    {
        if (!IsGridSizeValid())
        {
            return;
        }

        scriptActive = true;
        DestroyAllChild(create.nonDestroyableWallHolder.transform);
        for (int i = 2; i < create.gridSizeX - 2; i++)
        {
            for (int j = 2; j < create.gridSizeZ - 2; j++)
            {
                if (i % 2 == 0 && j % 2 == 0)
                {
                    GameObject nonDestroyableWall = (GameObject)PrefabUtility.InstantiatePrefab(nonDestroyableWallPrefab);
                    nonDestroyableWall.transform.position = new Vector3(create.startPos.x + i + create.offset.x,
                        create.startPos.y + create.offset.y, create.startPos.z + j + create.offset.z);
                    nonDestroyableWall.transform.parent = create.nonDestroyableWallHolder.transform;
                }
            }
        }
        scriptActive = false;
    }

    //Creates inner destructible walls
    void CreateDestroyableWalls()
    {
        if (!IsGridSizeValid())
        {
            return;
        }

        //Calculate valid possible number of destroyable walls
        int x = ((create.gridSizeX - 5) / 2) + 1;
        int z = ((create.gridSizeZ - 5) / 2) + 1;
        int possibleAmt = ((create.gridSizeX - 2) * (create.gridSizeZ - 2)) - (x * z);

        if (create.noOfDestroyableWalls >= possibleAmt)
        {
            Debug.LogError("Possible count for Destroyable walls is " + (possibleAmt - 1).ToString());
            return;
        }

        destroyableWallPositions = new List<string>();

        scriptActive = true;
        DestroyAllChild(create.destroyableWallHolder.transform);
        DestroyAllChild(create.enemyHolder.transform);
        DestroyImmediate(GameObject.FindGameObjectWithTag("Player"));

        for (int a = 0; a < create.noOfDestroyableWalls; a++)
        {
            int i = Random.Range(1, create.gridSizeX - 1);
            int j = Random.Range(1, create.gridSizeZ - 1);

            while (i % 2 == 0 && j % 2 == 0)
            {
                i = Random.Range(1, create.gridSizeX - 1);
                j = Random.Range(1, create.gridSizeZ - 1);
            }

            if (!(i % 2 == 0 && j % 2 == 0))
            {
                float posX = create.startPos.x + i + create.offset.x;
                float posY = create.startPos.y + create.offset.y;
                float posZ = create.startPos.z + j + create.offset.z;

                if (!destroyableWallPositions.Contains("X" + posX.ToString() + "Z" + posZ.ToString()))
                {
                    GameObject destroyableWall = (GameObject)PrefabUtility.InstantiatePrefab(destroyableWallPrefab);
                    destroyableWall.transform.position = new Vector3(posX, posY, posZ);
                    destroyableWall.transform.parent = create.destroyableWallHolder.transform;
                    destroyableWallPositions.Add("X" + posX.ToString() + "Z" + posZ.ToString());
                }
                
            }
        }
        scriptActive = false;

    }

    //Creates entire level rather than individually
    void CreateEntireLevel()
    {
        if (!IsGridSizeValid())
        {
            return;
        }

        BuildBorder();

        CreateNonDestroyableWalls();

        CreateDestroyableWalls();
    }

    //Destroys entire level rather than individually
    void DestroyEntireLevel()
    {
        Transform[] wallHolders = new Transform[] 
        {
            create.outerWallHolder.transform,
            create.nonDestroyableWallHolder.transform,
            create.destroyableWallHolder.transform,
            create.enemyHolder.transform
        };

        foreach (Transform t in wallHolders)
        {
            DestroyAllChild(t);
        }
        DestroyImmediate(GameObject.FindGameObjectWithTag("Player"));
    }

    //Destroy all childs of the given parent element
    void DestroyAllChild(Transform parentObj)
    {
        int childCount = parentObj.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(parentObj.GetChild(i).gameObject);
        }
    }

    List<Vector3> GetEmptyCells()
    {
        List<Vector3> _emptyCells = new List<Vector3>();

        //Get destroyable walls positions to avoid overlap
        Transform _destroyableWallHolder = create.destroyableWallHolder.transform;
        int _destroyableWallCount = _destroyableWallHolder.childCount;
        destroyableWallPositions = new List<string>();
        for (int _child = 0; _child < _destroyableWallCount; _child++)
        {
            destroyableWallPositions.Add("X" + _destroyableWallHolder.GetChild(_child).position.x.ToString()
                + "Z" + _destroyableWallHolder.GetChild(_child).position.z.ToString());
        }

        //Get enemy positions
        Transform _enemyHolder = create.enemyHolder.transform;
        int _enemyCount = _enemyHolder.childCount;
        enemyPositions = new List<string>();
        for (int _child = 0; _child < _enemyCount; _child++)
        {
            enemyPositions.Add("X" + _enemyHolder.GetChild(_child).position.x.ToString()
                + "Z" + _enemyHolder.GetChild(_child).position.z.ToString());
        }

        //Find possible positions for placing enemies
        for (int i = 1; i < create.gridSizeX - 1; i++)
        {
            for (int j = 1; j < create.gridSizeZ - 1; j++)
            {
                if (!(i % 2 == 0 && j % 2 == 0))
                {
                    float posX = create.startPos.x + i + create.enemyOffset.x;
                    float posY = create.startPos.y;
                    float posZ = create.startPos.z + j + create.enemyOffset.z;
                    if (!destroyableWallPositions.Contains("X" + posX.ToString() + "Z" + posZ.ToString()))
                    {
                        _emptyCells.Add(new Vector3(posX, posY, posZ));

                    }
                }
            }
        }

        return _emptyCells;
    }

    //Place enemies on empty cells as per the input parameters
    void PlaceEnemies()
    {
        List<Vector3> _possibleEnemyPosition = GetEmptyCells();

        if (destroyableWallPositions.Count == 0 || enemyPrefabs.Count == 0 || noOfEachEnemies.Count == 0)
        {
            Debug.LogError("Create valid number of destroyable walls || Mention valid number of enemies");
            return;
        }

        scriptActive = true;
        DestroyAllChild(create.enemyHolder.transform);
        DestroyImmediate(GameObject.FindGameObjectWithTag("Player"));

        //Choose random position to place enemy
        for (int _enemyIndex = 0; _enemyIndex < enemyPrefabs.Count; _enemyIndex++)
        {
            for (int _enemyCount = 0; _enemyCount < noOfEachEnemies[_enemyIndex]; _enemyCount++)
            {
                int _posIndex = Random.Range(0, _possibleEnemyPosition.Count);
                bool isValidPos = false;
                int validityCounter = 0;

                while (!isValidPos)
                {
                    validityCounter++;
                    _posIndex = Random.Range(0, _possibleEnemyPosition.Count);
                    if (!enemyPositions.Contains("X" + _possibleEnemyPosition[_posIndex].x.ToString()
                        + "Z" + _possibleEnemyPosition[_posIndex].z.ToString())) isValidPos = true;

                    if (validityCounter > _possibleEnemyPosition.Count) break;
                }

                if (!isValidPos)
                {
                    Debug.LogError("Not able to find a valid position for enemy");
                    scriptActive = false;
                    return;
                }

                Vector3 _validEnemyPos = _possibleEnemyPosition[_posIndex];
                GameObject _enemy = (GameObject)PrefabUtility.InstantiatePrefab(enemyPrefabs[_enemyIndex]);
                _enemy.transform.position = new Vector3(_validEnemyPos.x, _validEnemyPos.y + create.enemyOffset.y, _validEnemyPos.z);
                _enemy.transform.parent = create.enemyHolder.transform;
                enemyPositions.Add("X" + _validEnemyPos.x.ToString() + "Z" + _validEnemyPos.z.ToString());
            }
        }

        scriptActive = false;
    }

    void PlacePlayer()
    {
        DestroyImmediate(GameObject.FindGameObjectWithTag("Player"));

        List<Vector3> _positions = GetEmptyCells();
        int p = Random.Range(0, _positions.Count);
        GameObject _player = (GameObject)PrefabUtility.InstantiatePrefab(create.player);
        _player.transform.position = new Vector3(_positions[p].x, _positions[p].y + create.playerOffset.y, _positions[p].z);
    }
}

public class ShowErrorPopup : EditorWindow
{
    public void Init()
    {

        ShowErrorPopup window = ScriptableObject.CreateInstance<ShowErrorPopup>();
        window.position = new Rect(Screen.width / 2, Screen.height / 2,500, 150);
        window.ShowPopup();
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Hurray", EditorStyles.wordWrappedLabel);
        GUILayout.Space(70);
        if (GUILayout.Button("OKAY!")) this.Close();
    }
}

