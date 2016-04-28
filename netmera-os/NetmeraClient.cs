using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Netmera
{
    /// <copyright>Copyright 2012 Inomera Research</copyright>
    /// <summary>
    /// It contains the configuration methods.
    /// </summary>
    public static class NetmeraClient
    {
        internal static String securityToken;
        private static String securityTokenBackup;

        /// <summary>
        /// Authenticates user and application. It is recommended to call this method at the beginning of the program.
        /// </summary>
        /// <param name="st">User and Application specific key</param>
        public static void init(String st)
        {
            securityToken = st;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal static String getSecurityToken()
        {
            return securityToken;
        }

        internal static void setLoggedUserSecurityToken(String st)
        {
            securityTokenBackup = securityToken;
            securityToken = st;
        }

        private static void setRootSecurityToken()
        {
            if (securityTokenBackup != null)
            {
                securityToken = securityTokenBackup;
                securityTokenBackup = null;
            }
        }

        internal static void finish()
        {
            setRootSecurityToken();
        }
    }
}