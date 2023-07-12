using FixExplorer.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using FixExplorer.Extensions;

namespace FixExplorer.Models
{
    public class FixMessage : NotificationObject
    {
        private readonly bool _showValueDescription;

        public FixMessage(bool showValueDescription)
        {
            _showValueDescription = showValueDescription;
        }

        private ObservableCollection<FixTag> _fixTags;
        public ObservableCollection<FixTag> FixTags
        {
            get { return _fixTags; }
            set
            {
                _fixTags = value;

                RaisePropertyChanged(() => FixTags);
            }
        }

        private string _message;
        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;

                RaisePropertyChanged(() => Message);
            }
        }

        private string _rawMessage;
        private string _mode;
        private string _msgType;
        private string _clOrdId;
        private string _ordStatus;
        private DateTime _sendingTime;

        public string RawMessage
        {
            get { return _rawMessage; }
            set
            {
                _rawMessage = value;

                // Search for 8=FIX
                var startPos = _rawMessage.IndexOf("8=FIX", StringComparison.Ordinal);
                if (startPos < 0)
                {
                    // don't parse it just set the raw
                    Message = _rawMessage;
                    return; // not a valid fix message
                }
                if (_rawMessage.IndexOf("<RX>", StringComparison.Ordinal) > -1)
                    Mode = "RX";
                else if (_rawMessage.IndexOf("<TX>", StringComparison.Ordinal) > -1)
                    Mode = "TX";
                else if (_rawMessage.IndexOf(" IN ", StringComparison.Ordinal) > -1)
                    Mode = "RX";
                else if (_rawMessage.IndexOf(" OUT ", StringComparison.Ordinal) > -1)
                    Mode = "TX";
                else
                    Mode = "RX";
                Message = _rawMessage.Substring(startPos);
                FixTags = new ObservableCollection<FixTag>();
                var tags = Message.Split((char)1);
                if (tags.Length == 1)
                {
                    tags = Message.Split('?');
                }
                var fixVersion = string.Empty;

                #region Tags
                var clOrdIdExist = false;
                foreach (var tag in tags)
                {
                    if (String.IsNullOrEmpty(tag)) continue;

                    var split = tag.Split('=');
                    if (split[0] == "8")
                        fixVersion = split[1];
                    var fixTag = new FixTag(_showValueDescription, fixVersion, split[0], split[1]);
                    FixTags.Add(fixTag);

                    switch (split[0])
                    {
                        case "11":
                            ClOrdID = split[1];
                            clOrdIdExist = true;
                            break;
                        case "131": // QuoteReqId for Message R and b
                        case "37":
                            if (!clOrdIdExist) ClOrdID = split[1];
                            break;
                        case "35":
                            switch (split[1])
                            {
                                case "b":
                                    MsgType = string.Format("({0}) MassQuoteAcknowledgement", split[1]);
                                    break;
                                case "A":
                                    MsgType = string.Format("({0}) Logon", split[1]);
                                    break;
                                case "D":
                                    MsgType = string.Format("({0}) NewOrderSingle", split[1]);
                                    break;
                                case "F":
                                    MsgType = string.Format("({0}) OrderCancelRequest", split[1]);
                                    break;
                                case "G":
                                    MsgType = string.Format("({0}) OrderCancelReplaceRequest", split[1]);
                                    break;
                                case "J":
                                    MsgType = string.Format("({0}) AllocationInstruction", split[1]);
                                    break;
                                case "P":
                                    MsgType = string.Format("({0}) AllocationInstructionAck", split[1]);
                                    break;
                                case "R":
                                    MsgType = string.Format("({0}) QuoteRequest", split[1]);
                                    break;
                                case "S":
                                    MsgType = string.Format("({0}) Quote", split[1]);
                                    break;
                                case "V":
                                    MsgType = string.Format("({0}) MarketDataRequest", split[1]);
                                    break;
                                case "W":
                                    MsgType = string.Format("({0}) MarketDataSnapshotFullRefresh", split[1]);
                                    break;
                                case "Y":
                                    MsgType = string.Format("({0}) MarketDataRequestReject", split[1]);
                                    break;
                                case "Z":
                                    MsgType = string.Format("({0}) QuoteCancel", split[1]);
                                    break;
                                case "0":
                                    MsgType = string.Format("({0}) Heartbeat", split[1]);
                                    break;
                                case "1":
                                    MsgType = string.Format("({0}) TestRequest", split[1]);
                                    break;
                                case "2":
                                    MsgType = string.Format("({0}) ResendRequest", split[1]);
                                    break;
                                case "3":
                                    MsgType = string.Format("({0}) Reject", split[1]);
                                    break;
                                case "4":
                                    MsgType = string.Format("({0}) SequenceReset", split[1]);
                                    break;
                                case "5":
                                    MsgType = string.Format("({0}) Logout", split[1]);
                                    break;
                                case "8":
                                    MsgType = string.Format("({0}) ExecutionReport", split[1]);
                                    break;
                                case "9":
                                    MsgType = string.Format("({0}) OrderCancelReject", split[1]);
                                    break;
                                default:
                                    MsgType = split[1];
                                    break;
                            }
                            break;
                        case "39":
                            switch (split[1])
                            {
                                case "0":
                                    OrdStatus = string.Format("({0}) New", split[1]);
                                    break;
                                case "1":
                                    OrdStatus = string.Format("({0}) Partially filled", split[1]);
                                    break;
                                case "2":
                                    OrdStatus = string.Format("({0}) Filled", split[1]);
                                    break;
                                case "3":
                                    OrdStatus = string.Format("({0}) Done for day", split[1]);
                                    break;
                                case "4":
                                    OrdStatus = string.Format("({0}) Cancelled", split[1]);
                                    break;
                                case "5":
                                    OrdStatus = string.Format("({0}) Replaced", split[1]);
                                    break;
                                case "6":
                                    OrdStatus = string.Format("({0}) Pending Cancel", split[1]);
                                    break;
                                case "7":
                                    OrdStatus = string.Format("({0}) Stopped", split[1]);
                                    break;
                                case "8":
                                    OrdStatus = string.Format("({0}) Rejected", split[1]);
                                    break;
                                case "9":
                                    OrdStatus = string.Format("({0}) Suspended", split[1]);
                                    break;
                                case "A":
                                    OrdStatus = string.Format("({0}) Pending New", split[1]);
                                    break;
                                case "B":
                                    OrdStatus = string.Format("({0}) Calculated", split[1]);
                                    break;
                                case "C":
                                    OrdStatus = string.Format("({0}) Expired", split[1]);
                                    break;
                                case "D":
                                    OrdStatus = string.Format("({0}) Accepted for bidding", split[1]);
                                    break;
                                case "E":
                                    OrdStatus = string.Format("({0}) Pending Replace", split[1]);
                                    break;
                                default:
                                    OrdStatus = split[1];
                                    break;
                            }
                            break;
                        case "52":
                            var sendingTime = split[1].ToDateTime();
                            SendingTime = sendingTime.UtcToLocalTime();
                            break;
                    }
                }
                #endregion

                // Sort out required field values
                string realMsgType = MsgType;
                var pos = MsgType.IndexOf(" ");
                if (pos > -1)
                    realMsgType = MsgType.Substring(pos + 1);
                IEnumerable<XElement> fixNodes = null;
                switch (fixVersion)
                {
                    case "FIX.4.2":
                        fixNodes = FixTag.Fix42Document().Elements("fix");
                        break;
                    case "FIX.4.4":
                        fixNodes = FixTag.Fix44Document().Elements("fix");
                        break;
                }
                if (fixNodes != null)
                {
                    foreach (var fields in from messageNode in fixNodes.Elements("messages").Elements()
                                           where messageNode.Attributes().Any(a => a.Name == "name" && a.Value == realMsgType)
                                           select messageNode.Elements())
                    {
                        if (realMsgType == "NewOrderSingle")
                        {
                            Console.Write("found");
                        }
                        foreach (var field in fields)
                        {
                            var fieldName = field.Attributes().FirstOrDefault(a => a.Name == "name");
                            if (fieldName == null) continue;

                            var fieldRequired = field.Attributes().FirstOrDefault(a => a.Name == "required");
                            if (fieldRequired == null) continue;

                            var name = fieldName.Value;
                            var required = fieldRequired.Value == "Y";
                            if (required && field.Name.LocalName == "component")
                            {
                                // need to find component and iterate through those fields
                                foreach (var component in from messageNode in fixNodes.Elements("components").Elements()
                                                          where messageNode.Attributes().Any(a => a.Name == "name" && a.Value == name)
                                                          select messageNode.Elements())
                                {
                                    foreach (var componentField in component)
                                    {
                                        var componentfieldName = componentField.Attributes().FirstOrDefault(a => a.Name == "name");
                                        if (componentfieldName == null) continue;

                                        var componentfieldRequired = componentField.Attributes().FirstOrDefault(a => a.Name == "required");
                                        if (componentfieldRequired == null) continue;

                                        var componentname = componentfieldName.Value;
                                        var componentrequired = componentfieldRequired.Value == "Y";
                                        UpdateFixTagRequired(componentname, componentrequired);
                                    }
                                }
                            }
                            else
                            {
                                UpdateFixTagRequired(name, required);
                            }
                        }
                        break;
                    }
                }

                RaisePropertyChanged(() => RawMessage);
            }
        }

        private void UpdateFixTagRequired(string name, bool required)
        {
            // find the fix tag
            var fixTag = FixTags.FirstOrDefault(a => a.Name == name);
            if (fixTag != null)
            {
                fixTag.Required = required;
            }

        }
        public DateTime SendingTime
        {
            get { return _sendingTime; }
            set { _sendingTime = value; RaisePropertyChanged(() => SendingTime); }
        }

        public string Mode
        {
            get { return _mode; }
            set { _mode = value; RaisePropertyChanged(() => Mode); }
        }

        public string MsgType
        {
            get { return _msgType; }
            set { _msgType = value; RaisePropertyChanged(() => MsgType); }
        }

        public string ClOrdID
        {
            get { return _clOrdId; }
            set { _clOrdId = value; RaisePropertyChanged(() => ClOrdID); }
        }

        public string OrdStatus
        {
            get { return _ordStatus; }
            set { _ordStatus = value; RaisePropertyChanged(() => OrdStatus); }
        }

    }

}
