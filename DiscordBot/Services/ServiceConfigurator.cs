﻿using DiscordBot.ConfigModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscordBot.Services
{
    public class ServiceConfigurator
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile("defaultchannelsettings.json")
                .AddEnvironmentVariables()
                .Build();

            services.AddSingleton(config);
            services.AddSingleton(provider =>
            {
                var settings = new AppSettings();
                config.GetSection("AppSettings").Bind(settings);
                return settings;
            });
            services.AddSingleton<JsonConfigManager>();
            services.AddSingleton<MessagesManager>();
        }
    }
}
