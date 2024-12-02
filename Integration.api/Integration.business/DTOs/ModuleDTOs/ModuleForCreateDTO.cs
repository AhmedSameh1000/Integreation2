﻿using Integration.business.DTOs.ModuleDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration.business.DTOs.ModuleDTOs
{

    public class ModuleForCreateDTO
    {
        public string ModuleName { get; set; }
        public string TableFromName { get; set; }
        public string TableToName { get; set; }
        public string ToPrimaryKeyName { get; set; }
        public string FromPrimaryKeyName { get; set; }
        public string LocalIdName { get; set; }
        public string CloudIdName { get; set; }
        public int ToDbId { get; set; }
        public int  FromDbId { get; set; }
        public string ToInsertFlagName { get; set; }
        public string ToUpdateFlagName { get; set; }
        public string FromInsertFlagName { get; set; }
        public string FromUpdateFlagName { get; set; }
        public List<ColumnMapping> Columns { get; set; }
        public List<Reference> References { get; set; }
        public string? condition { get; set; }
    }

}


