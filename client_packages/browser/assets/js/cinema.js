var cinema = new Vue({
    el: ".cinema",
    data: {
		active: false,
		url: "",
		votes: 0,
		maxvotes: 4,
		time: 0,
		playing: false,
		skipped: false,
		modal: false,
		admin: false,
		input: "",
		allurls: [],
	},
	methods: {
		opencinema: function(url, time, urls, playing, admin, vote, votes) {
			this.skipped = false;
			this.url = url;
			this.time = time;
			this.allurls = urls;
			this.playing = playing;
			this.admin = admin;
			this.votes = vote;
			this.maxvotes = votes;
			for (var i = 0; i < this.allurls.length; i++) {
				get(this.allurls[i].URL, i);
			}
			if (url != null && url.URL != null) {
				$('.cinema_screen').html(`			
					<iframe 
						src="https://www.youtube.com/embed/${this.url.URL}?start=${this.time}&rel=0&modestbranding=1&autohide=1&showinfo=0&controls=0&autoplay=1&vq=hd720&disablekb=1"  
						frameborder="0" 
						allowfullscreen
						class="cinema-video">
					</iframe>`);
			}
		},
		sendinfo: function(time, url, urls, votes, votesm) {
			this.time = time;
			this.url = url;
			this.allurls = urls;
			this.votes = votes;
			this.maxvotes = votesm;
			
			for (var i = 0; i < this.allurls.length; i++) {
				get(this.allurls[i].URL, i);
			}
			this.$forceUpdate();
		},
		setvote: function(vote, votesm) {
			this.votes = vote;
			this.maxvotes = votesm;
		},
		skipvideo: function(time, url, urls, vote, votesm) {
			this.skipped = false;
			this.time = time;
			this.allurls = urls;
			this.votes = vote;
			this.maxvotes = votesm;
			for (var i = 0; i < this.allurls.length; i++) {
				get(this.allurls[i].URL, i);
			}
			if (this.url == null || (url != null && url.URL != null)) {
				this.url = url;
				$('.cinema_screen').html(`			
				<iframe 
					src="https://www.youtube.com/embed/${this.url.URL}?start=${this.time}&rel=0&modestbranding=1&autohide=1&showinfo=0&controls=0&autoplay=1&vq=hd720&disablekb=1"  
					frameborder="0" 
					allowfullscreen
					class="cinema-video" allow="accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture">
				</iframe>`);
			}
		},
		skip: function() {
			if (this.allurls.length == 0 || this.skipped) return;
			this.skipped = true;
			mp.trigger('client::cinema:skip');
		},
		btnurl: function() {
			mp.trigger("client::cinema:btnurl", this.input);
		},
		exit: function() {
			this.active = false;
			var iframes = document.querySelectorAll('iframe');
			for (var i = 0; i < iframes.length; i++) {
				iframes[i].parentNode.removeChild(iframes[i]);
			}
			mp.trigger('client::cinema:close');
		}
	},
});

function get(url, id)
{
    $.ajax({
    url: "https://www.googleapis.com/youtube/v3/videos?id=" + url + "&key=AIzaSyDR5pWL6JnfIMGhenJ73jDEhyvcACndgCo&fields=items(snippet(title))&part=snippet", 
    dataType: "jsonp",
        success: function(data){
            cinema.allurls[id].NameVideo = data.items[0].snippet.title;
        },
    
    });
}