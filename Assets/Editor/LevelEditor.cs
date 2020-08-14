using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelCreator))]
[CanEditMultipleObjects]
public class LevelEditor : Editor
{
    GameObject borderWallPrefab;
    GameObject nonDestroyableWallPrefab;
    GameObject destroyableWallPrefab;

    List<string> destroyableWallPositions;

    bool scriptActive;

    LevelCreator create;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        create = (LevelCreator)target;

        //Saving prefab references locally
        borderWallPrefab = create.outerWall;
        nonDestroyableWallPrefab = create.nonDestroyableWall;
        destroyableWallPrefab = create.destroyableWall;

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
            create.destroyableWallHolder.transform
        };

        foreach (Transform t in wallHolders)
        {
            DestroyAllChild(t);
        }
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

