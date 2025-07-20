using System.Collections.Generic;
using Kampai.Game.View;
using Kampai.Main;

namespace Kampai.Game.Controller.Audio
{
	public class PlayMinionStateAudioCommand
	{
		public enum StateParameter
		{
			Unselected = 0,
			GroupGacha = 1,
			Selected = 2,
			Deviant = 3
		}

		private const string CUE_PARAM_NAME = "Cue";

		private const string STATE_PARAM_NAME = "State";

		private static readonly Dictionary<StateParameter, float> stateLookup = new Dictionary<StateParameter, float>
		{
			{
				StateParameter.Unselected,
				1f
			},
			{
				StateParameter.GroupGacha,
				2f
			},
			{
				StateParameter.Selected,
				3f
			},
			{
				StateParameter.Deviant,
				2f
			}
		};

		private Dictionary<string, float> parameters = new Dictionary<string, float>(2);

		[Inject]
		public PlayLocalAudioSignal playLocalAudioSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		public void Execute(MinionStateAudioArgs args)
		{
			ActionableObject source = args.source;
			string audioEvent = args.audioEvent;
			string emitterKey = args.emitterKey;
			float cueId = args.cueId;
			StateParameter state = GetState(source);
			float value = 0f;
			if (stateLookup.TryGetValue(state, out value))
			{
				parameters["Cue"] = cueId;
				parameters["State"] = value;
				playLocalAudioSignal.Dispatch(source.GetAudioEmitter(emitterKey), audioEvent, parameters);
			}
		}

		private StateParameter GetState(ActionableObject source)
		{
			StateParameter result = StateParameter.Unselected;
			MinionObject minionObject = source as MinionObject;
			if (minionObject != null)
			{
				int iD = minionObject.ID;
				Minion byInstanceId = playerService.GetByInstanceId<Minion>(iD);
				if (byInstanceId != null)
				{
					MinionState state = byInstanceId.State;
					MinionObject.MinionGachaState gachaState = minionObject.GachaState;
					switch (gachaState)
					{
					case MinionObject.MinionGachaState.Deviant:
						result = StateParameter.Deviant;
						break;
					case MinionObject.MinionGachaState.Active:
						result = StateParameter.GroupGacha;
						break;
					default:
						if (state == MinionState.Selected || gachaState == MinionObject.MinionGachaState.IndividualTap)
						{
							result = StateParameter.Selected;
						}
						break;
					}
				}
			}
			return result;
		}
	}
}
