using Audiotool.viewmodel;
using System.Windows;
using Audiotool.Converters;

namespace Audiotool;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private NativeAudio ViewModel => (NativeAudio)DataContext;
    
    public MainWindow()
    {
        DataContext = new NativeAudio();
        InitializeComponent();
        
        // Add supported formats text
        ViewModel.GetType().GetProperty("SupportedFormatsText")?.SetValue(
            ViewModel, 
            string.Join(", ", WavConverter.SupportedFormats)
        );
    }

    private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "FiveM Audio Converter Tool\nCreated by Renewed Scripts | FjamZoo\nConverts audio files to FiveM native format\nSpecial thanks to Ehbw, CodeWalker, ChatDisabled, uShifty",
            "About",
            MessageBoxButton.OK,
            MessageBoxImage.Information
        );
    }
    
    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (ViewModel.CurrentProject.IsModified)
        {
            MessageBoxResult result = MessageBox.Show(
                "Save changes to the current project?", 
                "Unsaved Changes", 
                MessageBoxButton.YesNoCancel
            );
            
            if (result == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            
            if (result == MessageBoxResult.Yes)
            {
                var saveCommand = ViewModel.SaveProjectCommand;
                if (saveCommand.CanExecute(null))
                {
                    saveCommand.Execute(null);
                }
            }
        }
        
        base.OnClosing(e);
    }
}