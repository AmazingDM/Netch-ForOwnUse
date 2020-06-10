﻿using System.Collections.Generic;

namespace Netch.Models.SSD
{
    public class Main
    {
        /// <summary>
        ///     机场名
        /// </summary>
        public string airport;

        /// <summary>
        ///     端口
        /// </summary>
        public int port;

        /// <summary>
        ///     加密方式
        /// </summary>
        public string encryption;

        /// <summary>
        ///     密码
        /// </summary>
        public string password;

        /// <summary>
        ///     插件
        /// </summary>
        public string plugin;

        /// <summary>
        ///     插件参数
        /// </summary>
        public string plugin_options;

        /// <summary>
        ///     服务器数组
        /// </summary>
        public List<Server> servers;
    }
}
