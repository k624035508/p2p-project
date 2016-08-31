var o = {
	init: function(){
		this.diagram();
	},
	random: function(l, u){
		return Math.floor((Math.random()*(u-l+1))+l);
	},
	diagram: function() {
	    var r = Raphael('diagram', "260", "210"),
	        rad = 120, //弧度
	        defaultText = "123";
		
		r.customAttributes.arc = function(value, color, rad) {
		    var v = 3.6 * value * 70,
		        alpha = v == 360 ? 360 : v,
		        random = 215,
		        
				a = (random-alpha) * Math.PI/180,
				b = random * Math.PI/180,
				sx = rad+8 + rad * Math.cos(b),
				sy = rad+8 - rad * Math.sin(b),
				x = rad+8 + rad * Math.cos(a),
				y = rad+8 - rad * Math.sin(a),
				path = [['M', sx, sy], ['A', rad, rad, 0, +(alpha > 180), 1, x, y]];
			return { path: path, stroke: color }
		}
		
		$('.get').find('.arc').each(function(i){
			var t = $(this), 
				color = t.find('.color').val(),
				value = t.find('.percent').val(),
				text = t.find('.text').text();
			
			//rad += 30;	
			var z = r.path().attr({ arc: [value, color, rad], 'stroke-width': 8 });
		});
		
	}
}
$(function(){ o.init(); });
