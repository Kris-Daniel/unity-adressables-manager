using System;
using System.Threading;

namespace Utils.Cancellation
{
	public interface ICancellationTokenUser
	{
		CancellationToken CtsToken { get; set; }
		Action OnCtsTokenSet { get; set; }
	}
}