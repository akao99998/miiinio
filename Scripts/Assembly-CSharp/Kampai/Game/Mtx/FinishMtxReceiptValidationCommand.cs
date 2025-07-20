using strange.extensions.command.impl;

namespace Kampai.Game.Mtx
{
	public class FinishMtxReceiptValidationCommand : Command
	{
		[Inject]
		public ReceiptValidationResult validationResult { get; set; }

		[Inject]
		public ICurrencyService currencyService { get; set; }

		public override void Execute()
		{
			currencyService.ReceiptValidationCallback(validationResult);
		}
	}
}
