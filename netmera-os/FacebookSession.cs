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
using System.IO.IsolatedStorage;

namespace Netmera
{
    internal class FacebookSession
    {
        private static readonly String ACCESS_TOKEN = "access_token";

        private static IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;

        public static void save(Facebook facebook)
        {
            clear();
            settings.Add(ACCESS_TOKEN, facebook.getAccessToken());
        }

        public static bool restore(Facebook session)
        {
            String accessToken = "";
            if (settings.Contains(ACCESS_TOKEN))
            {
                accessToken = settings[ACCESS_TOKEN].ToString();
            }
            session.setAccessToken(accessToken);

            return session.isSessionValid();
        }

        public static void clear()
        {
            settings.Remove(ACCESS_TOKEN);
        }
    }
}
