using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

public class GridMap : EditorWindow
{
    Vector2 offset;
    Vector2 drag;
    Vector2 cellPos;
    List<List<GridCell>> cells;
    List<List<RealMap>> rMap;
    GUIStyle empty;
    Style bStyle;
    GUIStyle currStyle;
    GameObject TheMap;
    Rect MenuBar;
    int edge = 20;
    bool isErasing;
    int width = 20;
    int height = 20;


    [MenuItem("Demo/Map")]
    private static void OpenWindow()
    {
        GridMap window = GetWindow<GridMap>();
        window.titleContent = new GUIContent("Demo Map Task 2");
    }

    private void OnEnable()
    {
        SetUpStyles();
        SetUpCells();
        SetUpMap();
    }  

    private void SetUpMap()
    {
        try
        {
            TheMap = GameObject.FindGameObjectWithTag("Map");
            RestoreMap(TheMap);
        }
        catch (Exception ex) { }
        if (TheMap == null)
        {
            TheMap = new GameObject("Map");
            TheMap.tag = "Map";
            GameObject Ground = new GameObject("Ground");
            TheMap.AddComponent(typeof(SearchPath));
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    GameObject G = Instantiate(Resources.Load("MapParts/" + currStyle.normal.background.name)) as GameObject;
                    G.name = currStyle.normal.background.name;
                    G.transform.position = new Vector3(j * 10, -4.9f, i * 10) + Vector3.forward * 5 + Vector3.right * 5;
                    G.transform.parent = Ground.transform;
                    G.tag = "Parts";
                }
            }
            
        }
    }

    private void RestoreMap(GameObject theMap)
    {
        if (theMap.transform.childCount > 0)
        {
            for (int i = 0; i < theMap.transform.childCount; i++)
            {
                RealMap temp = theMap.transform.GetChild(i).GetComponent<RealMap>();
                int r = temp.Row;
                int c = temp.Column;
                GUIStyle TheStyle = temp.Mapstyle;
                cells[r][c].SetStyle(TheStyle);
                rMap[r][c] = temp;
                rMap[r][c].Map = theMap.transform.GetChild(i).gameObject;
                rMap[r][c].Name = theMap.transform.GetChild(i).name;
                rMap[r][c].Row = r;
                rMap[r][c].Column = c;
            }
            
        }
    }

    private void SetUpStyles()
    {
        try
        {
            bStyle = GameObject.FindGameObjectWithTag("StyleManager").GetComponent<Style>();
            for (int i = 0; i < bStyle.button.Length; i++)
            {
                bStyle.button[i].CellStyle = new GUIStyle();
                bStyle.button[i].CellStyle.normal.background = bStyle.button[i].Icon;
            }
        } catch (Exception ex) { }

        empty = bStyle.button[0].CellStyle;
        currStyle = bStyle.button[0].CellStyle;
    }

    private void SetUpCells()
    {
        cells = new List<List<GridCell>>();
        rMap = new List<List<RealMap>>();
        for (int i = 0; i < width; i++)
        {
            cells.Add(new List<GridCell>());
            rMap.Add(new List<RealMap>());
            for (int j = 0; j < height; j++)
            {
                cellPos.Set(i * edge, j * edge);
                cells[i].Add(new GridCell(cellPos, edge, edge, empty));
                rMap[i].Add(null);
            }
        }
    }

    private void OnGUI()
    {
        DrawGrid();
        DrawCells();
        DrawMenuBar();
        ProcessCells(Event.current);
        ProcessGrid(Event.current);
        if (GUI.changed)
        {
            // Update the window
            Repaint();
        }
    }

    private void SplitView()
    {
        
    }

    private void DrawMenuBar()
    {
        MenuBar = new Rect(0, 0, position.width, 20);
        GUILayout.BeginArea(MenuBar, EditorStyles.toolbar);
        GUILayout.BeginHorizontal();
        for (int i = 0; i < bStyle.button.Length; i++)
        {
            if (GUILayout.Toggle((currStyle == bStyle.button[i].CellStyle), new GUIContent(bStyle.button[i].ButtonText), EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                currStyle = bStyle.button[i].CellStyle;
            }
        }
        
        if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(80)))
        {
            SaveLevel();
        }
        if (GUILayout.Button("Load", EditorStyles.toolbarButton, GUILayout.Width(80)))
        {
            LoadLevel();
        }
        if (GUILayout.Button("Random", EditorStyles.toolbarButton, GUILayout.Width(80)))
        {
            RandLevel();
        }
        if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(80)))
        {
            ClearLevel();
        }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
        Repaint();
    }

    private void ClearLevel()
    {
        DestroyImmediate(TheMap.gameObject);
        DestroyImmediate(GameObject.Find("Ground"));
        rMap.Clear();
        cells.Clear();
        SetUpCells();
        currStyle = bStyle.button[0].CellStyle;
        SetUpMap();
    }

    private void RandLevel()
    {
        int rnd = UnityEngine.Random.Range(1, width*height-(width+height));
        int row = UnityEngine.Random.Range(0, width);
        int col = UnityEngine.Random.Range(0, height);
        int style;
        Vector2Int temp = new Vector2Int(row, col);
        List<Vector2Int> IntLst = new List<Vector2Int>();
        for (int i = 0; i < rnd; i++)
        {
            while (IntLst.Contains(temp))
            {
                temp.x = UnityEngine.Random.Range(0, width);
                temp.y = UnityEngine.Random.Range(0, height);
            }
            IntLst.Add(temp);
            style = UnityEngine.Random.Range(1, 4);
            currStyle = bStyle.button[style].CellStyle;
            PaintCells(temp.x, temp.y);
        }
        for (int i = 0; i < 2; i++)
        {
            while (IntLst.Contains(temp))
            {
                temp.x = UnityEngine.Random.Range(0, width);
                temp.y = UnityEngine.Random.Range(0, height);
            }
            IntLst.Add(temp);
            currStyle = bStyle.button[i+5].CellStyle;
            PaintCells(temp.x, temp.y);
        }
    }

    private void SaveLevel()
    {
        int len = TheMap.transform.childCount;
        List<MapData> data = new List<MapData>();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (rMap[i][j] != null)
                {
                    data.Add(new MapData(i, j, rMap[i][j].Name));
                }
            }
        }
        string outputString = JsonHelper.ToJson(data.ToArray());
        Debug.Log(outputString);
        File.WriteAllText(Application.dataPath + "/Data/data.json", outputString);

    }

    private void LoadLevel()
    {
        string inputString = File.ReadAllText(Application.dataPath + "/Data/data.json");
        MapData[] data = JsonHelper.FromJson<MapData>(inputString);
        for (int i = 0; i < data.Length; i++)
        {
            MapData eachData = data[i];
            cells[eachData.row][eachData.col].SetStyle(bStyle.button[eachData.style].CellStyle);
            GameObject G = Instantiate(Resources.Load("MapParts/" + eachData.Cname)) as GameObject;
            G.name = eachData.Cname;
            G.transform.position = new Vector3(eachData.col * 10, 0, eachData.row * 10) + Vector3.forward * 5 + Vector3.right * 5;
            G.transform.parent = TheMap.transform;
            G.tag = "Parts";
            rMap[eachData.row][eachData.col] = G.GetComponent<RealMap>();
            rMap[eachData.row][eachData.col].Map = G;
            rMap[eachData.row][eachData.col].Name = G.name;
            rMap[eachData.row][eachData.col].Row = eachData.row;
            rMap[eachData.row][eachData.col].Column = eachData.col;
            rMap[eachData.row][eachData.col].Mapstyle = bStyle.button[eachData.style].CellStyle;



            Collider[] intersecting = Physics.OverlapSphere(new Vector3(G.transform.position.x, -4.9f, G.transform.position.z), 0.01f);
            if (intersecting.Length != 0)
            {
                if (G.name == "Player")
                {
                    TheMap.GetComponent<SearchPath>()._startingPoint = intersecting[0].gameObject.GetComponent<Node>();
                }
                else if (G.name == "Cactus")
                {
                    TheMap.GetComponent<SearchPath>()._endingPoint = intersecting[0].gameObject.GetComponent<Node>();
                }
                else
                {
                    GameObject G1 = Instantiate(Resources.Load("MapParts/Empty_1")) as GameObject;
                    G1.name = "Empty_1";
                    G1.transform.position = new Vector3(eachData.col * 10, -4.9f, eachData.row * 10) + Vector3.forward * 5 + Vector3.right * 5;
                    G1.transform.parent = TheMap.transform;
                    G1.tag = "Parts";

                    DestroyImmediate(intersecting[0].gameObject);
                }

            }

            GUI.changed = true;
        }
    }

    private void ProcessCells(Event e)
    {
        int r = (int)((e.mousePosition.x - offset.x) / edge);
        int c = (int)((e.mousePosition.y - offset.y) / edge);
        if (!((e.mousePosition.x - offset.x) < 0 || (e.mousePosition.x - offset.x) > width*edge || (e.mousePosition.y - offset.y) < 0 || (e.mousePosition.y - offset.y) > height*edge))
        {
            if (e.type == EventType.MouseDown)
            {
                if (cells[r][c].style.normal.background.name == "Empty")
                {
                    isErasing = false;
                }
                else
                {
                    isErasing = true;
                }
                PaintCells(r, c);
            }
            if (e.type == EventType.MouseDrag)
            {
                PaintCells(r, c);
                e.Use();
            }
        }
    }

    private void PaintCells(int r, int c)
    {
        if (isErasing)
        {
            if (rMap[r][c] != null)
            {
                cells[r][c].SetStyle(empty);
                DestroyImmediate(rMap[r][c].gameObject);
                GUI.changed = true;
            }
            rMap[r][c] = null;
        }
        else
        {
            if (rMap[r][c] == null && currStyle.normal.background.name != "Empty")
            {
                cells[r][c].SetStyle(currStyle);
                GameObject G = Instantiate(Resources.Load("MapParts/" + currStyle.normal.background.name)) as GameObject;
                G.name = currStyle.normal.background.name;
                G.transform.position = new Vector3(c * 10, 0, r * 10) + Vector3.forward*5 + Vector3.right*5;
                G.transform.parent = TheMap.transform;
                G.tag = "Parts";
                rMap[r][c] = G.GetComponent<RealMap>();
                rMap[r][c].Map = G;
                rMap[r][c].Name = G.name;
                rMap[r][c].Row = r;
                rMap[r][c].Column = c;
                rMap[r][c].Mapstyle = currStyle;
                


                Collider[] intersecting = Physics.OverlapSphere(new Vector3(G.transform.position.x, -4.9f, G.transform.position.z), 0.01f);
                if (intersecting.Length != 0)
                {
                    if (G.name == "Player")
                    {
                        TheMap.GetComponent<SearchPath>()._startingPoint = intersecting[0].gameObject.GetComponent<Node>();
                    }
                    else if (G.name == "Cactus")
                    {
                        TheMap.GetComponent<SearchPath>()._endingPoint = intersecting[0].gameObject.GetComponent<Node>();
                    } else
                    {
                        GameObject G1 = Instantiate(Resources.Load("MapParts/Empty_1")) as GameObject;
                        G1.name = "Empty_1";
                        G1.transform.position = new Vector3(c * 10, -4.9f, r * 10) + Vector3.forward * 5 + Vector3.right * 5;
                        G1.transform.parent = TheMap.transform;
                        G1.tag = "Parts";

                        DestroyImmediate(intersecting[0].gameObject);
                    }
                    
                }

                

                GUI.changed = true;
            }
        }
    }

    private void DrawCells()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                cells[i][j].Draw();
            }
        }
    }

    private void ProcessGrid(Event e)
    {
        drag = Vector2.zero;
        switch (e.type)
        {
            case EventType.MouseDrag:
                if (e.button == 0)
                {
                    OnMouseDrag(e.delta);
                }
                break;
        }
    }

    private void OnMouseDrag(Vector2 delta)
    {
        drag = delta;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                cells[i][j].Drag(delta);
            }
        }
        GUI.changed = true;
    }

    private void DrawGrid()
    {
        int width = Mathf.CeilToInt(position.width / 20);
        int height = Mathf.CeilToInt(position.height / 20);
        Handles.BeginGUI();
        Handles.color = new Color(0.75f, 0.75f, 0.75f, 0.25f);
        offset += drag;
        Vector3 newOffset = new Vector3(offset.x % 20, offset.y % 20, 0);
        for (int i = 0; i < width; i++)
        {
            Handles.DrawLine(new Vector3(20 * i, -20, 0) + newOffset, new Vector3(20 * i, position.height, 0) + newOffset);
        }
        for (int i = 0; i < height; i++)
        {
            Handles.DrawLine(new Vector3(-20, 20 * i, 0) + newOffset, new Vector3(position.width, 20 * i, 0) + newOffset);
        }
        Handles.color = Color.white;
        Handles.EndGUI();
    }

}
