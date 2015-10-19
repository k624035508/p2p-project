import { REFRESH_USER_INFO } from "../actions/usercenter.js"
import Immutable from "immutable"

const initialState = {
	userInfo: { totalMoney: 0, userName: "", prevLoginTime: "", idleMoney: 0, lockedMoney: 0}
}

export default function userCenter(state = initialState, action) {
	switch (action.type) {
	case REFRESH_USER_INFO:
		return Immutable.fromJS(state).mergeIn([ "userInfo" ], action.userInfo).toJS();
	default:
		return state;
	}
}