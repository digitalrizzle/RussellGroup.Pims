using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace RussellGroup.Pims.Website.Models
{
    public class FileViewModel
    {
        private static readonly Regex ContentTypeRegex = new Regex(@"^(data:)(.*)(;base64,)(.*)");

        public int Id { get; set; }

        public string Src { get; set; }

        public string FileName { get; set; }

        // hide the constructor so that the Create method must be used to instantiate
        private FileViewModel() { }

        public bool HasContent
        {
            get
            {
                return !string.IsNullOrEmpty(Src);
            }
        }

        public string ContentType
        {
            get
            {
                if (Src != null)
                {
                    return GetSrcMatchGroups()[2].Value;
                }

                throw new NullReferenceException();
            }
        }

        public byte[] Content
        {
            get
            {

                if (Src != null)
                {
                    return Convert.FromBase64String(GetSrcMatchGroups()[4].Value);
                }

                throw new NullReferenceException();
            }
        }

        public static FileViewModel Create(File file)
        {
            var model = new FileViewModel();

            if (file != null)
            {
                model.Id = file.Id;
                model.Src = GetSrc(file.ContentType, file.Content.Data);
                model.FileName = file.FileName;
            }

            return model;
        }

        public static string GetSrc(string contentType, byte[] data)
        {
            var content = data != null
                ? string.Format("data:{0};base64,{1}", contentType, Convert.ToBase64String(data))
                : null;

            return content;
        }

        private GroupCollection GetSrcMatchGroups()
        {
            var match = ContentTypeRegex.Match(Src);

            if (match.Success)
            {
                var groups = match.Groups;
                return groups;
            }

            throw new ArgumentException();
        }
    }
}