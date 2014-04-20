using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RussellGroup.Pims.Website.Helpers
{
    public interface IActiveDirectoryHelper : IDisposable
    {
        bool IsAuthorized(string roles);
    }
}