var menu = new Vue({
    el: "#app",
    data: {
		active: false,
		style: 0,
		namequest: "Первое знакомство",
		descquest: "Сюжетное",
		money: 150,
		exp: 1,
		questnum: 0,
		itemsbonus: [603,0,0,0],
		itemsbonustwo: [0,0],
		qtext: [
		[
			"Вы сможете помочь Эмме на ферме, ее сестра посвятит Вас в детали работы на ферме и Вы получите свой первый заработок.",
		],
		[
			"Эмма явно не была рада, узнав что Вы взяли мопед без прав. Вы сможете получить права в центре лицензирования."
		],
		],
		quests: [
		[
			'Посетить Эмму Йонг и узнать о задании',
			'Поговорить с сестрой Эммы на ферме',
			'Поговорить с Робертом Маккензи',
			'Соберите 0 / 5 корзинок на ферме',
			'Сдать задание Аманде Йонг',
		],
		[
			'Посетить Аманду Йонг и узнать о задании',
			'Отправиться в центр лицензирования',
			'Получите права категории DRIVE D',
			'Посетите друга Аманды',
		],
		],
    },
	mounted: function() {
		// if (this.active == true) {
			document.addEventListener('keyup', this.keyUp);
		// }
	},
    methods: {
		keyUp(event) {
		  if(event.keyCode == 13) this.take();
		  if(event.keyCode == 27) this.exit();
		},
		set: function(state, namequest, descquest, money, exp, itemsbonus, itemsbonustwo, questnum) {
			this.active = state;
			this.namequest = namequest;
			this.descquest = descquest;
			this.money = money;
			this.exp = exp;
			this.itemsbonus = itemsbonus;
			this.itemsbonustwo = itemsbonustwo;
			this.questnum = questnum;
		},
		exit: function() {
			this.active = false;
			mp.trigger('client::closemenucheckquest');
		},
		take: function() {
			this.active = false;
			mp.trigger('client::takequestcheckmenu', this.questnum);
		},
    }
});