export const REFRESH_USER_INFO = "REFRESH_USER_INFO"

export default function refreshUserInfo(userInfo) {
	return {
		type: REFRESH_USER_INFO,
		userInfo
	};
}