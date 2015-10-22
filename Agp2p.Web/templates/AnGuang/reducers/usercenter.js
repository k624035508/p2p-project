import { UPDATE_WALLET_INFO, UPDATE_USER_INFO, UPDATE_USER_INFO_BY_NAME } from "../actions/usercenter.js"
import Immutable from "immutable"

const initialState = {
	walletInfo: { idleMoney: 0, lockedMoney: 0, investingMoney: 0, profitingMoney: 0},
	userInfo: {
		userName: "",
		prevLoginTime: "",
		realName: "",
		idCardNumber: "",
		nickName: "",
		mobile: "",
		email: "",
		qq: "",
		sex: "",
		birthday: "",
		area: "",
		address: "",
		hasTransactPassword: ""
	}
}

export default function userCenter(state = initialState, action) {
	switch (action.type) {
	case UPDATE_WALLET_INFO:
		return Immutable.fromJS(state).mergeIn([ "walletInfo" ], action.walletInfo).toJS();
	case UPDATE_USER_INFO:
		return Immutable.fromJS(state).mergeIn([ "userInfo" ], action.userInfo).toJS();
	case UPDATE_USER_INFO_BY_NAME:
		return Immutable.fromJS(state).setIn([ "userInfo", action.name ], action.value).toJS();
	default:
		return state;
	}
}