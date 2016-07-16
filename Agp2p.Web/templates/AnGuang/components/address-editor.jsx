import React from "react";
import { ajax } from "jquery";
import CityPicker from "../components/city-picker.jsx";
import {appendAddress, modifyAddress} from "../actions/order_address.js";

class AddressEditor extends React.Component {
    constructor(props) {
        super(props);
        this.state = this.genStateByValue(props.defaultValue);
    }
    componentWillReceiveProps(nextProps) {
        this.setState(this.genStateByValue(nextProps));
    }
    genStateByValue(val) {
        var state = {
            address: "",
            selectedLocation: "",
            postalCode: "",
            orderName: "",
            orderPhone: ""
        };
        if (val) {
            state.address = val.orderAddress;
            state.postalCode = val.postalCode;
            state.orderName = val.orderName;
            state.orderPhone = val.orderPhone;
        }
        return state;
    }
    doSaveAddress() {
        if (!this.props.value) {
            this.props.dispatch(appendAddress(this.state.address,
                this.state.selectedLocation,
                this.state.postalCode,
                this.state.orderName,
                this.state.orderPhone));
        } else {
            this.props.dispatch(modifyAddress(this.props.addressId,this.state.address, this.state.postalCode,this.state.orderName,this.state.orderPhone)); 
        }
    }
    render() {
        let createAddress = !this.props.value, editingAddress = !createAddress;
        return (
            <div className="modal fade" id="addressConfirm"  role="dialog" aria-labelledby="addressConfirmLabel" data-backdrop="static">
                <div className="modal-dialog addressConfirm-dialog" role="document">
                    <div className="modal-content">
                        <div className="modal-header">
                            <h4 className="modal-title" id="myModalLabel">{createAddress ? "新增" : "修改"}收货地址</h4>
                            <span className="close" data-dismiss="modal" aria-label="Close">&times; </span>
                        </div>
                        <div className="modal-body">
                            <ul className="list-unstyled">
                                <li><span><i>*</i>所在地区</span>
                                <CityPicker value={this.state.selectedLocation.split(",")} onLocationChanged={(...args) => this.setState({selectedLocation: [...args].join(",")})}/> 
                                </li>
                                <li className="dizhi"><span><i>*</i>详细地址</span><textarea rows="3" value={this.state.address} onChange={ev => this.setState({address:ev.target.value})}
                                    placeholder="建议您如实填写详细收货地址，例如街道名称，门牌号码，楼层和房间号码等信息" /></li>
                                <li><span><i>*</i>邮政编码</span><input type="text" value={this.state.postalCode} onChange={ev => this.setState({postalCode:ev.target.value})}/></li>
                                <li><span><i>*</i>收货人姓名</span><input type="text" value={this.state.orderName} onChange={ev => this.setState({orderName:ev.target.value})}/></li>
                                <li><span><i>*</i>手机号码</span><input type="text" value={this.props.orderPhone} onChange={ev => this.setState({orderPhone:ev.target.value})}/></li>
                            </ul>
                            <a href="javascript:" onClick={ev => this.doSaveAddress() }>保 存</a>
                        </div>
                    </div>
                </div>
            </div>
        );
    }
}

function mapStateToProps(state) {
	return state.userInfo;
	}

import { connect } from 'react-redux';
export default connect(mapStateToProps)(AddressEditor);
