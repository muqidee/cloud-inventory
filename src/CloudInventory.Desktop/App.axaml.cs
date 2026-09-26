using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CloudInventory.Application.Connections;
using CloudInventory.Desktop.Services;
using CloudInventory.Desktop.ViewModels;
using CloudInventory.Desktop.Views;
using CloudInventory.Infrastructure.Aws.Connections;
using Microsoft.Extensions.DependencyInjection;

namespace CloudInventory.Desktop;

public partial class App : Avalonia.Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var services = BuildServices();

            desktop.MainWindow = new MainWindow
            {
                DataContext = services.GetRequiredService<MainViewModel>(),
            };

            desktop.Exit += (_, _) => services.Dispose();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static ServiceProvider BuildServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IAwsConnectionService, AwsConnectionService>();
        services.AddSingleton<IAwsProfileCatalog, AwsProfileCatalog>();
        services.AddSingleton<IUriLauncher, SystemUriLauncher>();
        services.AddSingleton<ConnectionSessionViewModel>();
        services.AddSingleton<ConnectionsViewModel>();
        services.AddSingleton<OverviewViewModel>();
        services.AddSingleton<MainViewModel>();

        return services.BuildServiceProvider();
    }
}
