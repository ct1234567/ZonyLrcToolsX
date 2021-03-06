﻿using System;
using System.Windows.Forms;
using Zony.Lib.Infrastructures.Dependency;
using Zony.Lib.Plugin;

namespace ZonyLrcTools.Forms
{
    public partial class Form_PluginManagement : Form, ITransientDependency
    {
        public IPluginManager PlugManager { get; set; }

        public Form_PluginManagement()
        {
            InitializeComponent();
        }

        private void From_PluginManager_Load(object sender, EventArgs e)
        {
            FillListView();
        }

        /// <summary>
        /// 填充插件 ListView 列表
        /// </summary>
        private void FillListView()
        {
            var _infos = PlugManager.GetAllPluginInfos();
            foreach (var _info in _infos)
            {
                listView_PluginList.Items.Add(new ListViewItem(new string[]
                {
                    _info.Name,
                    _info.Author,
                    "0",
                    _info.Version.ToString(),
                    _info.Description
                }));
            }
        }
    }
}
