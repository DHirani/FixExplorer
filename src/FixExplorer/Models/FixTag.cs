using FixExplorer.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using FixExplorer.Extensions;

namespace FixExplorer.Models
{
    public class FixTag : NotificationObject
    {
        private readonly string _fixVersion;
        private string _tag;
        private string _value;
        private string _description;
        private string _name;
        private bool _requried;

        public FixTag(bool showValueDescription, string fixVersion, string tag, string value)
        {
            _fixVersion = fixVersion;

            Tag = tag;

            if (Tag == "52")
            {
                var dt = value.ToDateTime().UtcToLocalTime();
                value = dt.ToString(CultureInfo.InvariantCulture);
            }
            Value = value;

            if (showValueDescription)
                SetDescription();
        }

        internal static XDocument Fix42DocumentData;
        public static XDocument Fix42Document()
        {
            if (Fix42DocumentData == null)
            {
                Fix42DocumentData = XDocument.Load(@".\Specs\FIX42.xml");
                _fix42Tags = new Dictionary<Tuple<string, string>, Tuple<string, string>>();
            }
            return Fix42DocumentData;
        }
        private static Dictionary<Tuple<string, string>, Tuple<string, string>> _fix42Tags;


        internal static XDocument Fix44DocumentData;
        public static XDocument Fix44Document()
        {
            if (Fix44DocumentData == null)
            {
                Fix44DocumentData = XDocument.Load(@".\Specs\FIX44.xml");
                _fix44Tags = new Dictionary<Tuple<string, string>, Tuple<string, string>>();
            }
            return Fix44DocumentData;
        }
        private static Dictionary<Tuple<string, string>, Tuple<string, string>> _fix44Tags;

        private void SetDescription()
        {
            switch (_fixVersion)
            {
                case "FIX.4.2":
                    SetDescriptionAndName42();
                    break;
                case "FIX.4.4":
                    SetDescriptionAndName44();
                    break;
            }

        }

        private void SetDescriptionAndName42()
        {
            var document = Fix42Document();

            var key = new Tuple<string, string>(Tag, Value);
            Tuple<string, string> value;
            if (!_fix42Tags.TryGetValue(key, out value))
            {
                var nodes = document.Elements("fix").Elements("fields").Elements("field");
                if (nodes != null)
                    foreach (XElement node in nodes)
                    {
                        if (node.Attribute("number").Value == Tag)
                        {
                            Name = node.Attribute("name").Value.Replace("_", " ");
                            foreach (XElement valueNode in node.Elements())
                            {
                                if (valueNode.Attribute("enum").Value == Value)
                                {
                                    Description = valueNode.Attribute("description").Value.Replace("_", " ");
                                    value = new Tuple<string, string>(Name, Description);
                                    _fix42Tags.Add(key, value);
                                    break;
                                }
                            }
                        }
                    }
            }
            else
            {
                Name = value.Item1;
                Description = value.Item2;
            }
        }

        private void SetDescriptionAndName44()
        {
            var document = Fix44Document();

            // version not supported
            if (document == null)
                return;
            var key = new Tuple<string, string>(Tag, Value);
            Tuple<string, string> value;
            if (!_fix44Tags.TryGetValue(key, out value))
            {
                var nodes = document.Elements("fix").Elements("fields").Elements("field");
                if (nodes != null)
                    foreach (XElement node in nodes)
                    {
                        if (node.Attribute("number").Value == Tag)
                        {
                            Name = node.Attribute("name").Value.Replace("_", " ");
                            foreach (XElement valueNode in node.Elements())
                            {
                                if (valueNode.Attribute("enum").Value == Value)
                                {

                                    Description = valueNode.Attribute("description").Value.Replace("_", " ");
                                    value = new Tuple<string, string>(Name, Description);
                                    _fix44Tags.Add(key, value);
                                    break;
                                }
                            }
                        }
                    }
            }
            else
            {
                Name = value.Item1;
                Description = value.Item2;
            }
        }

        public string Tag
        {
            get { return _tag; }
            set { _tag = value; RaisePropertyChanged(() => Tag); }
        }

        public string Value
        {
            get { return _value; }
            set { _value = value; RaisePropertyChanged(() => Value); }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; RaisePropertyChanged(() => Description); }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; RaisePropertyChanged(() => Name); }
        }

        public bool Required
        {
            get { return _requried; }
            set { _requried = value; RaisePropertyChanged(() => Required); }
        }
    }
}
