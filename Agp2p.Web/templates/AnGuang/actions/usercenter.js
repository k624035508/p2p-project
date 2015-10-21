export const UPDATE_WALLET_INFO = "UPDATE_WALLET_INFO"
export const UPDATE_USER_INFO = "UPDATE_USER_INFO"
export const UPDATE_USER_INFO_BY_NAME = "UPDATE_USER_INFO_BY_NAME"

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