var AnimMenu = new Vue({
    el: '.animation',
    data:{
        active: false,
		cats: ["Действия","Неприличные","Социальные","Стойки","Танцы","Физ. упражнения","Эмоции","Стили походки","Эксклюзивные"],
		ListAnimation: animationList,
		select: 0,
		search: "",
		played: false,
		selectedAnim: null,
		TimeOutAnim: null,
		blockControl: false,
		fastSlots: 
		[
		null,
		null,
		null,
		null,
		null,
		],
		stateSelectFast: false,
		idSelectFast: -1,
		blockAction: false,
    },
    mounted: function() {
		document.addEventListener('keyup', this.keyUp);
	},
    methods: {
        keyUp: () => {
            if (!AnimMenu.active) return
            if (event.keyCode == 27) AnimMenu.exit();
        },
		open: function(fs) {
			this.active = true;
			this.fastSlots = fs;
		},
		setCat: function(a) {
			if (this.search != '') return;
			this.select = a;
		},
		getList: function(a) {
			return animationList.find(x => x.id == a).animations;
		},
		btnSelectSlot: function(a) {
			if (this.fastSlots[a-1] != null || this.blockAction) return;
			this.stateSelectFast = true;
			this.idSelectFast = a-1;
		},
		removeFS: function(a) {
			if (this.fastSlots[a-1] == null) return;
			this.fastSlots[a-1] = null;
			this.$forceUpdate();
			this.blockAction = true;
			setTimeout(() => {
				this.blockAction = false;
			}, 100);
			mp.trigger('client::animmenu:saveslots', JSON.stringify(this.fastSlots));
		},
		btn: function(a) {
			if (this.stateSelectFast == true) {
				this.fastSlots[this.idSelectFast] = a;
				this.idSelectFast = -1;
				this.stateSelectFast = false;
				this.$forceUpdate();
				mp.trigger('client::animmenu:saveslots', JSON.stringify(this.fastSlots));
				return;
			}
			if (this.blockControl)return;
			this.blockControl = true;
			this.selectedAnim = a;
			if (this.select == 7) {
				mp.trigger('client::animmenu:walk', JSON.stringify(a));
				return;
			}
			mp.trigger('client::animmenu:dance', JSON.stringify(a));
			clearTimeout(this.TimeOutAnim);
			this.played = true;
			this.TimeOutAnim = setTimeout(() => {
				this.played = false;
				this.blockControl = false;
			}, 3000);
		},
		exit: function() {
			mp.trigger('client::animmenu:close');
		}
    }
});