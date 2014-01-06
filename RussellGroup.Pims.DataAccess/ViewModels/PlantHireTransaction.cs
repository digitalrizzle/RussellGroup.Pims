using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RussellGroup.Pims.DataAccess.ViewModels
{
    public class PlantHireTransaction
    {
        public Job Job { get; set; }

        public string Docket { get; set; }

        public virtual ICollection<Plant> Plants { get; set; }

        public virtual ICollection<PlantHire> PlantHire { get; set; }

        public int? JobId
        {
            get
            {
                return Job != null ? Job.JobId : (int?)null;
            }
        }
    }
}