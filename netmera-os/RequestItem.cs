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
    internal class RequestItem
    {
        public String path = String.Empty;
        public String contentType = String.Empty;
        public int max = 10;
        public int page = 0;
        public String sortOrder = String.Empty;
        public String sortBy = String.Empty;
        public String customCondition = String.Empty;
        public String searchText = String.Empty;
        public String locationSearchType = String.Empty;
        public String latitudes = String.Empty;
        public String longitudes = String.Empty;
        public String locationSearchField = String.Empty;
        public double distance = 0.0D;

        private String email;
        private String nickname;
        private String password;
        private String name;
        private String surname;
        private String fbId;
        private String twId;
        //private Map<String, String> profileAttributes;

        public FilterOperation filterOperation = FilterOperation.contains;
        public String filterBy="";
        public String filterValue="";

        public enum FilterOperation
        {
            contains, equals, startsWith, present
        }

        public String getEmail()
        {
            return email;
        }

        public void setEmail(String email)
        {
            this.email = email;
        }

        public String getNickname()
        {
            return nickname;
        }

        public void setNickname(String nickname)
        {
            this.nickname = nickname;
        }

        public String getPassword()
        {
            return password;
        }

        public void setPassword(String password)
        {
            this.password = password;
        }

        public String getName()
        {
            return name;
        }

        public void setName(String name)
        {
            this.name = name;
        }

        public String getSurname()
        {
            return surname;
        }

        public void setSurname(String surname)
        {
            this.surname = surname;
        }

        public String getFbId()
        {
            return fbId;
        }

        public void setFbId(String fbId)
        {
            this.fbId = fbId;
        }

        public String getTwId()
        {
            return twId;
        }

        public void setTwId(String twId)
        {
            this.twId = twId;
        }
    }
}