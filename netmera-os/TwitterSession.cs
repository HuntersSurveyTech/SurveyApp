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

    internal class TwitterSession
    {
        private static readonly String ACCESS_TOKEN = "access_token";
        private static readonly String SECRET_TOKEN = "secret_token";

        private static IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;

        public static void save(Twitter twitter)
        {
            clear();
            settings.Add(ACCESS_TOKEN, twitter.getAccessToken());
            settings.Add(SECRET_TOKEN, twitter.getSecretToken());
        }

        public static bool restore(Twitter session)
        {
            String accessToken = "";
            String secretToken = "";
            if (settings.Contains(ACCESS_TOKEN) && settings.Contains(SECRET_TOKEN))
            {
                accessToken = settings[ACCESS_TOKEN].ToString();
                secretToken = settings[SECRET_TOKEN].ToString();
            }

            session.setAccessToken(accessToken);
            session.setSecretToken(secretToken);

            return session.isSessionValid();
        }

        public static void clear()
        {
            settings.Remove(ACCESS_TOKEN);
            settings.Remove(SECRET_TOKEN);
        }
    }
}
