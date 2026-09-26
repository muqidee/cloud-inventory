namespace CloudInventory.Desktop.ViewModels;

public sealed class OverviewViewModel : ViewModelBase
{
	public OverviewViewModel(ConnectionSessionViewModel connectionSession)
	{
		ConnectionSession = connectionSession;
	}

	public ConnectionSessionViewModel ConnectionSession { get; }
}
