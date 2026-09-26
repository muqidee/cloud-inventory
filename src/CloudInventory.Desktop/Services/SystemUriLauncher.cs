using System.Diagnostics;

namespace CloudInventory.Desktop.Services;

public sealed class SystemUriLauncher : IUriLauncher
{
    public void Open(Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);

        var process = Process.Start(new ProcessStartInfo(uri.AbsoluteUri)
        {
            UseShellExecute = true,
        });

        if (process is null)
        {
            throw new InvalidOperationException("The sign-in page could not be opened.");
        }
    }
}
