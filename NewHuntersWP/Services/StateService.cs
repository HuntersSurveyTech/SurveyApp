using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HuntersWP.Controls;
using HuntersWP.Models;

namespace HuntersWP.Services
{
    public class StateService
    {
        public static bool IsQA
        {
            get { return StateService.CurrentUserType == ESurveyorType.QA; }
        }

        public static string CurrentServiceUri { get; set; }

        public static QuestionGroup CurrentQuestionGroup
        {
            get
            {
                return new ApplicationSettingsService().GetSetting<QuestionGroup>("QuestionGroup", null);
            }
            set
            {
                new ApplicationSettingsService().SetSetting("QuestionGroup", value);
            }
        }

        public static Address CurrentAddress
        {
            get
            {
                return new ApplicationSettingsService().GetSetting<Address>("CurrentAddress", null);
            }
            set
            {
                new ApplicationSettingsService().SetSetting("CurrentAddress", value);
            }
        }


        public static Customer CurrentCustomer
        {
            get
            {
                return new ApplicationSettingsService().GetSetting<Customer>("CurrentCustomer", null);
            }
            set
            {
                new ApplicationSettingsService().SetSetting("CurrentCustomer", value);
            }
        }

        public static string Login
        {
            get
            {
                return new ApplicationSettingsService().GetSetting<string>("Login","");
            }
            set
            {
                new ApplicationSettingsService().SetSetting("Login", value);
            }
        }

        public static string Password
        {
            get
            {
                return new ApplicationSettingsService().GetSetting<string>("Password", "");
            }
            set
            {
                new ApplicationSettingsService().SetSetting("Password", value);
            }
        }

        public static Guid CurrentUserId
        {
            get
            {
                return new ApplicationSettingsService().GetSetting<Guid>("CurrentUserId",Guid.Empty);
            }
            set
            {
                new ApplicationSettingsService().SetSetting("CurrentUserId",value);
            }
        }

        public static ESurveyorType CurrentUserType
        {
            get
            {
                return new ApplicationSettingsService().GetSetting<ESurveyorType>("CurrentUserType", ESurveyorType.Common);
            }
            set
            {
                new ApplicationSettingsService().SetSetting("CurrentUserType", value);
            }
        }

        public static int CurrentUserTimeStamp
        {
            get
            {
                return new ApplicationSettingsService().GetSetting<int>("CurrentUserTimeStamp", 0);
            }
            set
            {
                new ApplicationSettingsService().SetSetting("CurrentUserTimeStamp", value);
            }
        }



        public static Guid PreviousUserId
        {
            get
            {
                return new ApplicationSettingsService().GetSetting<Guid>("PreviousUserId", Guid.Empty);
            }
            set
            {
                new ApplicationSettingsService().SetSetting("PreviousUserId", value);
            }
        }

        public static ProgressIndicatorService ProgressIndicatorService { get; set; }

        public static bool LoginEnabled
        {
            get
            {
                return new ApplicationSettingsService().GetSetting<bool>("LoginEnabled");
            }
            set
            {
                new ApplicationSettingsService().SetSetting("LoginEnabled", value);
            }
        }

    }

    public class ApplicationSettingsService 
    {
        static readonly object _locker = new object();

        public void SetSetting<T>(string key, T value)
        {
            lock (_locker)
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings[key] = value;
                settings.Save();
            }
        }

        public T GetSetting<T>(string key)
        {
            return GetSetting(key, default(T));
        }

        public T GetSetting<T>(string key, T defaultValue)
        {
            lock (_locker)
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return settings.Contains(key) &&
                       settings[key] is T
                    ? (T)settings[key]
                    : defaultValue;
            }


        }

        public bool HasSetting<T>(string key)
        {
            lock (_locker)
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return settings.Contains(key) && settings[key] is T;
            }
        }

        public bool RemoveSetting(string key)
        {
            lock (_locker)
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                if (settings.Contains(key))
                {
                    var res = settings.Remove(key);
                    settings.Save();
                    return res;
                }
                return false;
            }
        }
    }
}
