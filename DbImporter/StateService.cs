using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HuntersWP.Models;

namespace HuntersWP.Services
{
   public class StateService
    {
       public static bool IsQA
       {
           get { return StateService.CurrentUserType == ESurveyorType.QA; }
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

       public static string CurrentServiceUri { get; set; }
        public static Guid CurrentUserId
        {
            get
            {
                return new ApplicationSettingsService().GetSetting<Guid>("CurrentUserId", Guid.Empty);
            }
            set
            {
                new ApplicationSettingsService().SetSetting("CurrentUserId", value);
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
    }

     public class ApplicationSettingsService 
    {
        static readonly object _locker = new object();

         static Dictionary<string, object> _settings = new Dictionary<string, object>(); 

        public void SetSetting<T>(string key, T value)
        {
            lock (_locker)
            {
                if (_settings.ContainsKey(key))
                {
                    _settings[key] = value;
                }
                else
                {
                    _settings.Add(key, value);
                }
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
                 if (_settings.ContainsKey(key))
                 {
                     return (T)_settings[key];
                 }

                 return defaultValue;
             }
         }

    }

}
