using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Xml;

using Board.Common;
using Board.Enums;

using Caliburn.Micro;

using PropertyChanged;

namespace Board.ViewModels
{
    [ImplementPropertyChanged]
    public class SettingViewModel : Screen
    {
        private readonly string _settingUri;
        private XmlDocument _xmlDocument;

        public string LogLevel { get; set; }

        public ObservableCollection<string> LogLevels { get; set; }

        public SettingViewModel()
        {
            LogLevel = LogHelper.LogLevel.ToString();

            LogLevels = new ObservableCollection<string>();
            foreach(var logLevel in Enum.GetValues(typeof(LogLevelEnum)))
            {
                LogLevels.Add(logLevel.ToString());
            }

            _settingUri = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Board.xml";
        }

        private void WriteLogLevel()
        {
            try
            {
                _xmlDocument = new XmlDocument();

                if(!File.Exists(_settingUri))
                {
                    var xmlDeclaration = _xmlDocument.CreateXmlDeclaration("1.0", "utf-8", "");
                    _xmlDocument.AppendChild(xmlDeclaration);
                    var boardElement = _xmlDocument.SelectSingleNode("Board");
                    if(boardElement == null)
                    {
                        boardElement = _xmlDocument.CreateElement("Board");
                        _xmlDocument.AppendChild(boardElement);
                    }

                    XmlElement xmlElement = _xmlDocument.CreateElement("LogLevel");
                    boardElement.AppendChild(xmlElement);

                    _xmlDocument.Save(_settingUri);
                }

                _xmlDocument.Load(_settingUri);

                var selectSingleNode = _xmlDocument.SelectSingleNode("Board");
                if(selectSingleNode != null)
                {
                    var xmlNode = selectSingleNode.SelectSingleNode("LogLevel");
                    if(xmlNode == null)
                    {
                        var boardElement = selectSingleNode;
                        XmlElement xmlElement = _xmlDocument.CreateElement("LogLevel");
                        xmlElement.SetAttribute("Value", LogLevel);
                        boardElement.AppendChild(xmlElement);
                        _xmlDocument.Save(_settingUri);
                    }
                    else
                    {
                        if(xmlNode.Attributes != null)
                        {
                            xmlNode.Attributes["Value"].Value = LogLevel;
                            _xmlDocument.Save(_settingUri);
                        }
                    }

                    switch(LogLevel)
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
            catch(Exception ex)
            {
                ShowMessage.Show("写入LogLevel出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to WriteLogLevel", ex.InnerException);
            }
            finally
            {
                if(LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "WriteLogLevel", null);
                }
            }
        }

        public void OkClick()
        {
            WriteLogLevel();

            TryClose();
        }

        public void CancelClick()
        {
            TryClose();
        }
    }
}
