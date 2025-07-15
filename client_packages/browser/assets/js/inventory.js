Vue.component('item', {
    template: '<div class="item draggable" :class="items" :id="ids" :data-type="typ" v-bind:weight="(weight*count).toFixed(2)" :fastslot="fast_slot" :data-active="ativ" @click.right.prevent="select">\
    <img :src="src" onerror="imgError(this)"><a>{{count}}</a></div>',
    props: ['id', 'index', 'count', 'isactive', 'type', 'sub', 'fast_slot', 'datai', 'weari'],
    data: function () {
        return {
            src: getClothesItem(this.id) ? err(this) : './assets/images/inventory/items/' + this.id + '.png',
            name: getClothesItem(this.id) ? getNameClothes(getListID(this.id.toString()), this.datai) : itemsData[this.id],
            ids: this.index,
            typ: this.type,
            ativ: (this.isactive == 1) ? `active` : 0,
            items: `i${this.id}a${this.isactive}`,
        }
    },
    methods: {
        select: function (event) {
			if(board.sType == (this.type == 'inv') ? 1 : 0){
                if(board.lIndex == this.ids){
                    context.hide()
                }else{
					board.sType = (this.type == 'inv') ? 1 : (this.type == 'out') ? 0 : (this.type == 'eq') ? 3 : 2 ;
                    board.sID = this.id;
					board.sIndex = this.ids;
					board.lIndex = this.ids;
                    context.type = (this.type == 'inv') ? 1 : (this.type == 'out') ? 0 : (this.type == 'eq') ? 3 : 2 ;
                    board.context(event, this)
                }
            }else{
				if(board.lIndex == this.ids){
                    context.hide()
                }
				else {
					board.sType = (this.type == 'inv') ? 1 : (this.type == 'out') ? 0 : (this.type == 'eq') ? 3 : 2 ;
					board.sID = this.id;
					board.sIndex = this.ids;
					board.lIndex = this.ids;
					context.type = (this.type == 'inv') ? 1 : (this.type == 'out') ? 0 : (this.type == 'eq') ? 3 : 2 ;
					board.context(event, this)
				}
            }
        }
    }
})
function imgError(image) {
	image.onerror = "";
	image.src = "https://multiproject.pw/img/inventory/items/error.svg";
	return true;
}
var board = new Vue({
    el: ".inventory_block",
    data: {
        active: false,
        outside: false,
        outType: 0,
        outHead: "",
		
        stats: [100, 80, 100],
        items: [[-1, 5, 1, "", 99, '1_0_True', 200, -1],[11, 100, 0, "", 100, "", 200, -1], [64, 500, 0, "", 100, "", 200, -1], [11, 100, 0, "", 100, "", 200, -1], [59, 10, 0, "", 100, "", 200, -1], [10, 500, 0, "", 100, "", 200, -1], [11, 100, 0, "", 100, "", 200, -1],[114, 5, 1, "GUNS200, -12353452", 100, "", 1500, 1], [5, 10, 0, "", 100, "", 200, -1], [10, 500, 0, "", 100, "", 200, -1], [11, 100, 0, "", 100, "", 200, -1], [10, 500, 0, "", 100, "", 200, -1], [11, 100, 0, "", 100, "", 200, -1], [5, 10, 0, "", 100, "", 200, -1], [10, 500, 0, "", 100, "", 200, -1]],
		outitems: [[11, 100, 0], [1000, 10, 0], [10, 500, 0], [11, 100, 0], [5, 10, 0], [10, 500, 0], [11, 100, 0],[114, 5, 1], [5, 10, 0], [10, 500, 0], [11, 100, 0], [1000, 10, 0], [10, 500, 0], [11, 100, 0], [5, 10, 0], [10, 500, 0]],
        itemsground: [[1, 5, 0, "", 99, ''], [1, 5, 0, "", 99, '']],
		
		sIndex: 0,
		lIndex: 0,
        sType: 0,
        sID: 0,
        key: 0,
		
		weight: 20,
		maxWeight: 20,
		maxWeightOut: 120,
		maxWeightBacks: 50,
		weightOut: 0,
		
		nameplayer: "Ilya Merumond",
		
		clothesitem: [-1,-2,-3,-4,-5,-6,-7,-8,-9,-10,-11,-12,-13,-14]
    },
    methods: {
        context: function (event, thiss) {
            if (clickInsideElement(event, 'item')) {
				let pageY = event.pageY;
				if (event.pageY > 700) {
					pageY -= 100;
				}
                context.show(event.pageX, pageY, thiss);
            } else {
				context.hide()
            }
        },
        hide: function (event) {
			context.hide()
        },
        outSet: function (json) {
            this.key++
            this.outType = json[0]
            this.outHead = json[1]
            this.outitems = json[2]
			this.maxWeightOut = json[3]
            this.weightOut = json[4].toFixed(1)
        },
        itemsSet: function (table, json, itemground) {
            this.key++
            this.items = json
			this.weight = table.toFixed(1);
			this.itemsground = itemground;
        },
        itemUpd: function (index, data, to, itemground) {
            this.key++
            this.items[index] = data
			this.weight = to.toFixed(1);
			this.itemsground = itemground;
        },
        send: function (id) {
            let type = (this.sType) ? 0 : this.sType == 3 ? 0 : this.outType
            mp.trigger('boardCB', id, type, this.sIndex)
        },
        shieldact: function (id, type, index) {
			if (id >= 14 && id <= 18) {
				mp.trigger('client::inventory:setslot', index, id - 13);
				return;
			}
			if (type >= 14 && type <= 18) {
				if (id == 1)
					mp.trigger('client::inventory:removeslot', index);
				else
					mp.trigger('notify', 2, 9, "Сначала достаньте предмет из быстрого слота", 3000);
				return;
			}
			if (id == 3 && type == 10) {
				mp.trigger('boardCB', 10, 10, index)
			}
			if (type != 5)
				mp.trigger('boardCB', id, type, index)
			else {
				mp.trigger('boardCB', 5, 0, index)
			}
        }
    }
})
var context = new Vue({
    el: ".infoitem",
    data: {
        active: false,
        style: '',
        type: true,
        name: "Ключ",
        sub: "Серийник",
		selectitem: null,
		dataSub: null,
    },
    methods: {
        show: function (x, y, thiss) {
			this.dataSub = null;
            this.style = `left:${x+25}px;top:${y-50}px;`
            this.name = getClothesItem(thiss.id) ? getNameClothes(getListID(thiss.id.toString()), thiss.datai) : itemsData[thiss.id] || "Null";
            this.sub = sub[thiss.id] || "нет информации";
			this.active = true;
			this.selectitem = thiss;
            if(sub) this.sub = sub[thiss.id];
            else this.sub = `нет информации`;
            let data = board.items[board.sIndex][3];
            if(data.length) this.dataSub = `${data}`;
        },
        hide: function () {
            this.active = false;
            this.name = "";
            this.sub = "";
            board.sType = null;
            board.sID = null;
            board.lIndex = null;
			this.dataSub = null;
        },
        btn: function (id) {
            board.send(id)
            this.hide()
        }
    }
})

