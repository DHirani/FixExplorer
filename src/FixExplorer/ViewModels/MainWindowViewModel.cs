using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using FixExplorer.Extensions;
using FixExplorer.Models;
using FixExplorer.Utils;
using Microsoft.Win32;

namespace FixExplorer.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        public MainWindowViewModel()
        {
            FilesCollection = new ObservableCollection<string>();

            ShowLast500Message = true;
            ShowFixTagDescription = true;
        }

        #region Commands
        public ICommand OpenFileCommand { get { return new DelegateCommand(OnOpenFile); } }

        public ICommand RefreshCommand { get { return new DelegateCommand(OnRefresh); } }
        public ICommand ShowHeartbeatCommand { get { return new DelegateCommand(OnShowHeartbeat); } }
        public ICommand ShowFixTagDescriptionCommand { get { return new DelegateCommand(OnShowFixTagDescription); } }
        public ICommand OrderDoubleClickCommand { get { return new DelegateCommand(OnOrderDoubleClick); } }
        public ICommand ShowTestRequestCommand { get { return new DelegateCommand(OnShowTestRequest); } }
        public ICommand ShowLast500MessageCommand { get { return new DelegateCommand(OnShowLast500Message); } }
        public ICommand MainFixMessageCopyCommand { get { return new DelegateCommand(OnMainFixMessageCopy); } }
        public ICommand DetailFixMessageCopyCommand { get { return new DelegateCommand(OnDetailFixMessageCopy); } }

        #endregion

        private void OnOpenFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Log files (*.log)|*.log|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                FilesCollection.Add(openFileDialog.FileName);
                SelectedFile = openFileDialog.FileName;
            }
        }

        private void OnRefresh()
        {
            if (SelectedFile == null) return;

            string wholeFile;

            using (var fs = new FileStream(SelectedFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var reader = new StreamReader(fs))
                {
                    wholeFile = reader.ReadToEnd();
                }
            }

            var tempItems = wholeFile.Split('\n');
            string[] items;

            if (ShowLast500Message)
            {
                var itemList = new List<string>();
                foreach (var msg in tempItems)
                {
                    if (ShowHeartbeat && msg.IndexOf((char)1 + "35=0" + (char)1) > -1)
                        itemList.Add(msg);
                    else if (ShowTestRequest && msg.IndexOf((char)1 + "35=1" + (char)1) > -1)
                        itemList.Add(msg);
                    else if ((!ShowHeartbeat && msg.IndexOf((char)1 + "35=0" + (char)1) == -1) &&
                             (!ShowTestRequest && msg.IndexOf((char)1 + "35=1" + (char)1) == -1))
                        itemList.Add(msg);
                }

                var maxItem = 500;
                if (itemList.Count < maxItem)
                {
                    maxItem = itemList.Count;
                }
                else
                {
                    maxItem = itemList.Count - maxItem;
                }
                items = itemList.GetRange(itemList.Count < 500 ? 0 : maxItem - 1, itemList.Count < 500 ? maxItem : 500).ToArray();
            }
            else
                items = tempItems;

            FixMessagesCollection = new ObservableCollection<FixMessage>();
            foreach (var fixMessage in from item in items
                                       where !string.IsNullOrEmpty(item.Trim())
                                       select
                                           new FixMessage(ShowFixTagDescription)
                                           {
                                               RawMessage = item.Replace("\r", string.Empty)
                                           })
            {
                if (fixMessage.MsgType == "(0) Heartbeat" && ShowHeartbeat)
                    FixMessagesCollection.Add(fixMessage);
                else if (fixMessage.MsgType == "(1) TestRequest" && ShowTestRequest)
                    FixMessagesCollection.Add(fixMessage);
                else if (fixMessage.MsgType != "(0) Heartbeat" && fixMessage.MsgType != "(1) TestRequest")
                    FixMessagesCollection.Add(fixMessage);
            }

        }

        private void OnShowHeartbeat()
        {
            OnRefresh();
        }

        private void OnShowTestRequest()
        {
            OnRefresh();
        }

        private void OnShowFixTagDescription()
        {
            OnRefresh();
        }

        private bool _singleMode;
        private void OnOrderDoubleClick()
        {
            if (SelectedFixMessage == null) return;

            if (string.IsNullOrEmpty(OrderId(SelectedFixMessage.ClOrdID))) return;

            if (_singleMode)
            {
                // back to multi mode
                _singleMode = false;
                ShowAllOrder();
            }
            else
            {
                // back to single mode
                _singleMode = true;
                ShowSingleOrder();
                // hide ALL orders except this one
            }
        }

        private void OnShowLast500Message()
        {
            OnRefresh();
        }

        private void OnMainFixMessageCopy()
        {
            // copy the current selected message to clipboard
            if (SelectedFixMessage == null) return;

            var builder = new StringBuilder();

            // write only single message if single order is NOT chosen otherwise write every message
            builder.AppendLine("<html>");
            builder.AppendLine("<head>");
            builder.AppendLine("<style>");
            builder.AppendLine("<!--");
            builder.AppendLine(" /* Font Definitions */");
            builder.AppendLine(" @font-face");
            builder.AppendLine("	{font-family:Calibri;");
            builder.AppendLine("	panose-1:2 15 5 2 2 2 4 3 2 4;");
            builder.AppendLine("	mso-font-charset:0;");
            builder.AppendLine("	mso-generic-font-family:swiss;");
            builder.AppendLine("	mso-font-pitch:variable;");
            builder.AppendLine("	mso-font-signature:-520092929 1073786111 9 0 415 0;}");
            builder.AppendLine(" /* Style Definitions */");
            builder.AppendLine(" p.MsoNormal, li.MsoNormal, div.MsoNormal");
            builder.AppendLine("	{mso-style-unhide:no;");
            builder.AppendLine("	mso-style-qformat:yes;");
            builder.AppendLine("	mso-style-parent:\"\";");
            builder.AppendLine("	margin:0cm;");
            builder.AppendLine("	margin-bottom:.0001pt;");
            builder.AppendLine("	mso-pagination:widow-orphan;");
            builder.AppendLine("	font-size:11.0pt;");
            builder.AppendLine("	font-family:\"Calibri\",\"sans - serif\";");
            builder.AppendLine("	mso-fareast-font-family:Calibri;");
            builder.AppendLine("	mso-fareast-theme-font:minor-latin;");
            builder.AppendLine("	mso-fareast-language:EN-US;}");
            builder.AppendLine("span.SpellE");
            builder.AppendLine("	{mso-style-name:\"\";");
            builder.AppendLine("	mso-spl-e:yes;}");
            builder.AppendLine(".MsoChpDefault");
            builder.AppendLine("	{mso-style-type:export-only;");
            builder.AppendLine("	mso-default-props:yes;");
            builder.AppendLine("	font-family:\"Calibri\",\"sans - serif\";");
            builder.AppendLine("	mso-ascii-font-family:Calibri;");
            builder.AppendLine("	mso-ascii-theme-font:minor-latin;");
            builder.AppendLine("	mso-fareast-font-family:Calibri;");
            builder.AppendLine("	mso-fareast-theme-font:minor-latin;");
            builder.AppendLine("	mso-hansi-font-family:Calibri;");
            builder.AppendLine("	mso-hansi-theme-font:minor-latin;");
            builder.AppendLine("	mso-bidi-font-family:\"Times New Roman\";");
            builder.AppendLine("	mso-bidi-theme-font:minor-bidi;");
            builder.AppendLine("	mso-fareast-language:EN-US;}");
            builder.AppendLine(".MsoPapDefault");
            builder.AppendLine("	{mso-style-type:export-only;");
            builder.AppendLine("	margin-bottom:10.0pt;");
            builder.AppendLine("	line-height:115%;}");
            builder.AppendLine("@page WordSection1");
            builder.AppendLine("	{size:595.3pt 841.9pt;");
            builder.AppendLine("	margin:72.0pt 72.0pt 72.0pt 72.0pt;");
            builder.AppendLine("	mso-header-margin:35.4pt;");
            builder.AppendLine("	mso-footer-margin:35.4pt;");
            builder.AppendLine("	mso-paper-source:0;}");
            builder.AppendLine("div.WordSection1");
            builder.AppendLine("	{page:WordSection1;}");
            builder.AppendLine("-->");
            builder.AppendLine("</style>");
            builder.AppendLine("</head>");
            builder.AppendLine("<body lang=EN-GB style='tab-interval:36.0pt'>");
            builder.AppendLine("<div class=WordSection1>");
            builder.AppendLine("<table class=MsoTableMediumGrid3Accent4 border=1 cellspacing=0 cellpadding=0");
            builder.AppendLine(" style='border-collapse:collapse;border:none;mso-border-alt:solid white 1.0pt;");
            builder.AppendLine(" mso-border-themecolor:background1;mso-yfti-tbllook:1056;mso-padding-alt:0cm 5.4pt 0cm 5.4pt'>");
            builder.AppendLine(" <tr style='mso-yfti-irow:-1;mso-yfti-firstrow:yes'>");
            builder.AppendLine("  <td width=153 valign=top style='width:115.0pt;border:solid white 1.0pt;");
            builder.AppendLine("  mso-border-themecolor:background1;border-bottom:solid white 3.0pt;mso-border-bottom-themecolor:");
            builder.AppendLine("  background1;background:#8064A2;mso-background-themecolor:accent4;padding:");
            builder.AppendLine("  0cm 5.4pt 0cm 5.4pt'>");
            builder.AppendLine("  <p class=MsoNormal style='mso-yfti-cnfc:1'><b style='mso-bidi-font-weight:");
            builder.AppendLine("  normal'><span style='color:white;mso-themecolor:background1'>Time<span");
            builder.AppendLine("  style='mso-bidi-font-weight:bold'><o:p></o:p></span></span></b></p>");
            builder.AppendLine("  </td>");
            builder.AppendLine("  <td width=74 valign=top style='width:55.85pt;border-top:solid white 1.0pt;");
            builder.AppendLine("  mso-border-top-themecolor:background1;border-left:none;border-bottom:solid white 3.0pt;");
            builder.AppendLine("  mso-border-bottom-themecolor:background1;border-right:solid white 1.0pt;");
            builder.AppendLine("  mso-border-right-themecolor:background1;mso-border-left-alt:solid white 1.0pt;");
            builder.AppendLine("  mso-border-left-themecolor:background1;background:#8064A2;mso-background-themecolor:");
            builder.AppendLine("  accent4;padding:0cm 5.4pt 0cm 5.4pt'>");
            builder.AppendLine("  <p class=MsoNormal style='mso-yfti-cnfc:1'><b style='mso-bidi-font-weight:");
            builder.AppendLine("  normal'><span style='color:white;mso-themecolor:background1'>Mode<span");
            builder.AppendLine("  style='mso-bidi-font-weight:bold'><o:p></o:p></span></span></b></p>");
            builder.AppendLine("  </td>");
            builder.AppendLine("  <td width=189 valign=top style='width:141.9pt;border-top:solid white 1.0pt;");
            builder.AppendLine("  mso-border-top-themecolor:background1;border-left:none;border-bottom:solid white 3.0pt;");
            builder.AppendLine("  mso-border-bottom-themecolor:background1;border-right:solid white 1.0pt;");
            builder.AppendLine("  mso-border-right-themecolor:background1;mso-border-left-alt:solid white 1.0pt;");
            builder.AppendLine("  mso-border-left-themecolor:background1;background:#8064A2;mso-background-themecolor:");
            builder.AppendLine("  accent4;padding:0cm 5.4pt 0cm 5.4pt'>");
            builder.AppendLine("  <p class=MsoNormal style='mso-yfti-cnfc:1'><span class=SpellE><b");
            builder.AppendLine("  style='mso-bidi-font-weight:normal'><span style='color:white;mso-themecolor:");
            builder.AppendLine("  background1'>MsgType</span></b></span><b style='mso-bidi-font-weight:normal'><span");
            builder.AppendLine("  style='color:white;mso-themecolor:background1'><o:p></o:p></span></b></p>");
            builder.AppendLine("  </td>");
            builder.AppendLine("  <td width=99 valign=top style='width:74.1pt;border-top:solid white 1.0pt;");
            builder.AppendLine("  mso-border-top-themecolor:background1;border-left:none;border-bottom:solid white 3.0pt;");
            builder.AppendLine("  mso-border-bottom-themecolor:background1;border-right:solid white 1.0pt;");
            builder.AppendLine("  mso-border-right-themecolor:background1;mso-border-left-alt:solid white 1.0pt;");
            builder.AppendLine("  mso-border-left-themecolor:background1;background:#8064A2;mso-background-themecolor:");
            builder.AppendLine("  accent4;padding:0cm 5.4pt 0cm 5.4pt'>");
            builder.AppendLine("  <p class=MsoNormal style='mso-yfti-cnfc:1'><b style='mso-bidi-font-weight:");
            builder.AppendLine("  normal'><span style='color:white;mso-themecolor:background1'>Key<o:p></o:p></span></b></p>");
            builder.AppendLine("  </td>");
            builder.AppendLine("  <td width=100 valign=top style='width:75.25pt;border-top:solid white 1.0pt;");
            builder.AppendLine("  mso-border-top-themecolor:background1;border-left:none;border-bottom:solid white 3.0pt;");
            builder.AppendLine("  mso-border-bottom-themecolor:background1;border-right:solid white 1.0pt;");
            builder.AppendLine("  mso-border-right-themecolor:background1;mso-border-left-alt:solid white 1.0pt;");
            builder.AppendLine("  mso-border-left-themecolor:background1;background:#8064A2;mso-background-themecolor:");
            builder.AppendLine("  accent4;padding:0cm 5.4pt 0cm 5.4pt'>");
            builder.AppendLine("  <p class=MsoNormal style='mso-yfti-cnfc:1'><b style='mso-bidi-font-weight:");
            builder.AppendLine("  normal'><span style='color:white;mso-themecolor:background1'>Status<o:p></o:p></span></b></p>");
            builder.AppendLine("  </td>");
            builder.AppendLine(" </tr>");
            builder.AppendLine(" <tr style='mso-yfti-irow:0'>");
            var fixMessages = new List<FixMessage>();

            if (_singleMode)
            {
                // show all the messages for this order
                var view = CollectionViewSource.GetDefaultView(FixMessagesCollection);
                var enumurator = view.GetEnumerator();
                while (enumurator.MoveNext())
                {
                    fixMessages.Add((FixMessage)enumurator.Current);
                }
            }
            else
            {
                // just the selected item
                fixMessages.Add(SelectedFixMessage);
            }
            foreach (var fixMessage in fixMessages)
            {
                // time
                builder.AppendLine("  <td width=153 valign=top style='width:115.0pt;border:solid white 1.0pt;");
                builder.AppendLine(
                    "  mso-border-themecolor:background1;border-top:none;mso-border-top-alt:solid white 1.0pt;");
                builder.AppendLine(
                    "  mso-border-top-themecolor:background1;background:#BFB1D0;mso-background-themecolor:");
                builder.AppendLine("  accent4;mso-background-themetint:127;padding:0cm 5.4pt 0cm 5.4pt'>");
                builder.AppendFormat("  <p class=MsoNormal style='mso-yfti-cnfc:64'>{0:yyyy-MM-dd HH:mm:ss:fff}<o:p></o:p></p>\r\n", fixMessage.SendingTime.ToUniversalTime());
                builder.AppendLine("  </td>");
                // Mode
                builder.AppendLine("  <td width=74 valign=top style='width:55.85pt;border-top:none;border-left:");
                builder.AppendLine("  none;border-bottom:solid white 1.0pt;mso-border-bottom-themecolor:background1;");
                builder.AppendLine("  border-right:solid white 1.0pt;mso-border-right-themecolor:background1;");
                builder.AppendLine("  mso-border-top-alt:solid white 1.0pt;mso-border-top-themecolor:background1;");
                builder.AppendLine("  mso-border-left-alt:solid white 1.0pt;mso-border-left-themecolor:background1;");
                builder.AppendLine("  background:#BFB1D0;mso-background-themecolor:accent4;mso-background-themetint:");
                builder.AppendLine("  127;padding:0cm 5.4pt 0cm 5.4pt'>");
                builder.AppendFormat("  <p class=MsoNormal style='mso-yfti-cnfc:64'>{0}<o:p></o:p></p>\r\n", fixMessage.Mode);
                builder.AppendLine("  </td>");
                // MsgType
                builder.AppendLine("  <td width=189 valign=top style='width:141.9pt;border-top:none;border-left:");
                builder.AppendLine("  none;border-bottom:solid white 1.0pt;mso-border-bottom-themecolor:background1;");
                builder.AppendLine("  border-right:solid white 1.0pt;mso-border-right-themecolor:background1;");
                builder.AppendLine("  mso-border-top-alt:solid white 1.0pt;mso-border-top-themecolor:background1;");
                builder.AppendLine("  mso-border-left-alt:solid white 1.0pt;mso-border-left-themecolor:background1;");
                builder.AppendLine("  background:#BFB1D0;mso-background-themecolor:accent4;mso-background-themetint:");
                builder.AppendLine("  127;padding:0cm 5.4pt 0cm 5.4pt'>");
                builder.AppendFormat(
                    "  <p class=MsoNormal style='mso-yfti-cnfc:64'><span class=SpellE>{0}</span><o:p></o:p></p>\r\n", fixMessage.MsgType);
                builder.AppendLine("  </td>");
                // Key
                builder.AppendLine("  <td width=99 valign=top style='width:74.1pt;border-top:none;border-left:none;");
                builder.AppendLine("  border-bottom:solid white 1.0pt;mso-border-bottom-themecolor:background1;");
                builder.AppendLine("  border-right:solid white 1.0pt;mso-border-right-themecolor:background1;");
                builder.AppendLine("  mso-border-top-alt:solid white 1.0pt;mso-border-top-themecolor:background1;");
                builder.AppendLine("  mso-border-left-alt:solid white 1.0pt;mso-border-left-themecolor:background1;");
                builder.AppendLine("  background:#BFB1D0;mso-background-themecolor:accent4;mso-background-themetint:");
                builder.AppendLine("  127;padding:0cm 5.4pt 0cm 5.4pt'>");
                builder.AppendFormat("  <p class=MsoNormal style='mso-yfti-cnfc:64'>{0}<o:p></o:p></p>\r\n", fixMessage.ClOrdID);
                builder.AppendLine("  </td>");
                // Status
                builder.AppendLine("  <td width=100 valign=top style='width:75.25pt;border-top:none;border-left:");
                builder.AppendLine("  none;border-bottom:solid white 1.0pt;mso-border-bottom-themecolor:background1;");
                builder.AppendLine("  border-right:solid white 1.0pt;mso-border-right-themecolor:background1;");
                builder.AppendLine("  mso-border-top-alt:solid white 1.0pt;mso-border-top-themecolor:background1;");
                builder.AppendLine("  mso-border-left-alt:solid white 1.0pt;mso-border-left-themecolor:background1;");
                builder.AppendLine("  background:#BFB1D0;mso-background-themecolor:accent4;mso-background-themetint:");
                builder.AppendLine("  127;padding:0cm 5.4pt 0cm 5.4pt'>");
                builder.AppendFormat("  <p class=MsoNormal style='mso-yfti-cnfc:64'>{0}<o:p></o:p></p>", fixMessage.OrdStatus);
                builder.AppendLine("  </td>");
                builder.AppendLine(" </tr>");
                // Raw message
                builder.AppendLine(" <tr style='mso-yfti-irow:1'>");
                builder.AppendLine("  <td width=616 colspan=5 valign=top style='width:462.1pt;border:solid white 1.0pt;");
                builder.AppendLine(
                    "  mso-border-themecolor:background1;border-top:none;mso-border-top-alt:solid white .75pt;");
                builder.AppendLine(
                    "  mso-border-top-themecolor:background1;mso-border-top-alt:.75pt;mso-border-left-alt:");
                builder.AppendLine(
                    "  1.0pt;mso-border-bottom-alt:.75pt;mso-border-right-alt:1.0pt;mso-border-color-alt:");
                builder.AppendLine("  white;mso-border-themecolor:background1;mso-border-style-alt:solid;");
                builder.AppendLine("  background:#DFD8E8;mso-background-themecolor:accent4;mso-background-themetint:");
                builder.AppendLine("  63;padding:0cm 5.4pt 0cm 5.4pt'>");
                builder.AppendFormat("  <p class=MsoNormal><span style='font-size:10.0pt'>{0}<o:p></o:p></span></p>\r\n", fixMessage.RawMessage.Replace("\u0001","|"));
                builder.AppendLine("  </td>");
                builder.AppendLine(" </tr>");
            }
            builder.AppendLine("</table>");
            builder.AppendLine("</div>");
            builder.AppendLine("</body>");
            builder.AppendLine("</html>");

            CopyHtmlToClipboard(builder.ToString());
        }


        private void OnDetailFixMessageCopy()
        {
            if (SelectedFixMessage == null) return;

            var builder = new StringBuilder();
            builder.AppendLine("<html>");
            builder.AppendLine("<head>");
            builder.AppendLine("<style>");
            builder.AppendLine("<!--");
            builder.AppendLine(" /* Font Definitions */");
            builder.AppendLine(" @font-face");
            builder.AppendLine("	{font-family:Calibri;");
            builder.AppendLine("	panose-1:2 15 5 2 2 2 4 3 2 4;");
            builder.AppendLine("	mso-font-charset:0;");
            builder.AppendLine("	mso-generic-font-family:swiss;");
            builder.AppendLine("	mso-font-pitch:variable;");
            builder.AppendLine("	mso-font-signature:-520092929 1073786111 9 0 415 0;}");
            builder.AppendLine(" /* Style Definitions */");
            builder.AppendLine(" p.MsoNormal, li.MsoNormal, div.MsoNormal");
            builder.AppendLine("	{mso-style-unhide:no;");
            builder.AppendLine("	mso-style-qformat:yes;");
            builder.AppendLine("	mso-style-parent:\"\";");
            builder.AppendLine("	margin:0cm;");
            builder.AppendLine("	margin-bottom:.0001pt;");
            builder.AppendLine("	mso-pagination:widow-orphan;");
            builder.AppendLine("	font-size:11.0pt;");
            builder.AppendLine("	font-family:\"Calibri\",\"sans - serif\";");
            builder.AppendLine("	mso-fareast-font-family:Calibri;");
            builder.AppendLine("	mso-fareast-theme-font:minor-latin;");
            builder.AppendLine("	mso-fareast-language:EN-US;}");
            builder.AppendLine(".MsoChpDefault");
            builder.AppendLine("	{mso-style-type:export-only;");
            builder.AppendLine("	mso-default-props:yes;");
            builder.AppendLine("	font-family:\"Calibri\",\"sans - serif\";");
            builder.AppendLine("	mso-ascii-font-family:Calibri;");
            builder.AppendLine("	mso-ascii-theme-font:minor-latin;");
            builder.AppendLine("	mso-fareast-font-family:Calibri;");
            builder.AppendLine("	mso-fareast-theme-font:minor-latin;");
            builder.AppendLine("	mso-hansi-font-family:Calibri;");
            builder.AppendLine("	mso-hansi-theme-font:minor-latin;");
            builder.AppendLine("	mso-bidi-font-family:\"Times New Roman\";");
            builder.AppendLine("	mso-bidi-theme-font:minor-bidi;");
            builder.AppendLine("	mso-fareast-language:EN-US;}");
            builder.AppendLine(".MsoPapDefault");
            builder.AppendLine("	{mso-style-type:export-only;");
            builder.AppendLine("	margin-bottom:10.0pt;");
            builder.AppendLine("	line-height:115%;}");
            builder.AppendLine("@page WordSection1");
            builder.AppendLine("	{size:595.3pt 841.9pt;");
            builder.AppendLine("	margin:72.0pt 72.0pt 72.0pt 72.0pt;");
            builder.AppendLine("	mso-header-margin:35.4pt;");
            builder.AppendLine("	mso-footer-margin:35.4pt;");
            builder.AppendLine("	mso-paper-source:0;}");
            builder.AppendLine("div.WordSection1");
            builder.AppendLine("	{page:WordSection1;}");
            builder.AppendLine("-->");
            builder.AppendLine("</style>");
            builder.AppendLine("</head>");
            builder.AppendLine("");
            builder.AppendLine("<body lang=EN-GB style='tab-interval:36.0pt'>");
            builder.AppendLine("");
            builder.AppendLine("<div class=WordSection1>");
            builder.AppendLine("");
            builder.AppendLine("<table class=MsoTableMediumGrid3Accent1 border=1 cellspacing=0 cellpadding=0");
            builder.AppendLine(" style='border-collapse:collapse;border:none;mso-border-alt:solid white 1.0pt;");
            builder.AppendLine(" mso-border-themecolor:background1;mso-yfti-tbllook:1056;mso-padding-alt:0cm 5.4pt 0cm 5.4pt'>");


            builder.AppendLine("<tr style='mso-yfti-irow:0;mso-yfti-firstrow:yes'>");
            // fix tag
            builder.AppendLine("  <td width=177 valign=top style='width:133.0pt;border:solid white 1.0pt;");
            builder.AppendLine("  mso-border-themecolor:background1;border-bottom:solid white 3.0pt;mso-border-bottom-themecolor:");
            builder.AppendLine("  background1;background:#4F81BD;mso-background-themecolor:accent1;padding:");
            builder.AppendLine("  0cm 5.4pt 0cm 5.4pt'>");
            builder.AppendLine("  <p class=MsoNormal style='margin-bottom:0cm;margin-bottom:.0001pt;line-height:");
            builder.AppendLine("  normal'><b><span style='mso-ascii-font-family:Calibri;mso-hansi-font-family:");
            builder.AppendLine("  Calibri;mso-bidi-font-family:Calibri;color:white;mso-themecolor:background1'>Fix");
            builder.AppendLine("  Tag</span></b><b style='mso-bidi-font-weight:normal'><span style='mso-ascii-font-family:");
            builder.AppendLine("  Calibri;mso-hansi-font-family:Calibri;mso-bidi-font-family:Calibri'><o:p></o:p></span></b></p>");
            builder.AppendLine("  </td>");
            // fix name
            builder.AppendLine("  <td width=177 valign=top style='width:133.0pt;border:solid white 1.0pt;");
            builder.AppendLine("  mso-border-themecolor:background1;border-bottom:solid white 3.0pt;mso-border-bottom-themecolor:");
            builder.AppendLine("  background1;background:#4F81BD;mso-background-themecolor:accent1;padding:");
            builder.AppendLine("  0cm 5.4pt 0cm 5.4pt'>");
            builder.AppendLine("  <p class=MsoNormal style='margin-bottom:0cm;margin-bottom:.0001pt;line-height:");
            builder.AppendLine("  normal'><b><span style='mso-ascii-font-family:Calibri;mso-hansi-font-family:");
            builder.AppendLine("  Calibri;mso-bidi-font-family:Calibri;color:white;mso-themecolor:background1'>Fix");
            builder.AppendLine("  Name</span></b><b style='mso-bidi-font-weight:normal'><span style='mso-ascii-font-family:");
            builder.AppendLine("  Calibri;mso-hansi-font-family:Calibri;mso-bidi-font-family:Calibri'><o:p></o:p></span></b></p>");
            builder.AppendLine("  </td>");
            // fix value
            builder.AppendLine("  <td width=439 valign=top style='width:329.1pt;border-top:solid white 1.0pt;");
            builder.AppendLine("  mso-border-top-themecolor:background1;border-left:none;border-bottom:solid white 3.0pt;");
            builder.AppendLine("  mso-border-bottom-themecolor:background1;border-right:solid white 1.0pt;");
            builder.AppendLine("  mso-border-right-themecolor:background1;mso-border-left-alt:solid white 1.0pt;");
            builder.AppendLine("  mso-border-left-themecolor:background1;background:#4F81BD;mso-background-themecolor:");
            builder.AppendLine("  accent1;padding:0cm 5.4pt 0cm 5.4pt'>");
            builder.AppendLine("  <p class=MsoNormal style='margin-bottom:0cm;margin-bottom:.0001pt;line-height:");
            builder.AppendLine("  normal'><b><span style='mso-ascii-font-family:Calibri;mso-hansi-font-family:");
            builder.AppendLine("  Calibri;mso-bidi-font-family:Calibri;color:white;mso-themecolor:background1'>Fix");
            builder.AppendLine("  Value</span></b><b style='mso-bidi-font-weight:normal'><span");
            builder.AppendLine("  style='mso-ascii-font-family:Calibri;mso-hansi-font-family:Calibri;");
            builder.AppendLine("  mso-bidi-font-family:Calibri'><o:p></o:p></span></b></p>");
            builder.AppendLine("  </td>");
            // description
            builder.AppendLine("  <td width=177 valign=top style='width:133.0pt;border:solid white 1.0pt;");
            builder.AppendLine("  mso-border-themecolor:background1;border-bottom:solid white 3.0pt;mso-border-bottom-themecolor:");
            builder.AppendLine("  background1;background:#4F81BD;mso-background-themecolor:accent1;padding:");
            builder.AppendLine("  0cm 5.4pt 0cm 5.4pt'>");
            builder.AppendLine("  <p class=MsoNormal style='margin-bottom:0cm;margin-bottom:.0001pt;line-height:");
            builder.AppendLine("  normal'><b><span style='mso-ascii-font-family:Calibri;mso-hansi-font-family:");
            builder.AppendLine("  Calibri;mso-bidi-font-family:Calibri;color:white;mso-themecolor:background1'>Description</span>");
            builder.AppendLine("  </b><b style='mso-bidi-font-weight:normal'><span style='mso-ascii-font-family:");
            builder.AppendLine("  Calibri;mso-hansi-font-family:Calibri;mso-bidi-font-family:Calibri'><o:p></o:p></span></b></p>");
            builder.AppendLine("  </td>");
            builder.AppendLine(" </tr>");


            for (var t = 0; t < SelectedFixMessage.FixTags.Count; t++)
            {
                var tag = SelectedFixMessage.FixTags[t];

                if (t % 2 != 0) // not even number
                {
                    builder.AppendLine(" <tr style='mso-yfti-irow:0'>");
                    // fix tag
                    builder.AppendLine("  <td width=177 valign=top style='width:133.0pt;border:solid white 1.0pt;");
                    builder.AppendLine("  mso-border-themecolor:background1;border-top:none;mso-border-top-alt:solid white 1.0pt;");
                    builder.AppendLine("  mso-border-top-themecolor:background1;background:#A7BFDE;mso-background-themecolor:");
                    builder.AppendLine("  accent1;mso-background-themetint:127;padding:0cm 5.4pt 0cm 5.4pt'>");
                    builder.AppendFormat("  <p class=MsoNormal style='mso-yfti-cnfc:64'>{0}<o:p></o:p></p>\r\n", tag.Tag);
                    builder.AppendLine("  </td>");
                    // fix name
                    builder.AppendLine("  <td width=177 valign=top style='width:133.0pt;border:solid white 1.0pt;");
                    builder.AppendLine("  mso-border-themecolor:background1;border-top:none;mso-border-top-alt:solid white 1.0pt;");
                    builder.AppendLine("  mso-border-top-themecolor:background1;background:#A7BFDE;mso-background-themecolor:");
                    builder.AppendLine("  accent1;mso-background-themetint:127;padding:0cm 5.4pt 0cm 5.4pt'>");
                    builder.AppendFormat("  <p class=MsoNormal style='mso-yfti-cnfc:64'>{0}<o:p></o:p></p>\r\n", tag.Name);
                    builder.AppendLine("  </td>");
                    // fix value
                    builder.AppendLine("  <td width=439 valign=top style='width:329.1pt;border-top:none;border-left:");
                    builder.AppendLine("  none;border-bottom:solid white 1.0pt;mso-border-bottom-themecolor:background1;");
                    builder.AppendLine("  border-right:solid white 1.0pt;mso-border-right-themecolor:background1;");
                    builder.AppendLine("  mso-border-top-alt:solid white 1.0pt;mso-border-top-themecolor:background1;");
                    builder.AppendLine("  mso-border-left-alt:solid white 1.0pt;mso-border-left-themecolor:background1;");
                    builder.AppendLine("  background:#A7BFDE;mso-background-themecolor:accent1;mso-background-themetint:");
                    builder.AppendLine("  127;padding:0cm 5.4pt 0cm 5.4pt'>");
                    builder.AppendFormat("  <p class=MsoNormal style='mso-yfti-cnfc:64'>{0}<o:p></o:p></p>\r\n", tag.Value);
                    builder.AppendLine("  </td>");
                    // description
                    builder.AppendLine("  <td width=177 valign=top style='width:133.0pt;border:solid white 1.0pt;");
                    builder.AppendLine("  mso-border-themecolor:background1;border-top:none;mso-border-top-alt:solid white 1.0pt;");
                    builder.AppendLine("  mso-border-top-themecolor:background1;background:#A7BFDE;mso-background-themecolor:");
                    builder.AppendLine("  accent1;mso-background-themetint:127;padding:0cm 5.4pt 0cm 5.4pt'>");
                    builder.AppendFormat("  <p class=MsoNormal style='mso-yfti-cnfc:64'>{0}<o:p></o:p></p>\r\n", tag.Description);
                    builder.AppendLine("  </td>");
                    builder.AppendLine(" </tr>");
                }
                else
                {
                    builder.AppendLine(" <tr style='mso-yfti-irow:1'>");
                    // fix tag
                    builder.AppendLine("  <td width=177 valign=top style='width:133.0pt;border:solid white 1.0pt;");
                    builder.AppendLine("  mso-border-themecolor:background1;border-top:none;mso-border-top-alt:solid white .75pt;");
                    builder.AppendLine("  mso-border-top-themecolor:background1;mso-border-alt:solid white .75pt;");
                    builder.AppendLine("  mso-border-themecolor:background1;mso-border-left-alt:solid white 1.0pt;");
                    builder.AppendLine("  mso-border-left-themecolor:background1;background:#D3DFEE;mso-background-themecolor:");
                    builder.AppendLine("  accent1;mso-background-themetint:63;padding:0cm 5.4pt 0cm 5.4pt'>");
                    builder.AppendFormat("  <p class=MsoNormal>{0}<o:p></o:p></p>\r\n", tag.Tag);
                    builder.AppendLine("  </td>");
                    // fix name
                    builder.AppendLine("  <td width=177 valign=top style='width:133.0pt;border:solid white 1.0pt;");
                    builder.AppendLine("  mso-border-themecolor:background1;border-top:none;mso-border-top-alt:solid white .75pt;");
                    builder.AppendLine("  mso-border-top-themecolor:background1;mso-border-alt:solid white .75pt;");
                    builder.AppendLine("  mso-border-themecolor:background1;mso-border-left-alt:solid white 1.0pt;");
                    builder.AppendLine("  mso-border-left-themecolor:background1;background:#D3DFEE;mso-background-themecolor:");
                    builder.AppendLine("  accent1;mso-background-themetint:63;padding:0cm 5.4pt 0cm 5.4pt'>");
                    builder.AppendFormat("  <p class=MsoNormal>{0}<o:p></o:p></p>\r\n", tag.Name);
                    builder.AppendLine("  </td>");
                    // fix value
                    builder.AppendLine("  <td width=439 valign=top style='width:329.1pt;border-top:none;border-left:");
                    builder.AppendLine("  none;border-bottom:solid white 1.0pt;mso-border-bottom-themecolor:background1;");
                    builder.AppendLine("  border-right:solid white 1.0pt;mso-border-right-themecolor:background1;");
                    builder.AppendLine("  mso-border-top-alt:solid white .75pt;mso-border-top-themecolor:background1;");
                    builder.AppendLine("  mso-border-left-alt:solid white .75pt;mso-border-left-themecolor:background1;");
                    builder.AppendLine("  mso-border-alt:solid white .75pt;mso-border-themecolor:background1;");
                    builder.AppendLine("  mso-border-right-alt:solid white 1.0pt;mso-border-right-themecolor:background1;");
                    builder.AppendLine("  background:#D3DFEE;mso-background-themecolor:accent1;mso-background-themetint:");
                    builder.AppendLine("  63;padding:0cm 5.4pt 0cm 5.4pt'>");
                    builder.AppendFormat("  <p class=MsoNormal>{0}<o:p></o:p></p>\r\n", tag.Value);
                    builder.AppendLine("  </td>");
                    // description
                    builder.AppendLine("  <td width=177 valign=top style='width:133.0pt;border:solid white 1.0pt;");
                    builder.AppendLine("  mso-border-themecolor:background1;border-top:none;mso-border-top-alt:solid white .75pt;");
                    builder.AppendLine("  mso-border-top-themecolor:background1;mso-border-alt:solid white .75pt;");
                    builder.AppendLine("  mso-border-themecolor:background1;mso-border-left-alt:solid white 1.0pt;");
                    builder.AppendLine("  mso-border-left-themecolor:background1;background:#D3DFEE;mso-background-themecolor:");
                    builder.AppendLine("  accent1;mso-background-themetint:63;padding:0cm 5.4pt 0cm 5.4pt'>");
                    builder.AppendFormat("  <p class=MsoNormal>{0}<o:p></o:p></p>\r\n", tag.Description);
                    builder.AppendLine("  </td>");
                    builder.AppendLine(" </tr>");
                }
            }
            builder.AppendLine("</table>");
            builder.AppendLine("</div>");
            builder.AppendLine("</body>");
            builder.AppendLine("</html>");

            CopyHtmlToClipboard(builder.ToString());
        }

        private void CopyHtmlToClipboard(string html)
        {
            var fullHtmlContent = html;
            var sb = new System.Text.StringBuilder();
            var header = @"Version:1.0
                StartHTML:<<<<<<<1
                EndHTML:<<<<<<<2
                StartFragment:<<<<<<<3
                EndFragment:<<<<<<<4";
            sb.Append(header);
            var startHTML = sb.Length;
            sb.Append(fullHtmlContent);
            var endHTML = sb.Length;

            sb.Replace("<<<<<<<1", startHTML.To8CharsString());
            sb.Replace("<<<<<<<2", endHTML.To8CharsString());
            sb.Replace("<<<<<<<3", startHTML.To8CharsString());
            sb.Replace("<<<<<<<4", endHTML.To8CharsString());
            Clipboard.Clear();
            Clipboard.SetText(sb.ToString(), TextDataFormat.Html);
        }

        private void ShowSingleOrder()
        {
            if (SelectedFixMessage == null) return;

            if (string.IsNullOrEmpty(OrderId(SelectedFixMessage.ClOrdID))) return;

            CollectionViewSource.GetDefaultView(FixMessagesCollection).Filter = o =>
            {
                var fixMessage = (FixMessage)o;
                if (fixMessage.MsgType == "(R) QuoteRequest" ||
                    fixMessage.MsgType == "(b) MassQuoteAcknowledgement" ||
                    fixMessage.MsgType == "(Z) QuoteCancel")
                    return fixMessage.ClOrdID == SelectedFixMessage.ClOrdID;
                return OrderId(fixMessage.ClOrdID) == OrderId(SelectedFixMessage.ClOrdID);
            };
        }

        private void ShowAllOrder()
        {
            if (SelectedFixMessage == null) return;

            if (string.IsNullOrEmpty(OrderId(SelectedFixMessage.ClOrdID))) return;

            CollectionViewSource.GetDefaultView(FixMessagesCollection).Filter = o => true;
        }

        private ObservableCollection<string> _filesCollection;

        public ObservableCollection<string> FilesCollection
        {
            get { return _filesCollection; }
            set
            {
                if (_filesCollection != value)
                {
                    _filesCollection = value;
                    RaisePropertyChanged(() => FilesCollection);
                }
            }
        }

        private string _selectedFile;
        public string SelectedFile
        {
            get { return _selectedFile; }
            set
            {
                _selectedFile = value;
                OnRefresh();
            }
        }

        private bool _showHeartbeat;
        public bool ShowHeartbeat
        {
            get { return _showHeartbeat; }
            set
            {
                _showHeartbeat = value;
                RaisePropertyChanged(() => ShowHeartbeat);
                OnRefresh();
            }
        }

        private bool _showFixTagDescription;
        public bool ShowFixTagDescription
        {
            get { return _showFixTagDescription; }
            set
            {
                _showFixTagDescription = value;
                RaisePropertyChanged(() => ShowFixTagDescription);
                OnRefresh();
            }
        }

        private bool _showTestRequest;
        public bool ShowTestRequest
        {
            get { return _showTestRequest; }
            set
            {
                _showTestRequest = value;
                RaisePropertyChanged(() => ShowTestRequest);
                OnRefresh();
            }
        }
        private bool _showLast500Message;
        public bool ShowLast500Message
        {
            get { return _showLast500Message; }
            set
            {
                _showLast500Message = value;
                RaisePropertyChanged(() => ShowLast500Message);
                OnRefresh();
            }
        }

        private ObservableCollection<FixMessage> _fixMessagesCollection;

        public ObservableCollection<FixMessage> FixMessagesCollection
        {
            get { return _fixMessagesCollection; }
            set
            {
                _fixMessagesCollection = value;
                RaisePropertyChanged(() => FixMessagesCollection);
            }
        }

        private FixMessage _selectedFixMessage;

        public FixMessage SelectedFixMessage
        {
            get { return _selectedFixMessage; }
            set
            {
                _selectedFixMessage = value;
                RaisePropertyChanged(() => SelectedFixMessage);
            }
        }

        private string OrderId(string clOrder)
        {
            if (clOrder == null) return null;

            var parms = clOrder.Split('.');
            return parms[0];
        }
    }
}

