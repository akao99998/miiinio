using System;

namespace Kampai.Common
{
	public interface ICoppaService
	{
		bool IsBirthdateKnown();

		bool GetBirthdate(out DateTime birthdate);

		bool Restricted();

		void SetUserBirthdate(DateTime birthdate);

		int GetAge();
	}
}
