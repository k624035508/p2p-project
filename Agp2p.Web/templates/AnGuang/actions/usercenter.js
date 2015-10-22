import { ajax } from "jquery";

export const UPDATE_WALLET_INFO = "UPDATE_WALLET_INFO"
export const UPDATE_USER_INFO = "UPDATE_USER_INFO"
export const UPDATE_USER_INFO_BY_NAME = "UPDATE_USER_INFO_BY_NAME"
export const FETCH_WALLET_AND_USER_INFO = "FETCH_WALLET_AND_USER_INFO"

export function updateWalletInfo(walletInfo) {
	return {
		type: UPDATE_WALLET_INFO,
		walletInfo
	};
}

export function updateUserInfo(userInfo) {
	return {
		type: UPDATE_USER_INFO,
		userInfo
	};
}

export function updateUserInfoByName(name, value) {
	return {
		type: UPDATE_USER_INFO_BY_NAME,
		name,
		value
	};
}

export function fetchWalletAndUserInfo() {
	return function (dispatch) {
		let url = USER_CENTER_ASPX_PATH + "/AjaxQueryUserInfo"
		return ajax({
			type: "get",
			dataType: "json",
			contentType: "application/json",
			url: url,
			data: "",
			success: function(result) {
				let data = JSON.parse(result.d);
				dispatch(updateWalletInfo(data.walletInfo));
				dispatch(updateUserInfo(data.userInfo));
			},
			error: function(xhr, status, err) {
				console.error(url, status, err.toString());
			}
		});
	};
}