var rangeslider = new Vue({
    el: ".rangeslider",
    data: {
        active: false,
        type: true,
        max: 100,
		count: 1,
    },
    methods: {
        btn: function () {
            mp.trigger('rangesliderboard', this.count)
        },
		exit: function () {
			this.hide()
		},
		hide: function () {
            this.active = false;
            this.max = 0;
            this.count = 0;
            board.sType = null;
            board.sID = null;
            board.sIndex = null
        },
    }
})

function getTypeClothes(a) {
	var result = "";
	if (a == '-1') {
		result = "masks"
	}
	if (a == '-3') {
		result = "gloves"
	}
	if (a == '-4') {
		result = "legs"
	}
	if (a == '-5') {
		result = "bags"
	}
	if (a == '-6') {
		result = "shoes"
	}
	if (a == '-7') {
		result = "accessories"
	}
	if (a == '-8') {
		result = "tops"
	}
	if (a == '-9') {
		result = "shoes" //armor
	}
	if (a == '-10') {
		result = "watches"
	}
	if (a == '-11') {
		result = "tops"
	}
	if (a == '-12') {
		result = "hats"
	}
	if (a == '-13') {
		result = "glasses"
	}
	if (a == '-13') {
		result = "bracelets"
	}
	return result;
}
function getClothesItem(a) {
	if (a == '-1' || a == '-2' || a == '-3' || a == '-4' || a == '-5' || a == '-6' || a == '-7' || a == '-10' || a == '-11' || a == '-12' || a == '-13' || a == '-14')
		return true;
	else
		return false;
}

