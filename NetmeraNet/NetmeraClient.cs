using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        /// <summary>
        /// sets root security token
        /// </summary>
        internal static void finish()
        {
            setRootSecurityToken();
        }
    }
}