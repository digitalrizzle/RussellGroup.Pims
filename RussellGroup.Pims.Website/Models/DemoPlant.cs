using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.Website.Models
{
    public class DemoPlant
    {
        public List<Plant> IncludedItems { get; set; }

        public List<Plant> ExcludedItems { get; set; }
    }
}
