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
using Newtonsoft.Json.Linq;
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

        bool newUser;
        bool generatedMail;

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
        /// <returns>Nickname of the user</returns>
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
        /// <returns>Name of the user</returns>
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
        /// Returns the status of the user. If it is newly created it returns <code> true </code>
        /// </summary>
        /// <returns>Status of the user</returns>
        public bool isNewUser()
        {
            return newUser;
        }

        private void setNewUser(bool newUser)
        {
            this.newUser = newUser;
        }

        /// <summary>
        /// Returns the mail status of the user. If it is auto-generated it returns <code> false </code>
        /// </summary>
        /// <returns>Status of the user</returns>

        public bool isGeneratedMail()
        {
            return generatedMail;
        }

        private void setGeneratedMail(bool generatedMail)
        {
            this.generatedMail = generatedMail;
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
            register(null);
        }

        /// <summary>
        /// Registers new user. Before calling this method email,password and	
        /// nickname fields of the <see cref="NetmeraUser"/> should be set. Those are
        /// the compulsory fields. There are also optional name and surname fields.
        /// </summary>
        /// <param name="callback">method called when user register operation finishes</param>
        public void register(Action<NetmeraUser, Exception> callback)
        {
            try
            {
                RequestItem registerUserReqItem = new RequestItem();
                registerUserReqItem.setEmail(email);
                registerUserReqItem.setPassword(password);
                registerUserReqItem.setNickname(nickname);
                registerUserReqItem.setName(name);
                registerUserReqItem.setSurname(surname);

                register(registerUserReqItem, (json, ex) =>
                {
                    if (json == null || ex != null)
                    {
                        if (callback != null)
                            callback(null, ex);
                    }
                    else
                    {
                        try
                        {
                            setUser(json);
                            if (callback != null)
                                callback(this, ex);
                        }
                        catch (NetmeraException e)
                        {
                            if (callback != null)
                                callback(null, e);
                        }
                    }
                });
            }
            catch (WebException)
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_REQUEST, "Request exception occurred while registering user"));
            }
            catch (IOException)
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_IO_EXCEPTION, "IO Exception occurred while registering user"));
            }
            catch (JsonException)
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_JSON, "Json in the response of register user method is invalid"));
            }
        }

        /// <summary>
        /// Updates user info. Before calling this method email,password and nickname
        /// fields of the <see cref="NetmeraUser"/> should be set. Those are the
        /// compulsory fields.
        /// </summary>
        public void update()
        {
            update(null);
        }

        /// <summary>
        /// Updates user info. Before calling this method email,password and nickname
        /// fields of the <see cref="NetmeraUser"/> should be set. Those are the
        /// compulsory fields.
        /// </summary>
        /// <param name="callback">Method called when user update operation finishes</param>
        public void update(Action<NetmeraUser, Exception> callback)
        {
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
                    profileUpdate(updateUserReqItem, (jsonp, ex) =>
                    {
                        if (jsonp == null || ex != null)
                        {
                            if (callback != null)
                                callback(null, ex);
                        }
                        else
                        {
                            try
                            {
                                setUser(jsonp);
                                if (callback != null)
                                    callback(this, ex);

                                if (password != null)
                                {
                                    accountUpdate(updateUserReqItem, (jsona, e) =>
                                    {
                                        if (callback != null)
                                            callback(this, e);
                                    });
                                }
                                if (callback != null)
                                    callback(this, ex);
                            }
                            catch (NetmeraException e)
                            {
                                if (callback != null)
                                    callback(null, e);
                            }
                        }
                    });
                }
                else if (password != null)
                {
                    accountUpdate(updateUserReqItem, (jsona, ex) =>
                    {
                        if (jsona == null || ex != null)
                        {
                            if (callback != null)
                                callback(null, ex);
                        }
                        else
                        {
                            try
                            {
                                setUser(jsona);
                                if (callback != null)
                                    callback(this, ex);
                            }
                            catch (NetmeraException e)
                            {
                                if (callback != null)
                                    callback(null, e);
                            }
                        }
                    });
                }
            }
            catch (WebException)
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_REQUEST, "Web exception occurred while updating user"));
            }
            catch (IOException)
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_IO_EXCEPTION, "IO Exception occurred while updating user"));
            }
            catch (JsonException)
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_JSON, "Json in the response of update user method is invalid"));
            }
        }

        /// <summary>
        /// Logs a guest into the application.
        /// </summary>
        public static void loginAsGuest()
        {
            loginAsGuestRequest(null);
        }

        /// <summary>
        /// Logs a guest into the application.
        /// </summary>
        /// <param name="callback">Method called when user login operation finishes</param>
        public static void loginAsGuest(Action<NetmeraUser, Exception> callback)
        {
            NetmeraUser user = new NetmeraUser();
            try
            {
                loginAsGuestRequest((json, ex) =>
                {
                    if (json == null || ex != null)
                    {
                        if (callback != null)
                            callback(null, ex);
                    }
                    else
                    {
                        try
                        {
                            user = setCurrentUser(json);
                            if (callback != null)
                                callback(user, ex);
                        }
                        catch (NetmeraException e)
                        {
                            if (callback != null)
                                callback(null, e);
                        }
                    }
                });
            }
            catch (WebException)
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_REQUEST, "Web exception occurred in user login method"));
            }
            catch (IOException)
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_IO_EXCEPTION, "IO Exception occurred in user login method"));
            }
            catch (JsonException)
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_JSON, "Json in the response of user login method is invalid"));
            }
        }

        /// <summary>
        /// Logs a user into the registered application. Email and password fields of
        /// user is used for this operation.
        /// </summary>
        /// <param name="email">Email of the user</param>
        /// <param name="password">Password of the user</param>
        public static void login(String email, String password)
        {
            login(email, password, null);
        }

        /// <summary>
        /// Logs a user into the registered application. Email and password fields of
        /// user is used for this operation.
        /// </summary>
        /// <param name="email">Email of the user</param>
        /// <param name="password">Password of the user</param>
        /// <param name="callback">Method called when user login operation finishes</param>
        public static void login(String email, String password, Action<NetmeraUser, Exception> callback)
        {
            clearSocialSessions((cleared, exception) =>
            {
                if (exception != null)
                {
                    if (callback != null)
                        callback(null, exception);
                }
                else if (cleared)
                {
                    NetmeraUser user = new NetmeraUser();
                    try
                    {
                        RequestItem loginUserReqItem = new RequestItem();
                        loginUserReqItem.setEmail(email);
                        loginUserReqItem.setPassword(password);

                        login(loginUserReqItem, (json, ex) =>
                        {
                            if (json == null || ex != null)
                            {
                                if (callback != null)
                                    callback(null, ex);
                            }
                            else
                            {
                                try
                                {
                                    user = setCurrentUser(json);
                                    if (callback != null)
                                        callback(user, ex);
                                }
                                catch (NetmeraException e)
                                {
                                    if (callback != null)
                                        callback(null, e);
                                }
                            }
                        });
                    }
                    catch (WebException)
                    {
                        if (callback != null)
                            callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_REQUEST, "Web exception occurred in user login method"));
                    }
                    catch (IOException)
                    {
                        if (callback != null)
                            callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_IO_EXCEPTION, "IO Exception occurred in user login method"));
                    }
                    catch (JsonException)
                    {
                        if (callback != null)
                            callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_JSON, "Json in the response of user login method is invalid"));
                    }
                }
                else
                {
                    if (callback != null)
                        callback(null, exception);
                }
            });
        }

        /// <summary>
        /// User logged out from the application
        /// </summary>
        public static void logout()
        {
            securityToken = null;
            currentUser = null;
        }


        internal static void clearSocialSessions()
        {
            NetmeraUser.logout();
            NetmeraTwitterUtils.clearTwitterSession(null);
            NetmeraFacebookUtils.clearFacebookSession(null);
        }

        internal static void clearSocialSessions(Action<Boolean, Exception> callback)
        {
            NetmeraUser.logout();
            NetmeraTwitterUtils.clearTwitterSession((logout, ex) =>
            {
                if (logout && ex == null)
                {
                    NetmeraFacebookUtils.clearFacebookSession((logout1, ex1) =>
                    {
                        if (logout1 && ex1 == null)
                        {
                            callback(logout1, ex1);
                        }
                        else
                        {
                            if (callback != null)
                                callback(false, ex1);
                        }
                    });
                }
                else
                {
                    if (callback != null)
                        callback(false, ex);
                }
            });

        }

        internal static void facebookRegister(RequestItem item, Action<JObject, Exception> callback)
        {
            if (String.IsNullOrEmpty(item.getFbId()))
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_REQUIRED_FIELD, NetmeraConstants.Netmera_UserFbId + " is required"));
            }
            if (String.IsNullOrEmpty(item.getNickname()))
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_REQUIRED_FIELD, NetmeraConstants.Netmera_UserNickname + " is required"));
            }
            if (String.IsNullOrEmpty(item.getName()))
            {
                item.setName(String.Empty);
            }
            if (String.IsNullOrEmpty(item.getSurname()))
            {
                item.setName(String.Empty);
            }
            if (String.IsNullOrEmpty(item.getEmail()))
            {
                item.setName(String.Empty);
            }
            NetmeraHttpUtils.facebookRegisterUser(item, (json, e) =>
            {
                if (json != null)
                {
                    if (json["entry"] != null && json["entry"].First != null)
                    {
                        if (callback != null)
                            callback((JObject)json.First.First, e);
                    }
                    else if (json["error"] != null)
                    {
                        String error = json["error"]["message"].ToString();
                        if (callback != null)
                            callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_USER_UPDATE_ERROR, error));
                    }
                    else
                    {
                        if (callback != null)
                            callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_USER_UPDATE_ERROR, "Error occurred while registering user"));
                    }
                }
                else
                {
                    if (callback != null)
                        callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_USER_UPDATE_ERROR, "Error occurred while registering user"));
                }
            });
        }

        internal static void twitterRegister(RequestItem item, Action<JObject, Exception> callback)
        {
            if (String.IsNullOrEmpty(item.getTwId()))
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_REQUIRED_FIELD, NetmeraConstants.Netmera_UserTwId + " is required"));
            }
            if (String.IsNullOrEmpty(item.getNickname()))
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_REQUIRED_FIELD, NetmeraConstants.Netmera_UserNickname + " is required"));
            }
            if (String.IsNullOrEmpty(item.getName()))
            {
                item.setName(String.Empty);
            }
            if (String.IsNullOrEmpty(item.getSurname()))
            {
                item.setName(String.Empty);
            }

            NetmeraHttpUtils.twitterRegisterUser(item, (json, e) =>
            {
                if (json != null)
                {
                    if (json["entry"] != null && json["entry"].First != null)
                    {
                        if (callback != null)
                            callback((JObject)json.First.First, e);
                    }
                    else if (json["error"] != null)
                    {
                        String error = json["error"]["message"].ToString();
                        if (callback != null)
                            callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_USER_UPDATE_ERROR, error));
                    }
                    else
                    {
                        if (callback != null)
                            callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_USER_UPDATE_ERROR, "Error occurred while registering user"));
                    }
                }
                else
                {
                    if (callback != null)
                        callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_USER_UPDATE_ERROR, "Error occurred while registering user"));
                }
            });
        }

        /// <summary>
        /// Activates the registered User.
        /// </summary>
        /// <param name="email">Email of the user</param>
        public void activateUser(String email)
        {
            activateUser(email, null);
        }

        private void activateUser(String email, Action<Exception> callback)
        {
            try
            {
                RequestItem activateUserReqItem = new RequestItem();
                activateUserReqItem.setEmail(email);

                activateUser(activateUserReqItem, ex =>
                {
                    if (callback != null)
                        callback(ex);
                });
            }
            catch (WebException)
            {
                if (callback != null)
                    callback(new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_REQUEST, "Request exception occurred while activating user"));
            }
            catch (IOException)
            {
                if (callback != null)
                    callback(new NetmeraException(NetmeraException.ErrorCode.EC_IO_EXCEPTION, "IO Exception occurred while activating user"));
            }
        }

        /// <summary>
        /// Deactivates the registered User.
        /// </summary>
        /// <param name="email">Email of the user</param>
        public void deactivateUser(String email)
        {
            deactivateUser(email, null);
        }

        private void deactivateUser(String email, Action<Exception> callback)
        {
            try
            {
                RequestItem deactivateUserReqItem = new RequestItem();
                deactivateUserReqItem.setEmail(email);

                deactivateUser(deactivateUserReqItem, ex =>
                {
                    if (callback != null)
                        callback(ex);
                });
            }
            catch (WebException)
            {
                if (callback != null)
                    callback(new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_REQUEST, "Request exception occurred while activating user"));
            }
            catch (IOException)
            {
                if (callback != null)
                    callback(new NetmeraException(NetmeraException.ErrorCode.EC_IO_EXCEPTION, "IO Exception occurred while activating user"));
            }
        }

        private void register(RequestItem item, Action<JObject, Exception> callback)
        {
            if (String.IsNullOrEmpty(item.getEmail()))
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_REQUIRED_FIELD, NetmeraConstants.Netmera_UserEmail + " is required"));
            }
            if (String.IsNullOrEmpty(item.getNickname()))
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_REQUIRED_FIELD, NetmeraConstants.Netmera_UserNickname + " is required"));
            }
            if (String.IsNullOrEmpty(item.getPassword()))
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_REQUIRED_FIELD, NetmeraConstants.Netmera_UserPassword + " is required"));
            }
            NetmeraHttpUtils.registerUser(item, (json, e) =>
            {
                if (json != null)
                {
                    if (json["entry"] != null && json["entry"].First != null)
                    {
                        if (callback != null)
                            callback((JObject)json.First.First, e);
                    }
                    else if (json["error"] != null)
                    {
                        String error = json["error"]["message"].ToString();
                        if (callback != null)
                            callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_USER_UPDATE_ERROR, error));
                    }
                    else
                    {
                        if (callback != null)
                            callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_USER_UPDATE_ERROR, "Error occurred while registering user"));
                    }
                }
                else
                {
                    if (callback != null)
                        callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_USER_UPDATE_ERROR, "Error occurred while registering user"));
                }
            });
        }

        private void profileUpdate(RequestItem item, Action<JObject, Exception> callback)
        {
            if (String.IsNullOrEmpty(item.getEmail()))
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_REQUIRED_FIELD, NetmeraConstants.Netmera_UserEmail + " is required"));
            }
            if (String.IsNullOrEmpty(item.getNickname()))
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_REQUIRED_FIELD, NetmeraConstants.Netmera_UserNickname + " is required"));
            }

            NetmeraHttpUtils.profileUpdate(item, (json, ex) =>
            {
                if (json != null)
                {
                    if (json != null)
                    {
                        if (json["data"] != null && json["data"].First != null)
                        {
                            if (callback != null)
                                callback((JObject)json.First.First, ex);
                        }
                        else if (json["error"] != null)
                        {
                            String error = json["error"]["message"].ToString();
                            if (callback != null)
                                callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_USER_UPDATE_ERROR, error));
                        }
                        else
                            if (callback != null)
                                callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_USER_UPDATE_ERROR, "Error occurred while updating user"));
                    }
                    else
                    {
                        if (callback != null)
                            callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_USER_UPDATE_ERROR, "Error occurred while updating user"));
                    }
                }
            });
        }

        private void accountUpdate(RequestItem item, Action<JObject, Exception> callback)
        {
            if (String.IsNullOrEmpty(item.getEmail()))
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_REQUIRED_FIELD, NetmeraConstants.Netmera_UserEmail + " is required"));
            }
            if (String.IsNullOrEmpty(item.getPassword()))
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_REQUIRED_FIELD, NetmeraConstants.Netmera_UserPassword + " is required"));
            }

            NetmeraHttpUtils.accountUpdate(item, (json, ex) =>
            {
                if (json != null)
                {
                    if (json["data"] != null && json["data"].First != null)
                    {
                        if (callback != null)
                            callback((JObject)json.First.First, ex);
                    }
                    else if (json["error"] != null)
                    {
                        String error = json["error"]["message"].ToString();
                        if (callback != null)
                            callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_USER_UPDATE_ERROR, error));
                    }
                    else
                        if (callback != null)
                            callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_USER_UPDATE_ERROR, "Error occurred while updating user"));
                }
                else
                {
                    if (callback != null)
                        callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_USER_UPDATE_ERROR, "Error occurred while updating user"));
                }
            });
        }

        private static void loginAsGuestRequest(Action<JObject, Exception> callback)
        {
            NetmeraHttpUtils.loginAsGuest((json, ex) =>
            {
                if (json != null)
                {
                    if (json["data"] != null && json["data"].First != null)
                    {
                        if (callback != null)
                            callback((JObject)json.First.First, ex);
                    }
                    else if (json["error"] != null)
                    {
                        String error = json["error"]["message"].ToString();
                        if (callback != null)
                            callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_USER_UPDATE_ERROR, error));
                    }
                    else
                        if (callback != null)
                            callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_USER_UPDATE_ERROR, "Error occurred while logging user"));
                }
                else
                {
                    if (callback != null)
                        callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_USER_LOGIN_ERROR, "Error occurred while logging user"));
                }
            });
        }

        private static void login(RequestItem item, Action<JObject, Exception> callback)
        {
            if (String.IsNullOrEmpty(item.getEmail()))
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_REQUIRED_FIELD, NetmeraConstants.Netmera_UserEmail + " is required"));
            }
            if (String.IsNullOrEmpty(item.getPassword()))
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_REQUIRED_FIELD, NetmeraConstants.Netmera_UserPassword + " is required"));
            }
            NetmeraHttpUtils.login(item, (json, ex) =>
            {
                if (json != null)
                {
                    if (json["data"] != null && json["data"].First != null)
                    {
                        if (callback != null)
                            callback((JObject)json.First.First, ex);
                    }
                    else if (json["error"] != null)
                    {
                        String error = json["error"]["message"].ToString();
                        if (callback != null)
                            callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_USER_UPDATE_ERROR, error));
                    }
                    else
                        if (callback != null)
                            callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_USER_UPDATE_ERROR, "Error occurred while logging user"));
                }
                else
                {
                    if (callback != null)
                        callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_USER_LOGIN_ERROR, "Error occurred while logging user"));
                }
            });
        }

        private void activateUser(RequestItem item, Action<Exception> callback)
        {
            if (String.IsNullOrEmpty(item.getEmail()))
            {
                if (callback != null)
                    callback(new NetmeraException(NetmeraException.ErrorCode.EC_REQUIRED_FIELD, NetmeraConstants.Netmera_UserEmail + " is required"));
            }
            NetmeraHttpUtils.activateUser(item, ex =>
            {
                if (callback != null)
                    callback(new NetmeraException(NetmeraException.ErrorCode.EC_USER_LOGIN_ERROR, "Error occurred while activating user"));
            });
        }

        private void deactivateUser(RequestItem item, Action<Exception> callback)
        {
            if (String.IsNullOrEmpty(item.getEmail()))
            {
                if (callback != null)
                    callback(new NetmeraException(NetmeraException.ErrorCode.EC_REQUIRED_FIELD, NetmeraConstants.Netmera_UserEmail + " is required"));
            }
            NetmeraHttpUtils.deactivateUser(item, ex =>
            {
                if (callback != null)
                    callback(new NetmeraException(NetmeraException.ErrorCode.EC_USER_LOGIN_ERROR, "Error occurred while deactivating user"));
            });
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
                    JObject emailObj = (JObject)json[NetmeraConstants.Netmera_UserEmails].First;

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
        internal static NetmeraUser setCurrentUser(JObject json)
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