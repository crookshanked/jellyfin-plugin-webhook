namespace Jellyfin.Plugin.Webhook.Destinations.Ntfy;

/// <summary>
/// Ntfy specific options.
/// </summary>
public class NtfyOption : BaseOption
{
    /// <summary>
    /// Gets or sets a value indicating whether to use credentials.
    /// </summary>
    public bool UseCredentials { get; set; }

    /// <summary>
    /// Gets or sets the username for basic auth.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the password for basic auth.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets the token for bearer auth.
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// Gets or sets the topic name.
    /// </summary>
    public string? Topic { get; set; }
}
