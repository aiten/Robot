﻿namespace InputWpf.Tools;

public class BaseViewModel : NotifyPropertyChanged
{
    public IWindowNavigator? Controller { get; set; }
}