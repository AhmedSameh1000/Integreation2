﻿using Integration.data.Models;

namespace Integration.business.DTOs.ModuleDTOs
{
    public class ModuleFullDataForReturnDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string TableFromName { get; set; }
        public string TableToName { get; set; }
        public string ToPrimaryKeyName { get; set; }
        public string fromPrimaryKeyName { get; set; }
        public string LocalIdName { get; set; }
        public string CloudIdName { get; set; }
        public int ToDbId { get; set; }
        public int FromDbId { get; set; }
        public string SyncType { get; set; }
        public string ToInsertFlagName { get; set; }
        public string ToUpdateFlagName { get; set; }
        public string FromInsertFlagName { get; set; }
        public string FromUpdateFlagName { get; set; }
        public List<ColumnFromDTO>  columnsFromDTOs { get; set; }
    }

    public class ColumnFromDTO
    {
        public int Id { get; set; }
        public string ColumnFromName { get; set; }
        public string ColumnToName { get; set; }
        public int ModuleId { get; set; }

        public bool isReference { get; set; }
        public string? TableReferenceName { get; set; }
    }
}