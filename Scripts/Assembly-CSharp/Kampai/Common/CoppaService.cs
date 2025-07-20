using System;

namespace Kampai.Common
{
	public class CoppaService : ICoppaService
	{
		[Inject]
		public ILocalPersistanceService localPersistanceService { get; set; }

		public bool IsBirthdateKnown()
		{
			return localPersistanceService.HasKeyPlayer("COPPA_Age_Month") && localPersistanceService.HasKeyPlayer("COPPA_Age_Year");
		}

		public bool GetBirthdate(out DateTime birthdate)
		{
			if (!IsBirthdateKnown())
			{
				birthdate = DateTime.Now;
				return false;
			}
			int dataIntPlayer = localPersistanceService.GetDataIntPlayer("COPPA_Age_Year");
			int dataIntPlayer2 = localPersistanceService.GetDataIntPlayer("COPPA_Age_Month");
			birthdate = new DateTime(dataIntPlayer, dataIntPlayer2, 1);
			return true;
		}

		public bool Restricted()
		{
			return GetAge() < 13;
		}

		public void SetUserBirthdate(DateTime birthdate)
		{
			localPersistanceService.PutDataIntPlayer("COPPA_Age_Month", birthdate.Month);
			localPersistanceService.PutDataIntPlayer("COPPA_Age_Year", birthdate.Year);
		}

		public int GetAge()
		{
			DateTime birthdate;
			if (GetBirthdate(out birthdate))
			{
				return (DateTime.Now - birthdate).Days / 365;
			}
			return 0;
		}
	}
}
