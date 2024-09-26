//using BluetoothTest.Shared.BluetoothService.Interfaces;
//using BluetoothTest.Shared.BluetoothService.Models;

using Blazor.Bluetooth;

namespace BluetoothTest.Shared.BluetoothService.Extensions;

public static class ServiceExtensions
{
    /// <summary>
    /// Add <see cref="IBluetoothNavigator"/> to <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <returns>Service collection.</returns>
    public static IServiceCollection AddBluetoothNavigator(this IServiceCollection services)
    {
        return services.AddTransient<IBluetoothNavigator>();
    }
}
