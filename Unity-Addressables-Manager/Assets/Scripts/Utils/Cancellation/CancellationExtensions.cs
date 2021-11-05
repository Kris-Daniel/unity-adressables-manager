using System.Threading;

namespace Utils.Cancellation
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