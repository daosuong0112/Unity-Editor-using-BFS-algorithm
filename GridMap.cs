using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

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
    public static bool isSaveLevel = false;
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
        } catch (Exception ex) { }
        if (TheMap == null)
        {
            TheMap = new GameObject("Map");
            TheMap.tag = "Map";
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
        currStyle = bStyle.button[1].CellStyle;
    }

    private void SetUpCells()
    {
        cells = new List<List<GridCell>>();
        rMap = new List<List<RealMap>>();
        for (int i = 0; i < 20; i++)
        {
            cells.Add(new List<GridCell>());
            rMap.Add(new List<RealMap>());
            for (int j = 0; j < 10; j++)
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
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
        GUILayout.FlexibleSpace();
        GUILayout.BeginArea(new Rect(position.width-100, position.height-20, 100, 20), EditorStyles.toolbar);
        if (GUILayout.Toggle((!isSaveLevel), "Save", EditorStyles.toolbarButton, GUILayout.Width(100))) {
            isSaveLevel = true;
        }
        GUILayout.EndArea();
    }

    private void ProcessCells(Event e)
    {
        int r = (int)((e.mousePosition.x - offset.x) / edge);
        int c = (int)((e.mousePosition.y - offset.y) / edge);
        if (!((e.mousePosition.x - offset.x) < 0 || (e.mousePosition.x - offset.x) > 20*edge || (e.mousePosition.y - offset.y) < 0 || (e.mousePosition.y - offset.y) > 10*edge))
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
            if (rMap[r][c] == null)
            {
                cells[r][c].SetStyle(currStyle);
                GameObject G = Instantiate(Resources.Load("MapParts/" + currStyle.normal.background.name)) as GameObject;
                G.name = currStyle.normal.background.name;
                G.transform.position = new Vector3(c * 10, 0, r * 10) + Vector3.forward*5 + Vector3.right*5;
                G.transform.parent = TheMap.transform;
                rMap[r][c] = G.GetComponent<RealMap>();
                rMap[r][c].Map = G;
                rMap[r][c].Name = G.name;
                rMap[r][c].Row = r;
                rMap[r][c].Column = c;
                rMap[r][c].Mapstyle = currStyle;
                GUI.changed = true;
            }
        }
    }

    private void DrawCells()
    {
        for (int i = 0; i < 20; i++)
        {
            for (int j = 0; j < 10; j++)
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
        for (int i = 0; i < 20; i++)
        {
            for (int j = 0; j < 10; j++)
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
