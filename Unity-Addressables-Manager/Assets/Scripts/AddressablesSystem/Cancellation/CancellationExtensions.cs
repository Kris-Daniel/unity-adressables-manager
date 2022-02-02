using System.Threading;

namespace AddressablesSystem.Cancellation
{
	public static class CancellationExtensions
	{
		public static void SetToken(this ICancellationTokenUser ctsUser, CancellationToken token)
		{
			ctsUser.CtsToken = token;
			ctsUser.OnCtsTokenSet?.Invoke();
		}
	}
}