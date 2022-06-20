using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapData
{
    public int row, col, style;
    public string Cname;

    public MapData(int row, int col, string Cname)
    {
        this.Cname = Cname;
        this.row = row;
        this.col = col;
        if (Cname == "Tree") this.style = 1;
        else if (Cname == "Lamb") this.style = 2;
        else if (Cname == "Cow") this.style = 3;
        else if (Cname == "Chicken") this.style = 4;
        else if (Cname == "Player") this.style = 5;
        else this.style = 6;
    } 
}
