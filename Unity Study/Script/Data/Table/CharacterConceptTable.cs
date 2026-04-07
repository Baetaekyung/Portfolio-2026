using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterConceptTable : BaseTable<CharacterConceptTable.RowData>
{
    [Serializable]
    public class RowData : Row
    {
        public string characterName;
        public string title;
        public string description;
        public string shortDesc;
        public string path;
    }
}
