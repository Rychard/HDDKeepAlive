﻿using System.ComponentModel;
using System.Configuration.Install;


namespace HDDKeepAliveService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }
    }
}
