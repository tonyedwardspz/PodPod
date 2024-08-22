using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PodPod.Models;

public class Base : INotifyPropertyChanged
{

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}

}