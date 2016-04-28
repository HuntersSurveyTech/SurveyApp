using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Phone.Controls;

namespace HuntersWP.Services
{
    public class ExNavigationService
    {
        static PhoneApplicationFrame Frame
        {
            get
            {
                return Application.Current.RootVisual as PhoneApplicationFrame;
            }
        }

        public static void GoBack()
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }

        }

        public static void RemoveBackEntry()
        {
            Frame.RemoveBackEntry();

        }

        public static void Navigate<T>()
        {
            Navigate<T>(null);
        }

        public static void ClearNavigationHistory()
        {
            while (Frame.BackStack.Any())
            {
                Frame.RemoveBackEntry();
            }
        }

        public static void Navigate(string page, params object[] parameters)
        {
            var s = "";
            if (parameters != null)
            {
                for (int i = 0; i < parameters.Count(); i += 2)
                {
                    if (parameters[i + 1] == null) continue;

                    s += parameters[i] + "=" + parameters[i + 1] + "&";
                }
            }
            Frame.Navigate(new Uri(string.Format("/Pages/{0}.xaml?" + s, page),UriKind.Relative));
        }

        public static void Navigate<T>(params object[] parameters)
        {
            Navigate(typeof(T).Name, parameters);
        }
    }
}
