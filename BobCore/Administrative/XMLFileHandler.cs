using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Xml;

namespace BobCore.Administrative
{
    static class XMLFileHandler
    {
        public static bool fileexists(string filename,bool isxml = true)
        {
            var path = filename;
            if(isxml)
            {
                path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/XML/";
                if (filename.Contains(".xml"))
                {
                    path += filename;
                }
                else
                {
                    path += filename + ".xml";
                }
            }
            
            if (File.Exists(path))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static void writeFile(dynamic data, string filename)
        {
            var path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/XML/";
            var backup = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/XML/";
            if (filename.Contains(".xml"))
            {
                path += filename;
            }
            else
            {
                path += filename + ".xml";
            }
            string backupfile = filename.Replace(".xml", "");
            backup += backupfile + "_bck.xml";
            if (XMLFileHandler.fileexists(filename))
            {
                //only works if file exists ...
                while (!Useful_Functions.IsFileReady(path))
                {
                    Thread.Sleep(100);
                }
                if (File.Exists(path))
                {
                    File.Copy(path, backup, true);
                }
            }

            System.Xml.Serialization.XmlSerializer xmlserializer = new System.Xml.Serialization.XmlSerializer(data.GetType());
            System.IO.FileStream file = System.IO.File.Create(path);
            xmlserializer.Serialize(file, data);
            file.Close();
        }
        public static dynamic readFile(string filename, string type)
        {

            //setting base path

            var path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/XML/";
            if (filename.Contains(".xml"))
            {
                path += filename;
            }
            else
            {
                path += filename + ".xml";
            }

            FileStream fsFile = new FileStream(path, FileMode.OpenOrCreate);
            XmlReader xReader = XmlReader.Create(fsFile);
            //auto add MainNameSpace
            if (!type.Contains("BobCore"))
            {
                type = "BobCore." + type;
            }
            Type elementType = Type.GetType(type);
            Type listType = typeof(List<>).MakeGenericType(new Type[] { elementType });

            object list = Activator.CreateInstance(listType);
            System.Xml.Serialization.XmlSerializer xmlserializer = new System.Xml.Serialization.XmlSerializer(listType);
            try
            {
                list = xmlserializer.Deserialize(xReader);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            fsFile.Close();
            while (!Useful_Functions.IsFileReady(path))
            {
                Thread.Sleep(100);
            }

            return list;
        }
    }
}
