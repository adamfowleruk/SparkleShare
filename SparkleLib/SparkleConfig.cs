//   SparkleShare, a collaboration and sharing tool.
//   Copyright (C) 2010  Hylke Bons <hylkebons@gmail.com>
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General Public License for more details.
//
//   You should have received a copy of the GNU General Public License
//   along with this program.  If not, see <http://www.gnu.org/licenses/>.


using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;

using Mono.Unix;

namespace SparkleLib {

    public class SparkleConfig : XmlDocument {

        public static SparkleConfig DefaultConfig = new SparkleConfig (
            SparklePaths.SparkleConfigPath, "config.xml");

        public string Path;


        public SparkleConfig (string config_path, string config_file_name)
        {
            Path = System.IO.Path.Combine (config_path, config_file_name);

            if (!Directory.Exists (config_path)) {
                Directory.CreateDirectory (config_path);
                SparkleHelpers.DebugInfo ("Config", "Created \"" + config_path + "\"");
            }

            string icons_path = System.IO.Path.Combine (config_path, "icons");
            if (!Directory.Exists (icons_path)) {
                Directory.CreateDirectory (icons_path);
                SparkleHelpers.DebugInfo ("Config", "Created \"" + icons_path + "\"");
            }

            if (!File.Exists (Path))
                CreateInitialConfig ();

            Load (Path);
        }


        private void CreateInitialConfig ()
        {
            string user_name = Environment.UserName;

            if (SparkleBackend.Platform == PlatformID.Unix ||
                SparkleBackend.Platform == PlatformID.MacOSX) {

                user_name = new UnixUserInfo (UnixEnvironment.UserName).RealName;
                user_name = user_name.TrimEnd (",".ToCharArray());

            }

            if (string.IsNullOrEmpty (user_name))
                user_name = Environment.UserName;

            TextWriter writer = new StreamWriter (Path);
            string n          = Environment.NewLine;

            writer.Write ("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>" + n +
                          "<sparkleshare>" + n +
                          "  <user>" + n +
                          "    <name>" + user_name + "</name>" + n +
                          "    <email>Unknown</email>" + n +
                          "  </user>" + n +
                          "</sparkleshare>");
            writer.Close ();

            SparkleHelpers.DebugInfo ("Config", "Created \"" + Path + "\"");
        }


        public string UserName {
            get {
                XmlNode node = SelectSingleNode ("/sparkleshare/user/name/text()");
                return node.Value;
            }

            set {
                XmlNode node = SelectSingleNode ("/sparkleshare/user/name/text()");
                node.InnerText = value;

                Save ();
            }
        }


        public string UserEmail {
            get {
                XmlNode node = SelectSingleNode ("/sparkleshare/user/email/text()");
                return node.Value;
            }

            set {
                XmlNode node = SelectSingleNode ("/sparkleshare/user/email/text()");
                node.InnerText = value;

                Save ();
            }
        }


        public List<string> Folders {
            get {
                List<string> folders = new List<string> ();

                foreach (XmlNode node_folder in SelectNodes ("/sparkleshare/folder"))
                    folders.Add (node_folder ["name"].InnerText);

                return folders;
            }
        }


        public void AddFolder (string name, string url, string backend)
        {
            XmlNode node_name = CreateElement ("name");
            node_name.InnerText = name;

            XmlNode node_url = CreateElement ("url");
            node_url.InnerText = url;

            XmlNode node_backend = CreateElement ("backend");
            node_backend.InnerText = backend;

            XmlNode node_folder = CreateNode (XmlNodeType.Element, "folder", null);
            node_folder.AppendChild (node_name);
            node_folder.AppendChild (node_url);
            node_folder.AppendChild (node_backend);

            XmlNode node_root = SelectSingleNode ("/sparkleshare");
            node_root.AppendChild (node_folder);

            Save ();
        }


        public void RemoveFolder (string name)
        {
            foreach (XmlNode node_folder in SelectNodes ("/sparkleshare/folder")) {
                if (node_folder ["name"].InnerText.Equals (name))
                    SelectSingleNode ("/sparkleshare").RemoveChild (node_folder);
            }

            Save ();
        }


        public bool FolderExists (string name)
        {
            foreach (XmlNode node_folder in SelectNodes ("/sparkleshare/folder")) {
                if (node_folder ["name"].InnerText.Equals (name))
                    return true;
            }

            return false;
        }


        public string GetBackendForFolder (string name)
        {
            foreach (XmlNode node_folder in SelectNodes ("/sparkleshare/folder")) {
                if (node_folder ["name"].InnerText.Equals (name))
                    return node_folder ["backend"].InnerText;
            }

            return null;
        }


        public string GetUrlForFolder (string name)
        {
            foreach (XmlNode node_folder in SelectNodes ("/sparkleshare/folder")) {
                if (node_folder ["name"].InnerText.Equals (name))
                    return node_folder ["url"].InnerText;
            }

            return null;
        }


        public string GetAnnouncementsForFolder (string name)
        {
            foreach (XmlNode node_folder in SelectNodes ("/sparkleshare/folder")) {
                if (node_folder ["name"].InnerText.Equals (name) &&
                    node_folder ["announcements"] != null) {

                    return node_folder ["announcements"].InnerText;
                }
            }

            return null;
        }


        public string GetConfigOption (string name)
        {
            XmlNode node = SelectSingleNode ("/sparkleshare/" + name);

            if (node != null)
                return node.InnerText;
            else
                return null;
        }


        public void SetConfigOption (string name, string content)
        {
            XmlNode node = SelectSingleNode ("/sparkleshare/" + name);

            if (node != null) {
                node.InnerText = content;

            } else {
                node           = CreateElement (name);
                node.InnerText = content;

                XmlNode node_root = SelectSingleNode ("/sparkleshare");
                node_root.AppendChild (node);
            }

            SparkleHelpers.DebugInfo ("Config", "Updated " + name + ":" + content);
            Save ();
        }


        public void Save ()
        {
            if (!File.Exists (Path))
                throw new ConfigFileNotFoundException (Path + " does not exist");

            Save (Path);
            SparkleHelpers.DebugInfo ("Config", "Updated \"" + Path + "\"");
        }
    }


    public class ConfigFileNotFoundException : Exception {

        public ConfigFileNotFoundException (string message) :
            base (message) { }
    }
}