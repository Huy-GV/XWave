using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XWave.Core.Services.Communication
{
    public enum ErrorCode
    {
        Undefined = 0,
        EntityNotFound,
        EntityAlreadyExist,
        EntityInvalidState,
        EntityInconsistentStates
    }
}