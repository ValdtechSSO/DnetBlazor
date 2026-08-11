using System;
using Dnet.Blazor.Components.Spinner.Infrastructure.Interfaces;

namespace Dnet.Blazor.Components.Spinner.Infrastructure.Services
{
    public class SpinnerService : ISpinnerService
    {
        public event Action<int> OnCounterReceived;

        public void Show()
        {
            UpdateCounter(1);
        }

        public void Hide()
        {
            UpdateCounter(-1);
        }

        public void UpdateCounter(int counter)
        {
            OnCounterReceived?.Invoke(counter);
        }

        [Obsolete("Use UpdateCounter instead.")]
        public void UdateCounter(int counter) => UpdateCounter(counter);

    }
}
