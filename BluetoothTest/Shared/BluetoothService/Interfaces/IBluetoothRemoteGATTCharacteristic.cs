namespace BluetoothTest.Shared.BluetoothService.Interfaces;

/// <summary>
/// Represents a GATT Characteristic, which is a basic data element that provides further information about a peripheral’s service.
/// </summary>
public interface IBluetoothRemoteGATTCharacteristic
{
    /// <summary>
    /// Gets a string containing the UUID of the characteristic, for example '00002a37-0000-1000-8000-00805f9b34fb' for the Heart Rate Measurement characteristic.
    /// </summary>
    string Uuid { get; }

    /// <summary>
    /// Gets a string that uniquely identifies a device.
    /// </summary>
    string DeviceUuid { get; }

    /// <summary>
    /// Gets a string representing the UUID of this service.
    /// </summary>
    string ServiceUuid { get; }

    /// <summary>
    /// Gets the currently cached characteristic value. This value gets updated when the value of the characteristic is read or updated via a notification or indication.
    /// </summary>
    byte[] Value { get; }

    /// <summary>
    /// Returns a bytes holding a duplicate of the value property if it is available and supported. Otherwise it throws an error.
    /// </summary>
    /// <returns>Task with a bytes result.</returns>
    Task<byte[]> ReadValue();

    /// <summary>
    /// Write a bytes value to the characteristic.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <returns>Task.</returns>
    [Obsolete("This feature is no longer recommended. Though some browsers might still support it, it may have already been removed from the relevant web standards, may be in the process of being dropped, or may only be kept for compatibility purposes. Avoid using it, and update existing code if possible; see the compatibility table at the bottom of this page to guide your decision. Be aware that this feature may cease to work at any time. Use IBluetoothRemoteGATTCharacteristic.WriteValueWithResponse and IBluetoothRemoteGATTCharacteristic.WriteValueWithoutResponse instead.")]
    Task WriteValue(byte[] value);

    /// <summary>
    /// Write a bytes value to the characteristic with response.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <returns>Task.</returns>
    Task WriteValueWithResponse(byte[] value);

    /// <summary>
    /// Write a bytes value to the characteristic without response.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <returns>Task.</returns>
    Task WriteValueWithoutResponse(byte[] value);

    /// <summary>
    /// Start notifications with subscribe user to get notifications raised in <see cref="OnRaiseCharacteristicValueChanged"/>.
    /// </summary>
    /// <returns>Task.</returns>
    Task StartNotifications();

    /// <summary>
    /// Stop notifications with subscribe user to get notifications raised in <see cref="OnRaiseCharacteristicValueChanged"/>.
    /// </summary>
    /// <returns>Task.</returns>
    Task StopNotifications();
}
