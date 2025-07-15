var CreatorBlips = new Vue({
    el: ".CreatorBlips",
    data: {
		active: false,
		selectedID: -1,
		modal: {
			header: "Daddy",
			state: -1,
			model: null,
		},
		selectedItem: null,
		blips: [
		{
			ID: 1,
			BlipSettings: { Sprite: 1, Position: {x: 123, y: 250, z: 10}, Scale: 1, Color: 1, Name: "Made", Alpha: 255, ShortRange: false, Rotation: 0, Dimension: 0,}
		},
		]
    },
	 mounted: function() {
		document.addEventListener('keyup', this.keyUp);
	},
    methods: {
		keyUp: () => {
			if (!CreatorBlips.active) return;
			switch(event.keyCode)
			{
				case 27:
					CreatorBlips.exit();
				break;
			}
		},
		openModal: function(a, b) {
			this.modal.state = a;
			this.modal.header = b;
			switch(a) {
				case 0:
					this.modal.model = this.selectedItem.BlipSettings.Name;
				break;
				case 1:
					this.modal.model = this.selectedItem.BlipSettings.Sprite;
				break;
				case 2:
					this.modal.model = this.selectedItem.BlipSettings.Scale;
				break;
				case 3:
					this.modal.model = this.selectedItem.BlipSettings.Dimension;
				break;
				case 4:
					this.modal.model = this.selectedItem.BlipSettings.Color;
				break;
			}
		},
		setData: function() {
			switch(this.modal.state) {
				case 0:
					this.selectedItem.BlipSettings.Name = this.modal.model;
				break;
				case 1:
					this.selectedItem.BlipSettings.Sprite = parseInt(this.modal.model);
				break;
				case 2:
					this.selectedItem.BlipSettings.Scale = this.modal.model;
				break;
				case 3:
					this.selectedItem.BlipSettings.Dimension = parseInt(this.modal.model);
				break;
				case 4:
					this.selectedItem.BlipSettings.Color = parseInt(this.modal.model);
				break;
			}
			this.modal.state = -1;
		},
		ChangePosition: function() {
			mp.trigger('client::creatorblips:changeposition', this.selectedItem.ID)
		},
		selectBlip: function(a) {
			if (this.selectedID == a) {
				this.selectedID = -1;
				this.selectedItem = null;
				return;
			}
			
			this.selectedID = a;
			this.selectedItem = this.blips[a];
		},
		SaveBlips: function() {
			mp.trigger('client::creatorblips:setsettings', this.selectedItem.ID, JSON.stringify(this.selectedItem.BlipSettings));
		},
		CreateBlip: function() {
			mp.trigger('client::creatorblips:create');
		},
		RemoveBlip: function(a) {
			mp.trigger('client::creatorblips:remove', a);
			setTimeout(() => {
				this.selectedID = -1;
				this.selectedItem = null;
			}, 10);
		},
		TeleportToBlip: function(a) {
			mp.trigger('client::creatorblips:tptoblip', a);
		},
		exit: function() {
			mp.trigger('client::creatorblips:close');
		},
    }
});