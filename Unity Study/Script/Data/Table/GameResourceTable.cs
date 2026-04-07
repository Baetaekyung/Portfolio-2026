using System;
using System.Collections.Generic;
using UnityEngine;

public class GameResourceTable : BaseTable<GameResourceTable.RowData>
{
    [Serializable]
    public class RowData : Row
    {
        public string resourceName;
        public string iconPath;
    }
}
