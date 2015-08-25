using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RussellGroup.Pims.DataAccess.Models;

namespace RussellGroup.Pims.Website.Models
{
    public class PlantViewModel
    {
        public Plant Plant { get; set; }

        public FileViewModel FileViewModel { get; set; }

        public void UpdatePlantPhotograph()
        {
            var photo = Plant.Photograph;

            if (FileViewModel.HasContent)
            {
                if (photo == null)
                {
                    photo = new File
                    {
                        FileName = FileViewModel.FileName,
                        Content = new Content
                        {
                            Data = FileViewModel.Content
                        },
                        ContentType = FileViewModel.ContentType
                    };
                }
                else
                {
                    photo.Id = FileViewModel.Id;
                    photo.FileName = FileViewModel.FileName;
                    photo.Content.Data = FileViewModel.Content;
                    photo.ContentType = FileViewModel.ContentType;
                }
            }
            else
            {
                photo = null;
            }

            Plant.Photograph = photo;
        }
    }
}