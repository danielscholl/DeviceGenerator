using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Configuration;

namespace DeviceGenerator
{
    static class Program
    {
        static async Task Main()
        {
            var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();

            var count = Convert.ToInt32(configuration.GetSection("DeviceCount").Value, CultureInfo.CurrentCulture);
            var multiplier = Convert.ToInt32(configuration.GetSection("Multiplier").Value, CultureInfo.CurrentCulture);
            var identityDb = RegistryManager.CreateFromConnectionString(configuration.GetConnectionString("Hub"));

            for (int m = 1; m < multiplier + 1; m++)
            {
                var deviceList = new List<Device>();
                for (int i = 1; i < count + 1; i++)
                {
                    var deviceId = Guid.NewGuid().ToString();
                    //Console.WriteLine($"Add Device:  '{deviceId}'");
                    await identityDb.AddDeviceAsync(new Device(deviceId)).ConfigureAwait(false);
                    deviceList.Add(new Device(deviceId));                
                }

                await identityDb.AddDevices2Async(deviceList).ConfigureAwait(false);
                Console.WriteLine($"Send Batch {m}: Completed");
            }
            Console.WriteLine($"Device Generation: {count * multiplier} Sent");
            var registryStats = await identityDb.GetRegistryStatisticsAsync().ConfigureAwait(false);
            Console.WriteLine($"Registry now has {registryStats.TotalDeviceCount} devices.");
        }
    }
}
