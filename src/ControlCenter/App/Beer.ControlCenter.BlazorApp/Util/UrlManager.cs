using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.ControlCenter.BlazorApp.Util
{
    public static class UrlManager
    {
        public static String UserOverview => "/Users";
        public static String Settings => "/Settings";
        public static String MessageCenter => "/MessageCenter";
        public static String CreateNewUser => "/new-user";
        public static String ClientOverview => "/clients";
        public static String CreateNewClient => "/new-client";


    }
}
