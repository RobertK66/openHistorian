﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace openHistorian.V2.StorageSystem.Specialized
{
    public enum InsertResults
    {
        InsertedOK,
        NodeIsFullError,
        DuplicateKeyError
    }
}
