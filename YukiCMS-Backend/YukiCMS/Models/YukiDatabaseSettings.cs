using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YukiCMS.Models
{
    public class YukiDatabaseSettings : IYukiDatabaseSettings
    {
        public string dbConnectionUrl { get; set; }
        public string databaseName { get; set; }
        public string username { get; set; }
        public string password { get; set; }

        public string globalColleName { get; set; }
        public string userColleName { get; set; }
        public string committeeColleName { get; set; }
        public string seatColleName { get; set; }
        public string taskColleName { get; set; }
        public string taskTemplateColleName { get; set; }
        public string questionColleName { get; set; }
        public string permissionColleName { get; set; }
        public string permissionGroupColleName { get; set; }
        public string fileGroupColleName { get; set; }
        public string fileColleName { get; set; }
        public string billColleName { get; set; }
        public string accommodationManagementColleName { get; set; }
        public string findPasswordTokenColleName { get; set; }

    }
    public interface IYukiDatabaseSettings
    {
        string dbConnectionUrl { get; set; }
        string databaseName { get; set; }
        string username { get; set; }
        string password { get; set; }

        string globalColleName { get; set; }
        string userColleName { get; set; }
        string committeeColleName { get; set; }
        string seatColleName { get; set; }
        string taskColleName { get; set; }
        string taskTemplateColleName { get; set; }
        string questionColleName { get; set; }
        string permissionColleName { get; set; }
        string permissionGroupColleName { get; set; }
        string fileGroupColleName { get; set; }
        string fileColleName { get; set; }
        string billColleName { get; set; }
        string accommodationManagementColleName { get; set; }
        string findPasswordTokenColleName { get; set; }
    }
}
