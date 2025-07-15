var Auth = new Vue({
    el: '#auth',
    data:{
        active: false,
        page: 0,
        login: '',
        code: '',
        restoreActive: false,
        email: '',
        password: '',
        password2: '',
        remember: false,
        type1: 'password',
        type2: 'password',
		user: "",
		
		allFacts: 
		[
		"- А вы знали? Изначально сервер хотели сделать в Майнкрафте!",
		"- Человеческий мозг генерирует за день больше электрических импульсов, чем все телефоны мира вместе взятые.",
		"- Низкочастотный крик горбатого кита - самый громкий звук, изданный живым существом.",
		"- Забавный факт! Наши админы настоящие танкисты",
		"- А вы знали? Арахис используется в производстве динамита.",
		"- Предложение: 'The quick brown fox jumps over the lazy dog' содержит все буквы английского алфавита.",
		"- Забавный факт! Сервер изначально хотел быть написан на YoptaScript!",
		"- В древнем Риме, если пациент умирал во время операции, врачу отрезали руки.",
		"- А вы знали, что Коровы мычат с региональным акцентом?",
		"- Покупателем взрывчатых веществ № 1 в мире являются военные США. № 2 — Дисней.",
		"- Если съесть полкило миндаля, можно отравиться цианидом.",
		"- Забавно! Когда форум был готов, Один из разработчиков его случайно удалил",
		"- Бикини и тампоны изобрели мужчины.",
		"- Мать Гитлера хотела сделать аборт, но врачи ее отговорили.",
		"- Джордж Вашингтон выращивал марихуану в своем саду.",
		"- Презерватив – вещь уникальная. Он создан людьми для людей и от людей.",
		"- Если умножить 1111111 на 1111111 то получится 1234567654321",
		],
		indexFact: 0,
		tryes: 3,
		blockControls: true,
		respawn: {
			org: ["Государственная","lspd","Офицер"],
		    home: [12,"люкс",400],
		    lastLocation: ["Cafe Crusual-Fix",64,42],
		    isHome: true,
		    isOrg: false,
		    isFamily: false,
			street: "Cade Crusual-Fix",
		},
    },
	mounted: function() {
		document.addEventListener('keyup', this.keyUp);
		document.addEventListener('keydown', this.keyDown);
	},
	methods: {
		keyUp: () => {
			// if (event.keyCode == 13) { 
				// Auth.changePage(1);
				// Auth.pressButton(document.querySelector('.login_load_pressenter'), true);
			// }
		},
		keyDown: () => {
			// if (event.keyCode == 13) { 
				// Auth.pressButton(document.querySelector('.login_load_pressenter'), false);
			// }
		},
		openauth: function(a) {
			this.active = true;
			this.user = a;
			this.blockControls = false;
		},
		changePage: function(a) {
			this.page = a;
		},		
        restore: function(a){
            if(a == "1"){
                mp.trigger("restorepass", 0, this.login)
            }
            else{
                mp.trigger("restorepass", 1, this.code)
            }
        },
        play: function(){
            mp.trigger("signin", this.login, this.password, true)
        },
        registration: function(){
            mp.trigger("signup", this.login, this.email, this.password, this.password2)
        },
		pressButton: function(pressButtonContainer, pressed) {
            let className = 'active';
            if (pressed) pressButtonContainer.classList.add(className)
            else pressButtonContainer.classList.remove(className)
        },
		set: function(data, pos) {
			this.page = 4;
			this.respawn.isOrg = data[0][1];
			this.respawn.isHome = data[0][2];
			this.respawn.org = data[1];
			this.respawn.home = data[2];
			this.respawn.lastLocation = data[3];
			this.respawn.street = pos;
		},
		selectSpawn: function(id) {
			if (this.blockControls) return;
			this.active = false;
			this.blockControls = true;
			setTimeout(() => {
				if (id == 2)
					if (this.respawn.isHome)
						mp.trigger("spawn", id);
					else
						return;
				if (id == 1)
					if (this.respawn.isOrg)
						mp.trigger("spawn", id);
					else 
						return;
				if (id == 0)
					mp.trigger("spawn", id);
			}, 500);
		},
    }
})
Auth.indexFact = Math.floor(Math.random() * Auth.allFacts.length)