using System.Collections.Generic;
using System.Windows.Media;
using BangumiSubReNamer.Models;
using CommunityToolkit.Mvvm.Messaging;
using Wpf.Ui.Controls;

namespace BangumiSubReNamer.ViewModels.Pages
{
    public partial class DataViewModel : ObservableObject, IRecipient<DataWindowSize>
    {
        public DataViewModel()
        {
            WeakReferenceMessenger.Default.Register(this);
        }
        [ObservableProperty] private int height = 580;
        
        public void Receive(DataWindowSize message)
        {
            Height = message.Height - 70;
        }
    }
}
