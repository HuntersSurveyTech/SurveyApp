using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Newtonsoft.Json.Linq;
using Netmera;
using Newtonsoft.Json;
using System.IO;

namespace Netmera
{
    /// <summary>
    /// It is for managing users of the application. You can
    /// register, update, login users with this class.
    /// </summary>
    public class NetmeraUser
    {
        private String email;
        private String password;
        private String nickname;
        private String name;
        private String surname;

        private static NetmeraUser currentUser;
        internal static String securityToken;

        /// <summary>
        /// Default constructor to create user object.
        /// </summary>
        public NetmeraUser()
        {
        }

        /// <summary>
        /// Returns the email of the user
        /// </summary>
        /// <returns>Email of the user</returns>
        public String getEmail()
        {
            return email;
        }

        /// <summary>
        /// Sets the email of the user
        /// </summary>
        /// <param name="email">Email of the user</param>
        public void setEmail(String email)
        {
            this.email = email;
        }

        /// <summary>
        /// Sets the password of the user
        /// </summary>
        /// <param name="password">Password of the user</param>
        public void setPassword(String password)
        {
            this.password = password;
        }

        /// <summary>
        /// Returns the nickname of the user
        /// </summary>
        /// <returns>nNickname of the user</returns>
        public String getNickname()
        {
            return nickname;
        }

        /// <summary>
        /// Sets the nickname of the user
        /// </summary>
        /// <param name="nickname">Nickname of the user</param>
        public void setNickname(String nickname)
        {
            this.nickname = nickname;
        }

        /// <summary>
        /// Returns the name of the user
        /// </summary>
        /// <returns>name of the user</returns>
        public String getName()
        {
            return name;
        }

        /// <summary>
        /// Sets the name of the user
        /// </summary>
        /// <param name="name">Name of the user</param>
        public void setName(String name)
        {
            this.name = name;
        }

        /// <summary>
        /// Returns the surname of the user
        /// </summary>
        /// <returns>Surname of the user</returns>
        public String getSurname()
        {
            return surname;
        }

        /// <summary>
        /// Sets the surname of the user
        /// </summary>
        /// <param name="surname">Surname of the user</param>
        public void setSurname(String surname)
        {
            this.surname = surname;
        }

        /// <summary>
        /// Gets the currently logged user
        /// </summary>
        /// <returns>Currently logged user</returns>
        public static NetmeraUser getCurrentUser()
        {
            return currentUser;
        }

