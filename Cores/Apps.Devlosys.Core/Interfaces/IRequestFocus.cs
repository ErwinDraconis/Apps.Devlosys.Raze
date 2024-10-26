using Apps.Devlosys.Core.Events;
using System;

namespace Apps.Devlosys.Core.Mvvm
{
    public interface IRequestFocus
    {
        event EventHandler<FocusRequestedEventArgs> FocusRequested;
    }
}
