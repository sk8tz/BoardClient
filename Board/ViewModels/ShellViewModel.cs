using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;

using Board.Common;
using Board.Enums;
using Board.Models.Users;
using Board.Resources.MessageResources;
using Board.Services.System;
using Board.Views;

using Caliburn.Micro;

using log4net.Appender;

using RestSharp;

namespace Board.ViewModels
{
    public class ShellViewModel : Screen, IShell
    {
        private readonly string _boardUri;
        private XmlDocument _xmlDocument;
        private static string _updaterModulePath;
        private readonly string _versionNumber = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        #region 属性

        public string VersionText { get; set; }

        public bool IsEnabled { get; set; }

        /// <summary>
        /// Email记录
        /// </summary>
        public ObservableCollection<string> EmailItems { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 错误提示
        /// </summary>
        public string ErrorMessage { get; set; }

        #endregion

        public ShellViewModel()
        {
            SetLogFilePosition();

            _boardUri = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Board.xml";
            IsEnabled = true;
            VersionText = "Version " + _versionNumber;

#if DEBUG
            Email = "gengxiaoliang@admaster.com.cn";
            Password = "462344147";
            //Email = "liguanghui@admaster.com.cn";
            //Password = "iswhat@821118";
#endif

#if Release
            StartSilent();
#endif
        }

        /// <summary>
        /// 设置日志文件位置
        /// </summary>
        private void SetLogFilePosition()
        {
            var repository = log4net.LogManager.GetRepository();
            var appenders = repository.GetAppenders();
            var targetApder = appenders.First(p => p.Name == "LogFileAppender") as FileAppender;
            if (targetApder != null)
            {
                targetApder.File = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/BoardLog.txt";
                targetApder.ActivateOptions();
            }
        }

        #region 方法

        /// <summary>
        /// 获取Token
        /// </summary>
        private async Task<string> GetToken()
        {
            var userInfo = new UserInfo { UserName = Email };
            User.UserInfo = userInfo;
            try
            {
                var tokenResult = await LoginService.GetToken(Email, Password);
                if (tokenResult.StatusCode == HttpStatusCode.OK) //200
                {
                    User.UserToken = tokenResult.Data;
                    ErrorMessage = string.Empty;
                }
                else if (((RestResponseBase)(tokenResult)).StatusCode == HttpStatusCode.NotFound) //404
                {
                    ErrorMessage = MessageResource.LoginNotFoundError;
                }
                else if ((int)tokenResult.StatusCode == 422)
                {
                    ErrorMessage = MessageResource.LoginInfoError;
                }
                else if (((RestResponseBase)(tokenResult)).StatusCode == HttpStatusCode.InternalServerError) //500
                {
                    ErrorMessage = MessageResource.LoginInternalServerError;
                }
                else
                {
                    ErrorMessage = MessageResource.LoginUnKnowError;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;

                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to GetToken", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "GetToken", null);
                }
            }
            return ErrorMessage;
        }

        /// <summary>
        /// 登录
        /// </summary>
        public async void Login()
        {
            ReadLogLevel();

            IsEnabled = false;
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = MessageResource.LoginEmptyError;
                IsEnabled = true;
                return;
            }

            if (!string.IsNullOrEmpty(await GetToken()))
            {
                IsEnabled = true;
                return;
            }

            try
            {
                var isAuthorizationResult = await LoginService.IsAuthorization(Email, _versionNumber);

                if (isAuthorizationResult.StatusCode == HttpStatusCode.OK) //200
                {
                    if (isAuthorizationResult.Data.Key == "NeedUpdate")
                    {
                        ErrorMessage = MessageResource.LoginNeedUpdate;
                        StartSilent();
                    }
                    else
                    {
                        if (isAuthorizationResult.Data.Value == bool.TrueString)
                        {
                            RecordEmail();

                            ErrorMessage = string.Empty;
                            var mainViewModel = IoC.Get<MainViewModel>();
                            if (mainViewModel != null)
                            {
                                IoC.Get<IWindowManager>().ShowWindow(mainViewModel);
                                TryClose();
                            }
                            else
                            {
                                ErrorMessage = MessageResource.LoginProgramError;
                            }
                        }
                        else
                        {
                            ErrorMessage = MessageResource.LoginNoAuthorizationError;
                        }
                    }
                }
                else if (isAuthorizationResult.StatusCode == HttpStatusCode.NotFound) //404
                {
                }
                else if ((int)isAuthorizationResult.StatusCode == 422)
                {
                }
                else if ((int)isAuthorizationResult.StatusCode == 0)
                {
                    ErrorMessage = MessageResource.LoginServerNoRun;
                }
                else if (isAuthorizationResult.StatusCode == HttpStatusCode.InternalServerError) //500
                {
                }
                IsEnabled = true;
            }
            catch (Exception ex)
            {
                IsEnabled = true;
                ErrorMessage = ex.Message;
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to Login", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Login", null);
                }
            }
        }

        /// <summary>
        /// 读取日志等级
        /// </summary>
        private void ReadLogLevel()
        {
            try
            {
                _xmlDocument = new XmlDocument();

                if (File.Exists(_boardUri))
                {
                    _xmlDocument.Load(_boardUri);
                    var selectSingleNode = _xmlDocument.SelectSingleNode("Board");
                    if (selectSingleNode != null)
                    {
                        var xmlNode = selectSingleNode.SelectSingleNode("LogLevel");
                        if (xmlNode == null)
                        {
                            var boardElement = selectSingleNode;
                            XmlElement xmlElement = _xmlDocument.CreateElement("LogLevel");
                            xmlElement.SetAttribute("Value", LogLevelEnum.Error.ToString());
                            boardElement.AppendChild(xmlElement);
                            _xmlDocument.Save(_boardUri);
                        }
                        else
                        {
                            if (xmlNode.Attributes != null)
                            {
                                switch (xmlNode.Attributes["Value"].Value)
                                {
                                    case "Debug":
                                        LogHelper.LogLevel = LogLevelEnum.Debug;
                                        break;
                                    case "Error":
                                        LogHelper.LogLevel = LogLevelEnum.Error;
                                        break;
                                    case "Fatal":
                                        LogHelper.LogLevel = LogLevelEnum.Fatal;
                                        break;
                                    case "Info":
                                        LogHelper.LogLevel = LogLevelEnum.Info;
                                        break;
                                    case "Warn":
                                        LogHelper.LogLevel = LogLevelEnum.Warn;
                                        break;
                                    default:
                                        LogHelper.LogLevel = LogLevelEnum.Error;
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to ReadLogLevel", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "ReadLogLevel", null);
                }
            }
        }

        /// <summary>
        /// 记录登录账号
        /// </summary>
        private void RecordEmail()
        {
            try
            {
                _xmlDocument = new XmlDocument();

                if (!File.Exists(_boardUri))
                {
                    var xmlDeclaration = _xmlDocument.CreateXmlDeclaration("1.0", "utf-8", "");
                    _xmlDocument.AppendChild(xmlDeclaration);
                    var boardElement = _xmlDocument.CreateElement("Board");
                    _xmlDocument.AppendChild(boardElement);
                    var xmlElement = _xmlDocument.CreateElement("LogLevel");
                    boardElement.AppendChild(xmlElement);
                    var recordsElement = _xmlDocument.CreateElement("Records");
                    boardElement.AppendChild(recordsElement);
                    _xmlDocument.Save(_boardUri);
                }

                EmailItems = new ObservableCollection<string>();
                _xmlDocument.Load(_boardUri);

                var selectSingleNode = _xmlDocument.SelectSingleNode("Board");
                if (selectSingleNode != null)
                {
                    var recordsNode = selectSingleNode.SelectSingleNode("Records");
                    if (recordsNode != null)
                    {
                        bool isExist = false;
                        foreach (var childNode in recordsNode.ChildNodes)
                        {
                            var email = ((XmlElement)childNode).Attributes["Value"].Value;
                            if (email == Email)
                            {
                                isExist = true;
                            }
                        }
                        if (!isExist && !string.IsNullOrEmpty(Email))
                        {
                            var xmlElement = _xmlDocument.CreateElement("Record");
                            xmlElement.SetAttribute("Value", Email);
                            recordsNode.AppendChild(xmlElement);
                            _xmlDocument.Save(_boardUri);
                        }
                    }

                    var logLevelNode = selectSingleNode.SelectSingleNode("LogLevel");
                    if (logLevelNode == null)
                    {
                        var boardElement = selectSingleNode;
                        XmlElement xmlElement = _xmlDocument.CreateElement("LogLevel");
                        xmlElement.SetAttribute("Value", LogHelper.LogLevel.ToString());
                        boardElement.AppendChild(xmlElement);
                        _xmlDocument.Save(_boardUri);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to RecordEmail", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "RecordEmail", null);
                }
            }
        }

        /// <summary>
        /// 加载登录过的账号
        /// </summary>
        public void LoadRecords()
        {
            if (EmailItems != null)
            {
                return;
            }

            try
            {
                _xmlDocument = new XmlDocument();

                if (!File.Exists(_boardUri))
                {
                    var xmlDeclaration = _xmlDocument.CreateXmlDeclaration("1.0", "utf-8", "");
                    _xmlDocument.AppendChild(xmlDeclaration);
                    var boardElement = _xmlDocument.CreateElement("Board");
                    _xmlDocument.AppendChild(boardElement);
                    var recordsElement = _xmlDocument.CreateElement("Records");
                    boardElement.AppendChild(recordsElement);
                    _xmlDocument.Save(_boardUri);
                }

                EmailItems = new ObservableCollection<string>();
                _xmlDocument.Load(_boardUri);

                var selectSingleNode = _xmlDocument.SelectSingleNode("Board");
                if (selectSingleNode != null)
                {
                    var xmlNode = selectSingleNode.SelectSingleNode("Records");
                    if (xmlNode != null)
                    {
                        foreach (var childNode in xmlNode.ChildNodes)
                        {
                            var email = ((XmlElement)childNode).Attributes["Value"].Value;
                            EmailItems.Add(email);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to LoadRecords", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "LoadRecords", null);
                }
            }
        }

        /// <summary>
        /// Email回车事件
        /// </summary>
        /// <param name="e"></param>
        public void EmailKeyEnter(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ShellView view = GetView() as ShellView;
                if (view != null)
                {
                    view.PasswordBox.Focus();
                }
            }
        }

        /// <summary>
        /// 密码回车事件
        /// </summary>
        /// <param name="e"></param>
        public void PasswordKeyEnter(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Login();
            }
        }

        /// <summary>
        /// 删除登录过的账号
        /// </summary>
        public void DeleteEmail(string emailString)
        {
            try
            {
                _xmlDocument = new XmlDocument();

                if (!File.Exists(_boardUri))
                {
                    var xmlDeclaration = _xmlDocument.CreateXmlDeclaration("1.0", "utf-8", "");
                    _xmlDocument.AppendChild(xmlDeclaration);
                    var boardElement = _xmlDocument.CreateElement("Board");
                    _xmlDocument.AppendChild(boardElement);
                    var xmlElement = _xmlDocument.CreateElement("LogLevel");
                    boardElement.AppendChild(xmlElement);
                    var recordsElement = _xmlDocument.CreateElement("Records");
                    boardElement.AppendChild(recordsElement);
                    _xmlDocument.Save(_boardUri);
                }

                _xmlDocument.Load(_boardUri);

                var selectSingleNode = _xmlDocument.SelectSingleNode("Board");
                if (selectSingleNode != null)
                {
                    var recordsNode = selectSingleNode.SelectSingleNode("Records");
                    if (recordsNode != null)
                    {
                        bool isExist = false;
                        foreach (var childNode in recordsNode.ChildNodes)
                        {
                            var email = ((XmlElement)childNode).Attributes["Value"].Value;
                            if (email == emailString)
                            {
                                recordsNode.RemoveChild((XmlElement)childNode);
                                break;
                            }
                        }

                        _xmlDocument.Save(_boardUri);
                        EmailItems.Remove(emailString);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to DeleteEmail", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "DeleteEmail", null);
                }
            }
        }

        public void EmailSelectedChanged()
        {
            Password = string.Empty;
        }

        public void EmailInput()
        {
            Password = string.Empty;
        }

        /// <summary>
        /// 检查更新
        /// </summary>
        private void StartSilent()
        {
            try
            {
                var aa = AppDomain.CurrentDomain.BaseDirectory;
                _updaterModulePath = Path.Combine(aa, "BoardUpdates.exe");
                Process process = Process.Start(_updaterModulePath, "/silent");
                if (process != null)
                {
                    process.Close();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to StartSilent", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "StartSilent", null);
                }
            }
        }

        /// <summary>
        /// 密码改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnPasswordBoxPasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordtext = (PasswordBox)sender;
            SetPasswordBoxSelection(passwordtext, passwordtext.Password.Length + 1, passwordtext.Password.Length + 1);
        }

        /// <summary>
        /// 设置密码框选中位置
        /// </summary>
        /// <param name="passwordBox"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        private static void SetPasswordBoxSelection(PasswordBox passwordBox, int start, int length)
        {
            var select = passwordBox.GetType().GetMethod("Select", BindingFlags.Instance | BindingFlags.NonPublic);
            select.Invoke(passwordBox, new object[] { start, length });
        }

        #endregion
    }
}