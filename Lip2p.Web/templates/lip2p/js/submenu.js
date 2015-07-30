$(document).ready(function(){
	$(document).off('click.bs.dropdown.data-api');
	
	dropdownOpen();
});

function dropdownOpen() {
	var $dropdownLi = $('li.dropdown');

	$dropdownLi.mouseover(function() {
			$(this).addClass('open');
	}).mouseout(function() {
			$(this).removeClass('open');
	});
}
