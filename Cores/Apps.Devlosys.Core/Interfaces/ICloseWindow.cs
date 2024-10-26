using System;
using System.Threading.Tasks;

namespace Apps.Devlosys.Core.Mvvm
{
    public interface ICloseWindow
    {
        Action Close { get; set; }

        Action Open { get; set; }

        void VerifyState();

        void ValidateToExit(Action onYes, Action onNo);
    }
}
