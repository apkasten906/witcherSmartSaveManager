# ✅ WPF DataGrid Checkbox Binding – Best Practices

This guide summarizes the techniques needed to build a reliable WPF `DataGrid` with:

- ✅ Per-row checkboxes
- ✅ A working “Select All” checkbox in the header
- ✅ A dynamically enabled “Delete Selected” button

---

## 📦 Cell Checkbox (Per Row)

- Binds to the row’s ViewModel (e.g., `SaveFileViewModel.IsSelected`)
- Must use `TwoWay` binding with `UpdateSourceTrigger=PropertyChanged`
- Prevent focus from interfering with click propagation

```xml
<CheckBox IsChecked="{Binding IsSelected, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
          Focusable="False"
          HorizontalAlignment="Center" />
```

---

## 🧱 Header Checkbox (Select All)

- Not part of the `ItemsSource`, so it doesn’t automatically bind to row context
- Must use `ElementName` to bind to your window’s `DataContext`

```xml
<CheckBox IsChecked="{Binding DataContext.AreAllSelected,
                      ElementName=MainWindowRoot,
                      Mode=TwoWay,
                      UpdateSourceTrigger=PropertyChanged}" />
```

---

## 🧠 ViewModel Logic

- `AreAllSelected` must be a **computed property**
- Changing it should loop through all rows and update `IsSelected`
- Listen to each row’s `PropertyChanged` event to reevaluate the header checkbox and commands

```csharp
public bool AreAllSelected => Saves.Count > 0 && Saves.All(s => s.IsSelected);

private void Save_PropertyChanged(object sender, PropertyChangedEventArgs e)
{
    if (e.PropertyName == nameof(SaveFileViewModel.IsSelected))
    {
        OnPropertyChanged(nameof(AreAllSelected)); // Refresh header
        CommandManager.InvalidateRequerySuggested(); // Refresh button states
    }
}
```

---

## 🧩 Delete Button Command

- Use a `RelayCommand` that binds `CanExecute` to `Saves.Any(s => s.IsSelected)`
- Hook into `CommandManager.RequerySuggested` to keep the button enabled/disabled dynamically

```csharp
public ICommand DeleteSelectedCommand => new RelayCommand(
    _ => { /* remove logic */ },
    _ => Saves.Any(s => s.IsSelected)
);
```

Make sure your `RelayCommand` implements `CanExecuteChanged` like this:

```csharp
public event EventHandler CanExecuteChanged
{
    add => CommandManager.RequerySuggested += value;
    remove => CommandManager.RequerySuggested -= value;
}
```

---

### ✅ That’s it!

Use this checklist anytime you wire up interactive selection logic in a WPF `DataGrid`.
