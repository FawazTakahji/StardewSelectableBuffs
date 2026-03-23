using StardewUI.Framework;

namespace SelectableBuffs.ViewModels;

public class SelectionViewModel
{
    public string Title { get; set; }
    public List<SelectionOption> Options { get; set; }

    private IMenuController Controller { get; set; } = null!;
    private bool _didSelect;
    private readonly Action<string> _onSelectionChanged;

    public SelectionViewModel(string title, List<SelectionOption> options, Action<string> onSelectionChanged)
    {
        Title = title;
        Options = options;
        _onSelectionChanged = onSelectionChanged;
    }

    public void SetController(IMenuController controller)
    {
        Controller = controller;
        controller.Closed += OnClosed;
    }

    public void SelectOption(string option)
    {
        _didSelect = true;
        _onSelectionChanged(option);
        Controller.Close();
    }

    private void OnClosed()
    {
        if (!_didSelect)
        {
            _onSelectionChanged("canceled");
        }

        Controller.Closed -= OnClosed;
    }
}