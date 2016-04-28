using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Netmera
{
    internal static class NetmeraConstants
    {
        //public static readonly String Netmera_Domain_Url = "http://www.netmera.org";
        public static readonly String Netmera_Domain_Url = "http://netmera.com";
        //public static readonly String Netmera_Domain_Url = "http://www.netmera.net";
        public static readonly String Netmera_Domain_Rest_Url = "/social/rest";
        public static readonly String Netmera_Domain_Rpc_Url = "/social/rpc?";
        public static readonly String Netmera_CreateContent_Url = "/content/createContentWithoutActionToken?";
        public static readonly String Netmera_CreateBulkContent_Url = "/content/createBulkContentWithoutActionToken?";
        public static readonly String Netmera_UpdateContent_Url = "/content/updateContentWithoutActionToken?";
        public static readonly String Netmera_RemoveContent_Url = "/content/deleteContent?";
        public static readonly String Netmera_SearchContent_Url = "/content/search?";
        public static readonly String Netmera_GetContent_Url = "/content/get?";
        public static readonly String Netmera_LocationSearchContent_Url = "/content/locationSearch?";
        public static readonly String Netmera_RegisterUser_Url = "/site/register?";
        public static readonly String Netmera_LoginUser_Url = "/site/login?";
        public static readonly String Netmera_ActivateUser_Url = "/site/activateUser?";
        public static readonly String Netmera_DeactivateUser_Url = "/site/deactivateUser?";
        public static readonly String Netmera_PeopleSearch_Url = "/people/search?";
        public static readonly String Netmera_PeopleProfileUpdate_Url = "/people/profileUpdate?";
        public static readonly String Netmera_PeopleAccountUpdate_Url = "/people/accountUpdate?";
        public static readonly String Netmera_People_Url = "/people";
        public static readonly String Cdn_Domain = Netmera_Domain_Url + "/cdn";

        public static readonly String Netmera_PeopleProfileUpdate_Method = "people.profileUpdate";
        public static readonly String Netmera_PeopleAccountUpdate_Method = "people.accountUpdate";
        public static readonly String Netmera_LoginUser_Method = "site.login";
        public static readonly String Netmera_LoginGuestUser_Method = "site.guestLogin";
        public static readonly String Netmera_ActivateUser_Method = "site.activateUser";
        public static readonly String Netmera_DeactivateUser_Method = "site.deactivateUser";

        public static readonly String Url_Pattern = "(http|https):\\/\\/(\\w+:{0,1}\\w*@)?(\\S+)(:[0-9]+)?(\\/|\\/([\\w#!:.?+=&%@!\\-\\/]))?";

        public static readonly String Http_Method_Post = "POST";
        public static readonly String Http_Method_Get = "GET";
        public static readonly String Http_Content_Type = "application/json";

        public static readonly String Netmera_User = "user";
        public static readonly String Netmera_UserEmail = "email";
        public static readonly String Netmera_UserEmailValue = "value";
        public static readonly String Netmera_UserEmails = "emails";
        public static readonly String Netmera_UserPassword = "password";
        public static readonly String Netmera_UserLoginPassword = "pwd";
        public static readonly String Netmera_UserNickname = "nickname";
        public static readonly String Netmera_UserDefaultNickname = "defaultNickname";
        public static readonly String Netmera_UserName = "name";
        public static readonly String Netmera_UserGivenName = "givenName";
        public static readonly String Netmera_UserFamilyName = "familyName";
        public static readonly String Netmera_UserSurname = "surname";
        public static readonly String Netmera_UserSecurityToken = "st";
        public static readonly String Netmera_UserProfileAttributes = "profile";

        public static readonly String Netmera_Owner = "owner";
        public static readonly String Netmera_Nickname = "nickname";
        public static readonly String Netmera_Thumbnail_Url = "thumbnailUrl";
        public static readonly String Netmera_Owner_Node_Name = "nodeName";
        public static readonly String Netmera_Owner_Path = "path";

        public static readonly String Netmera_SDK_Params = "sdk";
        public static readonly String Netmera_SDKVERSION_Params = "sdkVersion";

        public static readonly String Netmera_SDK_Value = "net";
        public static readonly String Netmera_SDKVERSION_Value = "1.4.2";

        public static readonly String SecurityToken_Params = "st";
        public static readonly String Path_Params = "path";
        public static readonly String Service_Params = "service";
        public static readonly String Action_Params = "action";
        public static readonly String ContentType_Params = "contentType";
        public static readonly String ContentName_Params = "contentName";
        public static readonly String ContentActionToken_Params = "contentActionToken";
        public static readonly String ContentCreateDate_Params = "createDate";
        public static readonly String ContentUpdateDate_Params = "updateDate";
        public static readonly String Content_Params = "content";
        public static readonly String Owner_Params = "owner";
        public static readonly String Type_Params = "type";
        public static readonly String ContentData_Params = "data";
        public static readonly String ContentPrivacy_Params = "contentPrivacy";
        public static readonly String CustomCondition_Params = "customCondition";
        public static readonly String SearchText_Params = "searchText";
        public static readonly String Max_Params = "max";
        public static readonly String Page_Params = "page";
        public static readonly String SortBy_Params = "sortBy";
        public static readonly String SortOrder_Params = "sortOrder";
        public static readonly String Filter_Params = "filterBy";
        public static readonly String FilterValue_Params = "filterValue";
        public static readonly String FilterOperation_Params = "filterOp";
        public static readonly String Entry_Params = "entry";
        public static readonly String Method_Params = "method";
        public static readonly String Params_Params = "params";
        public static readonly String LocationSearchType_Params = "searchType";
        public static readonly String LocationSearchField_Params = "fieldName";
        public static readonly String LocationLatitude_Params = "latitude";
        public static readonly String LocationLongitude_Params = "longitude";
        public static readonly String LocationDistance_Params = "distance";
        public static readonly String ContentPrivacy_Field = "privacyTypeName";

        public static readonly String Default_ParentPath = "/mobimeracontents";
        public static readonly String Default_Service = "netmera-mobimera";
        public static readonly String Default_ContentType = "netmera-mobimera:mobimera";
        public static readonly String Default_Privacy = "public";
        public static readonly String Privacy_Public = "public";
        public static readonly String Privacy_Private = "private";
        public static readonly String ApiContentType = "netmera-mobimera:api-content-type";

        public static readonly String Create_Action = "netmera-mobimera:create-mobimera";
        public static readonly String Update_Action = "netmera-mobimera:update-mobimera";

        public static readonly String ContentKey = "key";
        public static readonly String ContentValue = "value";

        public static readonly String LocationField_Suffix = "_netmera_mobile_loc";
        public static readonly String LocationLatitude_Suffix = "_netmera_mobile_latitude";
        public static readonly String LocationLongitude_Suffix = "_netmera_mobile_longitude";

        public static readonly String LocationSearchType_Circle = "circle";
        public static readonly String LocationSearchType_Box = "box";

        /************************** Netmera Media Constants **********************************************/

        public static readonly String Upload_Url = Netmera_Domain_Url + "/photo/app/upload/entry?";
        public static readonly String Swf_Url = Netmera_Domain_Url + "/cdn/app/upload/swfUpl?";
        public static readonly String Save_Photo_Url = Netmera_Domain_Url + "/photo/app/upload/save?";

        public static readonly String Upload_Url_Params_St = "st=";
        public static readonly String Upload_Url_Params_Content_Path = "contentPath=";
        public static readonly String Upload_Url_Params_Opensocial_Viewer_Id = "opensocial_viewer_id=";
        public static readonly String Upload_Url_Params_Opensocial_Netmera_Domain = "opensocial_netmera_domain=";
        public static readonly String Upload_Url_Params_Opensocial_App_Id = "opensocial_app_id=";

        public static readonly String Swf_Url_Params_Upload_Type = "uploadType_site_domain=";

        public static readonly String Save_Photo_Url_Params_St = "st=";
        public static readonly String Save_Photo_Url_Params_Album = "album=";
        public static readonly String Save_Photo_Url_Params_Uploaded_Photo_Hash = "uploadedPhotoHash=";
        public static readonly String Save_Photo_Url_Params_Privacy = "privacy=";
        public static readonly String Save_Photo_Url_Params_Cdn_Domain = "cdnDomain=";
        public static readonly String Save_Photo_Url_Params_Opensocial_App_Id = "opensocial_app_id=";
        public static readonly String Save_Photo_Url_Params_Opensocial_Netmera_Domain = "opensocial_netmera_domain=";
        public static readonly String Save_Photo_Url_Params_Opensocial_Viewer_Id = "opensocial_viewer_id=";

        public static readonly String Params_Splitter = "&";

        public static readonly String Site = "site";
        public static readonly String Domain = "domain";
        public static readonly String Path = "path";
        public static readonly String Album_List = "albumList";
        public static readonly String Upload_Key = "uploadKey";

        public static readonly String ACTION_TOKEN_KEY = "key";

        public static readonly String Netmera_Media_Photo = "photo";
        public static readonly String Netmera_Media_Content = "content";
        public static readonly String Netmera_Media_Data = "data";
        public static readonly String Netmera_Media_Thumbnail_Url = "thumbnailUrl";

        /**************************************************************************************************************/

        public static readonly String Netmera_Push_Server_Url = "/mobimera/app/push/";
        public static readonly String Netmera_Push_Register = "register";
        public static readonly String Netmera_Push_Unregister = "unregister";
        public static readonly String Netmera_Push_Channel = "channel";
        public static readonly String Netmera_Push_Channels = "channels";
        public static readonly String Netmera_Push_Devices_List = "devicesList";
        public static readonly String Netmera_Push_Send_To_Android = "sendToAndroid";
        public static readonly String Netmera_Push_Send_To_Ios = "sendToIos";
        public static readonly String Netmera_Push_Send = "sendPush";
        public static readonly String Netmera_Push_Type_Android = "android";
        public static readonly String Netmera_Push_Type_Ios = "ios";
        public static readonly String Netmera_Push_Type_Wp = "wp";
        public static readonly String Netmera_Push_Type_Circle_Location = "circle";
        public static readonly String Netmera_Push_Type_Box_Location = "box";

        public static readonly String Netmera_Push_LocationType_Params = "locationType";
        public static readonly String Netmera_Push_Latitude1_Params = "lat1";
        public static readonly String Netmera_Push_Latitude2_Params = "lat2";
        public static readonly String Netmera_Push_Longitude1_Params = "lng1";
        public static readonly String Netmera_Push_Longitude2_Params = "lng2";

        public static readonly String Netmera_Push_Registration_Id = "regId";
        public static readonly String Netmera_Push_Security_Token = "st";
        public static readonly String Netmera_Push_Apikey = "apiKey";
        public static readonly String Netmera_Push_Device_Groups = "deviceGroups";

        public static readonly String Netmera_Push_Display_Message_Action = "com.netmera.mobile.DISPLAY_MESSAGE";
        public static readonly String Netmera_Push_Extra_Message = "message";
        public static readonly String Netmera_Push_Data = "netmera_push_data";
        public static readonly String Netmera_Push_Default_Application_Name = "NetmeraPush";

        public static readonly String Netmera_Push_Android_Data = "data";
        public static readonly String Netmera_Push_Android_Message = "message";
        public static readonly String Netmera_Push_Android_Collapse_Key = "collapseKey";
        public static readonly String Netmera_Push_Android_Deal_While_Idle = "dealWhileIdle";
        public static readonly String Netmera_Push_Android_Time_To_Live = "timeToLive";

        public static readonly String Netmera_Push_Ios_Data = "data";
        public static readonly String Netmera_Push_Ios_Message = "message";
        public static readonly String Netmera_Push_Ios_App_Name = "appName";
    }
}