        /// <summary>
        /// Registers new user. Before calling this method email,password and	
        /// nickname fields of the <see cref="NetmeraUser"/> should be set. Those are
        /// the compulsory fields. There are also optional name and surname fields.
        /// </summary>
        public void register()
        {
            try
            {
                RequestItem registerUserReqItem = new RequestItem();
                registerUserReqItem.setEmail(email);
                registerUserReqItem.setPassword(password);
                registerUserReqItem.setNickname(nickname);
                registerUserReqItem.setName(name);
                registerUserReqItem.setSurname(surname);

                JObject jUser = register(registerUserReqItem);

                setUser(jUser);
            }
            catch (WebException)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_REQUEST, "Request exception occurred while registering user");
            }
            catch (IOException)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_IO_EXCEPTION, "IO Exception occurred while registering user");
            }
            catch (JsonException)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_JSON, "Json in the response of register user method is invalid");
            }
        }

        /// <summary>
        /// Updates user info. Before calling this method email,password and nickname
        /// fields of the <see cref="NetmeraUser"/> should be set. Those are the
        /// compulsory fields.
        /// </summary>
        public void update()
        {
            JObject jProfile = null;
            JObject jAccount = null;

            try
            {
                RequestItem updateUserReqItem = new RequestItem();
                updateUserReqItem.setEmail(email);
                updateUserReqItem.setPassword(password);
                updateUserReqItem.setNickname(nickname);
                updateUserReqItem.setName(name);
                updateUserReqItem.setSurname(surname);

                if (nickname != null)
                {
                    jProfile = profileUpdate(updateUserReqItem);

                    setUser(jProfile);
                    if (password != null)
                    {
                        jAccount = accountUpdate(updateUserReqItem);
                    }
                }
                else if (password != null)
                {
                    jAccount = accountUpdate(updateUserReqItem);
                    setUser(jAccount);
                }
            }
            catch (WebException)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_REQUEST, "Web exception occurred while updating user");
            }
            catch (IOException)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_IO_EXCEPTION, "IO Exception occurred while updating user");
            }
            catch (JsonException)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_JSON, "Json in the response of update user method is invalid");
            }
        }

        /// <summary>
        /// Logs a guest into the application.
        /// </summary>
        /// <returns>The logged user</returns>
        public static NetmeraUser loginAsGuest()
        {
            NetmeraUser user = new NetmeraUser();
            try
            {
                JObject jLogin = loginAsGuestRequest();
                if (jLogin != null)
                {
                    user = setCurrentUser(jLogin);
                }
                else
                {
                    throw new NetmeraException(NetmeraException.ErrorCode.EC_NULL_EXCEPTION, "Response of user login method is null");
                }
            }
            catch (WebException)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_REQUEST, "Web exception occurred in user login method");
            }
            catch (IOException)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_IO_EXCEPTION, "IO Exception occurred in user login method");
            }
            catch (JsonException)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_JSON, "Json in the response of user login method is invalid");
            }

            return user;
        }

        /// <summary>
        /// Logs a user into the registered application. Email and password fields of
        /// user is used for this operation.
        /// </summary>
        /// <param name="email">Email of the user</param>
        /// <param name="password">Password of the user</param>
        /// <returns>The logged user</returns>
        public static NetmeraUser login(String email, String password)
        {
            NetmeraUser user = new NetmeraUser();
            try
            {
                RequestItem loginUserReqItem = new RequestItem();
                loginUserReqItem.setEmail(email);
                loginUserReqItem.setPassword(password);

                JObject jLogin = login(loginUserReqItem);
                if (jLogin != null)
                {
                    user = setCurrentUser(jLogin);
                }
                else
                {
                    throw new NetmeraException(NetmeraException.ErrorCode.EC_NULL_EXCEPTION, "Response of user login method is null");
                }
            }
            catch (WebException)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_REQUEST, "Web exception occurred in user login method");
            }
            catch (IOException)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_IO_EXCEPTION, "IO Exception occurred in user login method");
            }
            catch (JsonException)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_JSON, "Json in the response of user login method is invalid");
            }

            return user;
        }

        /// <summary>
        /// User logged out from the application
        /// </summary>
        public static void logout()
        {
            securityToken = null;
            currentUser = null;
        }

        /// <summary>
        /// Activates the registered User.
        /// </summary>
        /// <param name="email">Email of the user</param>
        public void activateUser(String email)
        {
            try
            {
                RequestItem activateUserReqItem = new RequestItem();
                activateUserReqItem.setEmail(email);

                activateUser(activateUserReqItem);
            }
            catch (WebException)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_REQUEST, "Request exception occurred while activating user");
            }
            catch (IOException)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_IO_EXCEPTION, "IO Exception occurred while activating user");
            }
        }

        /// <summary>
        /// Deactivates the registered User.
        /// </summary>
        /// <param name="email">Email of the user</param>
        public void deactivateUser(String email)
        {
            try
            {
                RequestItem deactivateUserReqItem = new RequestItem();
                deactivateUserReqItem.setEmail(email);

                deactivateUser(deactivateUserReqItem);
            }
            catch (WebException)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_REQUEST, "Request exception occurred while activating user");
            }
            catch (IOException)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_IO_EXCEPTION, "IO Exception occurred while activating user");
            }
        }

        private JObject register(RequestItem item)
        {
            JObject json = null;
            if (String.IsNullOrEmpty(item.getEmail()))
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_REQUIRED_FIELD, NetmeraConstants.Netmera_UserEmail + " is required");
            }
            if (String.IsNullOrEmpty(item.getNickname()))
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_REQUIRED_FIELD, NetmeraConstants.Netmera_UserNickname + " is required");
            }
            if (String.IsNullOrEmpty(item.getPassword()))
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_REQUIRED_FIELD, NetmeraConstants.Netmera_UserPassword + " is required");
            }
            try
            {
                json = NetmeraHttpUtils.registerUser(item);
            }
            catch (NetmeraException)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_USER_REGISTER_ERROR, "Error occurred while registering user");
            }

            if (json != null)
            {
                if (json["entry"] != null && json["entry"].Count() != 0)
                {
                    return (JObject)json.First.First;
                }
                else if (json["error"] != null)
                {
                    String error = json["error"]["message"].ToString();
                    throw new NetmeraException(NetmeraException.ErrorCode.EC_USER_REGISTER_ERROR, error);
                }
                else
                    throw new NetmeraException(NetmeraException.ErrorCode.EC_USER_REGISTER_ERROR, "Error occurred while registering user");
            }
            return null;
        }

        private JObject profileUpdate(RequestItem item)
        {
            JObject json = null;
            if (String.IsNullOrEmpty(item.getEmail()))
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_REQUIRED_FIELD, NetmeraConstants.Netmera_UserEmail + " is required");
            }
            if (String.IsNullOrEmpty(item.getNickname()))
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_REQUIRED_FIELD, NetmeraConstants.Netmera_UserNickname + " is required");
            }
            try
            {
                json = NetmeraHttpUtils.profileUpdate(item);
            }
            catch (NetmeraException)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_USER_UPDATE_ERROR, "Error occurred while updating user");
            }

            if (json != null)
            {
                if (json["data"] != null && json["data"].Count() != 0)
                {
                    return (JObject)json.First.First;
                }
                else if (json["error"] != null)
                {
                    String error = json["error"]["message"].ToString();
                    throw new NetmeraException(NetmeraException.ErrorCode.EC_USER_UPDATE_ERROR, error);
                }
                else
                    throw new NetmeraException(NetmeraException.ErrorCode.EC_USER_UPDATE_ERROR, "Error occurred while updating user");
            }
            return null;
        }

        private JObject accountUpdate(RequestItem item)
        {
            JObject json = null;
            if (String.IsNullOrEmpty(item.getEmail()))
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_REQUIRED_FIELD, NetmeraConstants.Netmera_UserEmail + " is required");
            }
            if (String.IsNullOrEmpty(item.getPassword()))
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_REQUIRED_FIELD, NetmeraConstants.Netmera_UserPassword + " is required");
            }
            try
            {
                json = NetmeraHttpUtils.accountUpdate(item);
            }
            catch (NetmeraException)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_USER_UPDATE_ERROR, "Error occurred while updating user");
            }
            if (json != null)
            {
                if (json["data"] != null && json["data"].Count() != 0)
                {
                    return (JObject)json.First.First;
                }
                else if (json["error"] != null)
                {
                    String error = json["error"]["message"].ToString();
                    throw new NetmeraException(NetmeraException.ErrorCode.EC_USER_UPDATE_ERROR, error);
                }
                else
                    throw new NetmeraException(NetmeraException.ErrorCode.EC_USER_UPDATE_ERROR, "Error occurred while updating user");
            }
            return null;
        }

        private static JObject loginAsGuestRequest()
        {
            JObject json = null;
            try
            {
                json = NetmeraHttpUtils.loginAsGuest();
            }
            catch (NetmeraException)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_USER_LOGIN_ERROR, "Error occurred while logging user");
            }
            if (json != null)
            {
                if (json["data"] != null && json["data"].Count() != 0)
                {
                    return (JObject)json.First.First;
                }
                else if (json["error"] != null)
                {
                    String error = json["error"]["message"].ToString();
                    throw new NetmeraException(NetmeraException.ErrorCode.EC_USER_LOGIN_ERROR, error);
                }
                else
                    throw new NetmeraException(NetmeraException.ErrorCode.EC_USER_LOGIN_ERROR, "Error occurred while logging user");
            }
            return json;
        }

        private static JObject login(RequestItem item)
        {
            JObject json = null;
            if (String.IsNullOrEmpty(item.getEmail()))
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_REQUIRED_FIELD, NetmeraConstants.Netmera_UserEmail + " is required");
            }
            if (String.IsNullOrEmpty(item.getPassword()))
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_REQUIRED_FIELD, NetmeraConstants.Netmera_UserPassword + " is required");
            }
            try
            {
                json = NetmeraHttpUtils.login(item);
            }
            catch (NetmeraException)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_USER_LOGIN_ERROR, "Error occurred while logging user");
            }
            if (json != null)
            {
                if (json["data"] != null && json["data"].Count() != 0)
                {
                    return (JObject)json.First.First;
                }
                else if (json["error"] != null)
                {
                    String error = json["error"]["message"].ToString();
                    throw new NetmeraException(NetmeraException.ErrorCode.EC_USER_LOGIN_ERROR, error);
                }
                else
                    throw new NetmeraException(NetmeraException.ErrorCode.EC_USER_LOGIN_ERROR, "Error occurred while logging user");
            }
            return json;
        }

        private void activateUser(RequestItem item)
        {
            if (String.IsNullOrEmpty(item.getEmail()))
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_REQUIRED_FIELD, NetmeraConstants.Netmera_UserEmail + " is required");
            }
            NetmeraHttpUtils.activateUser(item);
        }

        private void deactivateUser(RequestItem item)
        {
            if (String.IsNullOrEmpty(item.getEmail()))
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_REQUIRED_FIELD, NetmeraConstants.Netmera_UserEmail + " is required");
            }
            NetmeraHttpUtils.deactivateUser(item);
        }

        internal void setUser(JObject json)
        {
            if (json == null)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_NULL_EXCEPTION, "Json contains user info is null");
            }
            try
            {
                if (json[NetmeraConstants.Netmera_UserEmails] != null)
                {
                    JObject emailObj = (JObject)json[NetmeraConstants.Netmera_UserEmails].First();

                    if (emailObj[NetmeraConstants.Netmera_UserEmailValue] != null)
                    {
                        this.email = emailObj[NetmeraConstants.Netmera_UserEmailValue].ToString();
                    }
                }

                if (json[NetmeraConstants.Netmera_UserNickname] != null)
                    this.nickname = json[NetmeraConstants.Netmera_UserNickname].ToString();

                if (json[NetmeraConstants.Netmera_UserName] != null)
                {
                    JObject jUserObj = (JObject)json[NetmeraConstants.Netmera_UserName];

                    if (jUserObj[NetmeraConstants.Netmera_UserGivenName] != null)
                    {
                        this.name = jUserObj[NetmeraConstants.Netmera_UserGivenName].ToString();
                    }
                    if (jUserObj[NetmeraConstants.Netmera_UserFamilyName] != null)
                    {
                        this.surname = jUserObj[NetmeraConstants.Netmera_UserFamilyName].ToString();
                    }
                }
            }
            catch (JsonException e)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_JSON, "Json is invalid.", e.Message);
            }
        }
        private static NetmeraUser setCurrentUser(JObject json)
        {
            NetmeraUser user = new NetmeraUser();

            if (json == null)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_NULL_EXCEPTION, "Json contains current user info is null");
            }
            try
            {
                if (json[NetmeraConstants.SecurityToken_Params] != null)
                {
                    securityToken = json[NetmeraConstants.SecurityToken_Params].ToString();
                }
                if (json[NetmeraConstants.Netmera_UserEmail] != null)
                {
                    user.email = json[NetmeraConstants.Netmera_UserEmail].ToString();
                }
                if (json[NetmeraConstants.Netmera_UserNickname] != null)
                {
                    user.nickname = json[NetmeraConstants.Netmera_UserNickname].ToString();
                }
                if (json[NetmeraConstants.Netmera_UserName] != null)
                {
                    user.name = json[NetmeraConstants.Netmera_UserName].ToString();
                }
                if (json[NetmeraConstants.Netmera_UserSurname] != null)
                {
                    user.surname = json[NetmeraConstants.Netmera_UserSurname].ToString();
                }
                currentUser = user;
            }
            catch (JsonException)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_EMAIL, "Json contains current user info is invalid");
            }
            return user;
        }
    }
}
