var circleDesc = {
    "handshake": "🤝 Пожать руку",
    "licenses": "📄 Показать лицензии",
    "carinv":"📦 Посмотреть багажник",
    "carpetrol":"🚘 Заправить транспорт",
    "trunks":"🚘 Залесть в багажник",
    "doors":"🔐 Двери",
    "fraction":"🔫 Фракция",
    "offer":"🖐 Предложить обмен",
    "givemoney":"💰 Передать деньги",
    "heal":"💊 Вылечить",
    "hood":"🚙 Капот",
    "leadaway":"👉 Вести за собой",
    "offerheal":"💉 Предложить лечение",
    "passport":"💳 Показать документы",
    "search":"😒 Обыскать",
    "sellkit":"💊 Продать аптечку",
    "takegun":"🔫 Изъять оружие",
    "takeillegal":"💣 Изъять нелегал",
    "trunk":"🚙 Багажник",
    "pushborkencar":"🚘 Толкать авто",
    "pocket": "🛍 Мешок",
    "takemask": "🎭 Сорвать маску",
    "rob": "🤬 Ограбить",
    "house": "🏡 Дом",
    "apart": "🏡 Квартира",
    "ticket": "💵 Выписать штраф",

    "sellcar": "🚘 Продать машину",
    "sellhouse": "🏠 Продать дом",
    "roommate": "👋 Заселить в дом",
    "invitehouse": "🤝 Пригласить в дом",
	
	"givelicgun": "✉️ Выдать лицензию на оружие",
	"givemedlic": "✉️ Выдать медицинскую карту",
	"sendkpz" : "👮 Посадить в КПЗ",
	"socialpage" : "🤝Социальное",
	"kiss" : "💋Поцеловать",
	"saveposauto" : "🚙 Припарковать авто",
	"carry": "👨‍👦 Взять на руки",
	"changenum": "🚗 Снять номер",
    "repair" : "🧰 Починить авто",

    "sellapart": "🏠 Продать квартиру",
    "roommatea": "👋 Заселить в квартиру",
    "inviteapart": "🤝 Пригласить в квартиру",

    "plastic": "🚘 Показать пластик",
	"gplastic": "📦 Выдать пластик",
	"kpz": "👮‍♂️ Посадить в КПЗ",
	"family": "📃 Выдать сертификат",
	"bazar": "🚘 Автобазар",
    "dice": "🎲 Сыграть в кости",
}
var circleData = {
    "Игрок":
    [
        ["givemoney", "offer", "fraction", "passport", "licenses", "socialpage", "house", "apart"],
    ],
    "Машина":
    [
        ["hood", "trunk", "doors", "carinv", "repair", "trunks", "pushborkencar","saveposauto"],
    ],
    "Дом":
    [
        ["sellcar", "sellhouse", "roommate", "invitehouse"],
    ],
	"Квартира":
    [
        ["sellcar", "sellapart", "roommatea", "inviteapart"],
    ],
	"Семья":
	[
		["leadaway", "pocket", "rob", "takemask"],
	],
    "Социальное": 
    [
        ["heal", "plastic", "handshake", "dice", "kiss"],
    ],
    "Фракция":
    [
        [],
        ["leadaway", "pocket", "rob", "takemask"],
        ["leadaway", "pocket", "rob", "takemask"],
        ["leadaway", "pocket", "rob", "takemask"],
        ["leadaway", "pocket", "rob", "takemask"],
        ["leadaway", "pocket", "rob", "takemask"],
        ["leadaway","ydov", "takemask", "search"], //ГОВ
        ["leadaway", "search", "takegun", "takeillegal", "takemask", "ticket","ydov","kpz"], //ПД
        ["sellkit", "offerheal","ydov"], //ЕМС
        ["leadaway", "search", "takegun", "takeillegal", "takemask","ydov"], //ФИБ
        ["leadaway", "pocket", "rob", "takemask"], //мафия 1
        ["leadaway", "pocket", "rob", "takemask"], //мафия 2
        ["leadaway", "pocket", "rob", "takemask"], //мафия 3
        ["leadaway", "pocket", "rob", "takemask", "family"], //мафия 4
        ["leadaway", "pocket", "takemask", "ydov", "search"], //Армия
		["leadaway","ydov"], // НОВОСТИ
		["leadaway"], //Наманабет
		["leadaway", "pocket", "search", "takeillegal", "takemask", "ydov", "takegun"], //Меризвейзер
		["leadaway", "pocket", "search", "takeillegal", "takemask", "ydov", "takegun"], //Gruppe6
    ]
}

var circle = new Vue({
    el: '.intercation',
    data: {
        active: false,
		data: ["hood", "trunk", "doors", "carinv", "repair", "trunks", "pushborkencar"],
        description: null,
        title: "title",
		circleDescs: circleDesc,
    },
    mounted: function() {
		document.addEventListener('keyup', this.keyUp);
	},
    methods: {
		keyUp: () => {
			if (!circle.active) return;
			if (event.keyCode == 27) 
			{
				mp.trigger("circleCallback", -1);
				circle.hide();
			}
		},
        set: function(t,id) {
            this.data = circleData[t][id]
            this.description = t
            this.title = t
        },
        btn: function(id) {
			if (this.data[id] == null) return;
            mp.trigger("circleCallback", Number(id));
            this.hide();
        },
        show: function(t, id) {
            this.active = true
            this.set(t, id)
        },
        hide: function() {
            this.active = false;
        }
    }
})