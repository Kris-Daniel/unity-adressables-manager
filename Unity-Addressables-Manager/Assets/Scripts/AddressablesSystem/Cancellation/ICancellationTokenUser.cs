using System;
using System.Threading;

namespace AddressablesSystem.Cancellation
{
	public interface ICancellationTokenUser
	{
		CancellationToken CtsToken { get; set; }
		Action OnCtsTokenSet { get; set; }
	}
}