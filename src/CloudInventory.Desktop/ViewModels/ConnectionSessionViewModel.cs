using CloudInventory.Application.Connections;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CloudInventory.Desktop.ViewModels;

public partial class ConnectionSessionViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string ConnectionLabel { get; private set; } = "No connection";

    [ObservableProperty]
    public partial string SessionLabel { get; private set; } = "Not signed in";

    [ObservableProperty]
    public partial bool IsConnected { get; private set; }

    [ObservableProperty]
    public partial AwsConnectionIdentity? Identity { get; private set; }

    public void Begin(string profileName)
    {
        ConnectionLabel = profileName;
        SessionLabel = "Signing in";
        IsConnected = false;
        Identity = null;
    }

    public void Complete(AwsConnectionIdentity identity)
    {
        Identity = identity;
        ConnectionLabel = identity.ProfileName;
        SessionLabel = $"{identity.AccountId} / {identity.Region}";
        IsConnected = true;
    }

    public void Clear()
    {
        ConnectionLabel = "No connection";
        SessionLabel = "Not signed in";
        IsConnected = false;
        Identity = null;
    }
}
