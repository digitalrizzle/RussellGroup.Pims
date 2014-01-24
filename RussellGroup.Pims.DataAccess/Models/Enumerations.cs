using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Models
{
    [Flags]
    [Serializable]
    public enum RoleType
    {
        // these have bitwise flags!
        Administrator = 1 << 0,         // 1
        Temp = 1 << 15,                 // 32768

        All = Administrator | Temp
    }
}
