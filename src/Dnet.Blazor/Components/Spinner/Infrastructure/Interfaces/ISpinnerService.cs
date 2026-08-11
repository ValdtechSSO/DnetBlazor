namespace Dnet.Blazor.Components.Spinner.Infrastructure.Interfaces
{
    public interface ISpinnerService
    {
        event Action<int> OnCounterReceived;

        void Show();

        void Hide();

        void UpdateCounter(int items);

        [Obsolete("Use UpdateCounter instead.")]
        void UdateCounter(int items);
    }
}
