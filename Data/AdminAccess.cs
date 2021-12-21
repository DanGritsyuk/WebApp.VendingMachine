using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.VendingMachine
{
    public static class AdminAccess
    {
        public static string UserName = "Administrator";
        public static Guid AccessKey = GetAccessKey();

        private static Guid GetAccessKey()
        {
            try { return Guid.Parse(File.ReadAllText("App_Data/Config/AdminAccessKey.config")); }
            catch { return Guid.Empty; }
        }
    }
}
