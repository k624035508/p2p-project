import { ajax } from "jquery";

export const UPDATE_BANK_CARDS = "UPDATE_BANK_CARDS"

export function updateBankCards(bankCards) {
	return {
		type: UPDATE_BANK_CARDS,
		bankCards
	};
}

export function fetchBankCards() {
	return function (dispatch) {
		let url = USER_CENTER_ASPX_PATH + "/AjaxQueryBankAccount"
		return ajax({
			type: "get",
			dataType: "json",
			contentType: "application/json",
			url: url,
			data: "",
			success: function(result) {
				let data = JSON.parse(result.d);
				dispatch(updateBankCards(data));
			}.bind(this),
			error: function(xhr, status, err) {
				console.error(url, status, err.toString());
			}.bind(this)
		});
	};
}

export function appendBankCard(cardNumber, bankName, bankLocation, openingBank) {
	return function (dispatch) {
		let url = USER_CENTER_ASPX_PATH + "/AjaxAppendCard";
		return ajax({
			type: "post",
			dataType: "json",
			contentType: "application/json",
			url: url,
			data: JSON.stringify({
				cardNumber,
				bankName,
				bankLocation: bankLocation.join(";"),
				openingBank
			}),
			success: function (data) {
				alert(data.d);
				dispatch(fetchBankCards());
			}.bind(this),
			error: function (xhr, status, err) {
				alert(xhr.responseJSON.d);
				console.error(url, status, err.toString());
			}.bind(this)
		});
	};
}