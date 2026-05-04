using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace AppMobile;

public partial class App : Application
{
	public App()
	{
		var fr = new CultureInfo("fr-FR");
		CultureInfo.DefaultThreadCurrentCulture = fr;
		CultureInfo.DefaultThreadCurrentUICulture = fr;
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}