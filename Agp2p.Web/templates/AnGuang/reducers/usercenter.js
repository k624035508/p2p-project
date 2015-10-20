import { UPDATE_WALLET_INFO, UPDATE_USER_INFO } from "../actions/usercenter.js"
import Immutable from "immutable"

const initialState = {
	walletInfo: { idleMoney: 0, lockedMoney: 0, investingMoney: 0, profitingMoney: 0},
	userInfo: {
		userName: "",
		prevLoginTime: "",
		nickName: "",
		realName: "",
		idCardNumber: "",
		mobile: "",
		email: "",
		qq: "",
		sex: "",
		birthday: "",
		area: "",
		address: "",
	}
}

export default function userCenter(state = initialState, action) {
	switch (action.type) {
	case UPDATE_WALLET_INFO:
		return Immutable.fromJS(state).mergeIn([ "walletInfo" ], action.walletInfo).toJS();
	case UPDATE_USER_INFO:
		return Immutable.fromJS(state).mergeIn([ "userInfo" ], action.userInfo).toJS();
	default:
		return state;
	}
}