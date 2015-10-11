using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.Website.Tests
{
    public static class TestDataFactory
    {
        public static Setting GetSetting()
        {
            return new Setting
            {
                Key = "Hello",
                Value = "Hello, setting!"
            };
        }
    }
}
