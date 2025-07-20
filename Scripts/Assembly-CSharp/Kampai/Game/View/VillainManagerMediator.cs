using System.Linq;
using Kampai.Common;
using Kampai.Util;
using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.Game.View
{
	public class VillainManagerMediator : EventMediator
	{
		private BuildingManagerView buildingManagerView;

		[Inject]
		public VillainManagerView view { get; set; }

		[Inject]
		public VillainPlayWelcomeSignal welcomeSignal { get; set; }

		[Inject]
		public VillainGotoCarpetSignal carpetSignal { get; set; }

		[Inject]
		public VillainGotoCabanaSignal cabanaSignal { get; set; }

		[Inject]
		public CreateVillainViewSignal createViewSignal { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public INamedCharacterBuilder builder { get; set; }

		[Inject]
		public BuildingChangeStateSignal buildingChangeStateSignal { get; set; }

		[Inject]
		public RecreateBuildingSignal recreateSignal { get; set; }

		[PostConstruct]
		public void Init()
		{
			buildingManagerView = buildingManager.GetComponent<BuildingManagerView>();
		}

		public override void OnRegister()
		{
			welcomeSignal.AddListener(PlayWelcome);
			carpetSignal.AddListener(GotoCarpet);
			cabanaSignal.AddListener(GotoCabana);
			createViewSignal.AddListener(CreateView);
		}

		public override void OnRemove()
		{
			welcomeSignal.RemoveListener(PlayWelcome);
			carpetSignal.RemoveListener(GotoCarpet);
			cabanaSignal.RemoveListener(GotoCabana);
			createViewSignal.RemoveListener(CreateView);
		}

		private void PlayWelcome(int id)
		{
			view.Get(id).PlayWelcome();
		}

		private void GotoCabana(int id, int cabanaId)
		{
			CabanaBuilding byInstanceId = playerService.GetByInstanceId<CabanaBuilding>(cabanaId);
			byInstanceId.Occupied = true;
			CabanaBuildingObject cabanaBuildingObject = buildingManagerView.GetBuildingObject(byInstanceId.ID) as CabanaBuildingObject;
			view.Get(id).GotoCabana(byInstanceId.ID, cabanaBuildingObject.GetRoutingPoint());
			buildingChangeStateSignal.Dispatch(byInstanceId.ID, BuildingState.Working);
			recreateSignal.Dispatch(byInstanceId);
		}

		private void GotoCarpet(int id)
		{
			NamedCharacterObject orCreateView = GetOrCreateView(id);
			orCreateView.transform.parent = base.transform;
			NoOpPlot noOpPlot = playerService.GetInstancesByType<NoOpPlot>().FirstOrDefault();
			orCreateView.setLocation(new Vector3(noOpPlot.Location.x, 0f, noOpPlot.Location.y));
		}

		private VillainView GetOrCreateView(int id)
		{
			NamedCharacterObject namedCharacterObject = view.Get(id);
			if (namedCharacterObject == null)
			{
				Villain byInstanceId = playerService.GetByInstanceId<Villain>(id);
				namedCharacterObject = builder.Build(byInstanceId, base.gameObject);
				view.Add(namedCharacterObject);
			}
			else
			{
				namedCharacterObject.gameObject.SetActive(true);
			}
			return namedCharacterObject as VillainView;
		}

		private void CreateView(int id)
		{
			GetOrCreateView(id);
		}
	}
}
