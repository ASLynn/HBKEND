//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Microsoft.WindowsAzure;
//using Microsoft.WindowsAzure.Diagnostics;
//using Microsoft.WindowsAzure.ServiceRuntime;
//using Microsoft.Web.Administration;

//namespace HeyDoc.Web
//{
//    public class WebRole : RoleEntryPoint
//    {
//        public override bool OnStart()
//        {
//            ServerManager iisManager = new ServerManager();

//            TimeSpan ts = new TimeSpan(0, 0, 0);
//            iisManager.ApplicationPoolDefaults.ProcessModel.IdleTimeout = ts;
//            iisManager.ApplicationPoolDefaults.Recycling.PeriodicRestart.PrivateMemory = 1300000;//1.3GB
//            iisManager.ApplicationPoolDefaults.AutoStart = true;
//            iisManager.ApplicationPoolDefaults["startMode"] = "AlwaysRunning";

//            foreach (var application in iisManager.Sites.SelectMany(c => c.Applications))
//            {
//                application["preloadEnabled"] = true;
//            }

//            iisManager.CommitChanges();

//            // For information on handling configuration changes
//            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

//            return base.OnStart();
//        }
//    }
//}