function getNameClothes(type, a) {
	var result = type[Number(a.split('_')[0])] != null ? type[Number(a.split('_')[0])][Number(a.split('_')[1])] != null ? type[Number(a.split('_')[0])][Number(a.split('_')[1])].Name_RU : null : null;
	if (result != undefined && result != "NULL")
		return result;
	else
		return "Комплект #" + Number(a.split('_')[0]);
};

function getGender(a) {
	if (a == "True") {
		return true;
	}
	if (a == "False") {
		return false;
	}
}

function getGenderName(a) {
	var genders = null;
	if (a == "True") {
		genders = "male";
	}
	if (a == "False") {
		genders = "female";
	}
	return genders;
}

function getListID(a) {
	switch(a) {
		case "-12":
			return getGender(a) ? MaleHats : FemaleHats;
		case "-11":
			return getGender(a) ? MaleTops : FemaleTops;
		case "-1":
			return MasksNames;
		case "-5":
			return BagsNames;
		case "-8":
			return getGender(a) ? MaleTops : FemaleTops;
		case "-4":
			return getGender(a) ? MaleLegs : FemaleLegs;
		case "-6":
			return getGender(a) ? MaleShoes : FemaleShoes;
		case "-3":
			return getGender(a) ? MaleGloves : FemaleGloves;
		case "-14":
			return getGender(a) ? MaleWatches : FemaleWatches;
		case "-13":
			return getGender(a) ? MaleGlasses : FemaleGlasses;
		case "-7":
			return getGender(a) ? MaleAccessoires : FemaleAccessoires;
	};
}

function err(a) {
	var img = new Image();
	img.src = "https://cloud.redage.net/cloud/inventoryItems/clothes/" + getGenderName(a.datai.split('_')[2]) + "/" + getTypeClothes(a.id) + "/" + a.datai.split('_')[0] + "_" + a.datai.split('_')[1] + ".png";
	img.onload = function(){a.src="https://cloud.redage.net/cloud/inventoryItems/clothes/" + getGenderName(a.datai.split('_')[2]) + "/" + getTypeClothes(a.id) + "/" + a.datai.split('_')[0] + "_" + a.datai.split('_')[1] + ".png"};
	img.onerror = function(){a.src='./assets/images/inventory/items/' + a.id + '.png'};
}

function clickInsideElement(e, className) {
    var el = e.srcElement || e.target;
    if (el.classList.contains(className)) {
        return el;
    } else {
        while (el = el.parentNode) {
            if (el.classList && el.classList.contains(className)) {
                return el;
            }
        }
    }
    return false;
}

DragManager.onDragCancel = function(dragObject) {
    dragObject.avatar.rollback();
};

DragManager.onDragEnd = function(dragObject, dropElem, action) {
	var sindex = $(dragObject.elem).attr('id')
	var temptype = $(dragObject.elem).data('type')
	var type;
	if(temptype == 'inv')
		type = 0
	else if (temptype == 'fast_1') 
		type = 14;
	else if (temptype == 'fast_2') 
		type = 15;
	else if (temptype == 'fast_3') 
		type = 16;
	else if (temptype == 'fast_4') 
		type = 17;
	else if (temptype == 'fast_5') 
		type = 18;
	else if (temptype == 'eq') 
		type = 5;
	else {
		switch(board.outHead) {
			case "Багажник":
				type = 2;
			break;
			case "Шкаф с предметами":
				type = 3;
			break;
			case "Шкаф с одеждой":
				type = 4;
			break;
			case "Склад оружия":
				type = 6;
			break;
			case "Связка ключей":
				type = 7;
			break;
			case "Оружейный сейф":
				type = 8;
			break;
			case "Обмен":
				type = 8;
			break;
			case "Рюкзак":
				type = 10;
			break;
			case "Мусорка":
				type = 21;
			break;
		}
	}
	board.shieldact(action, type, sindex)    
	dragObject.avatar.rollback();  
};