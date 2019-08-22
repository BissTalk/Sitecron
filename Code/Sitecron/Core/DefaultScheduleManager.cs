﻿using System;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecron.Core.Jobs;
using Sitecron.Core.Scheduling;
using Sitecron.SitecronSettings;
using System.Reflection;

namespace Sitecron.Core
{
    public class DefaultScheduleManager : IScheduleManager
    {
        private readonly ISitecronJobProvider _jobProvider;
        private readonly ISitecronScheduler _scheduler;

        public DefaultScheduleManager(ISitecronJobProvider jobProvider, ISitecronScheduler scheduler)
        {
            Assert.ArgumentNotNull(jobProvider, nameof(jobProvider));
            Assert.ArgumentNotNull(scheduler, nameof(scheduler));

            _jobProvider = jobProvider;
            _scheduler = scheduler;
        }

        public void ScheduleAllJobs()
        {
            var publishingInstance = Settings.Publishing.PublishingInstance;
            var instanceName = Settings.InstanceName.ToLower();
            var usePublishingInstanceAsPrimaryServer = Settings.GetBoolSetting(SitecronConstants.SettingsNames.UsePublishingInstanceAsPrimaryServer, true);

            if ((!string.IsNullOrEmpty(publishingInstance) &&
                !string.IsNullOrEmpty(instanceName) &&
                !publishingInstance.Equals(instanceName, StringComparison.OrdinalIgnoreCase)) 
                && usePublishingInstanceAsPrimaryServer)
            {
                Log.Info($"SiteCron - Exit without initialization, this server is not the primary in the load balanced environment. PublishingInstance: {publishingInstance} != InstanceName: {instanceName}", this);
                return;
            }

            try
            {
                Log.Info("Initialize SiteCron: " + Assembly.GetExecutingAssembly().GetName().Version, this);
                _scheduler.ClearJobs();

                Log.Info("Loading SiteCron Jobs", this);

                foreach (var job in _jobProvider.GetJobs())
                {
                    _scheduler.ScheduleJob(job);
                }
            }
            catch (Exception ex)
            {
                Log.Error("SiteCron ERROR: " + ex.Message, ex, this);
            }
        }
    }
}