using System;
using System.IO;
using System.Linq;
using UnityEngine;
using Modules.LogSystem;
using Modules.Utility.Singleton;

namespace Modules.IOSystem
{
    public class IOManager : MonoSingleton<IOManager>, IModule
    {
        protected override void OnAwake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void Prepare()
        {
            LogManager.Info("IOSystem.IOManager", "IOSystem已经初始化");
        }

        public static void Write(string f_path, string f_name, string data)
        {
            //找到文件路径
            string fpath = $"{Application.dataPath}/{f_path}/{f_name}.sdf";

            //打开写入流
            StreamWriter sw = new StreamWriter(fpath);

            //写入内容
            sw.WriteLine(data);

            //关闭流
            sw.Close();
        }

        public static string Read(string path)
        {
            try
            {
                return File.ReadAllText(path);
            }
            catch (Exception ex)
            {
                LogManager.Error("DataIO", $"读取文件（{path}）时发生异常", args: ("异常信息", ex.ToString()));
                return null;
            }
        }

        public static string Read(string f_path, string f_name)
        {
            return Read($"{f_path}/{f_name}");
        }

        public static string Read(params string[] args)
        {
            return Read(string.Concat(args));
        }
    }
}
