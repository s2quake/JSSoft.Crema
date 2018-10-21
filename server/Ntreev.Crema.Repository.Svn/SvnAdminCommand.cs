﻿using Ntreev.Crema.Services;
using Ntreev.Library;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ntreev.Crema.Repository.Svn
{
    class SvnAdminCommand : CommandHost
    {
        private const string svnadmin = "svnadmin";

        public SvnAdminCommand(string commandName)
            : base(svnadmin, null, commandName)
        {

        }

        public string Run(ILogService logService)
        {
            try
            {
                return this.Run();
            }
            catch (Exception e)
            {
                logService.Error(e);
                throw;
            }
        }
    }
}