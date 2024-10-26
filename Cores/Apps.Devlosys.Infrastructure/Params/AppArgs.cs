using System;
using System.Windows.Input;

namespace Apps.Devlosys.Infrastructure.Params
{
    public class MenuButton
    {
        public string Title { get; set; }

        public string Kind { get; set; }

        public string View { get; set; }

        public bool IsEnable { get; set; }

        public ICommand Action { get; set; }

    }
}
