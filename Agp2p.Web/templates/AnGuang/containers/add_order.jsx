import React from "react";

import CityPicker from "../components/city-picker.jsx";
import { fetchAddress, deleteAddress} from "../actions/order_address.js";
import AddressEditor from "../components/address-editor.jsx";

class Orders extends React.Component {
    constructor(props) {
        super(props);
        this.state = {};
    }
    componentWillMount() {
        this.props.dispatch(fetchAddress());
    }
    componentDidMount() {
        $(".addName").click(function () {
            $("#addressConfirm").modal();
        });
    }
    deleteAddress(id) {
        this.props.dispatch(deleteAddress(id));
    }
    render() {
        return (
            <div className="order-wrap">
                <div className="goods-address">
                    <p>选择收货地址 </p>
                    <ul className="list-unstyled list-inline">
                        {this.props.orderAddress.map((a, index) => 
                            <li key={a.addressId}>
                                <div className="dizhi">
                                    <span className="mingzi">{a.orderName}</span>
                                    <span className="shouji">{a.orderPhone}</span>
                                    <span className="xiugai"></span>
                                    <span className="shanchu" onClick={ev => this.deleteAddress(a.addressId)}></span>
                                </div>
                                <p className="jutidizhi">{a.orderAddress}</p>
                                <p className="youbian">{a.postalCode}</p>
                            </li>
                        )}
                            <li className="addName">                                
                            </li>
                    </ul>
                </div>
                <AddressEditor />
            </div>
        );
    }
}

class OrderAdding extends React.Component {
    constructor(props) {
        super(props);
    }
    render() {
        return(
            <div className="order-content">
            <p className="bill">商品清单</p>
            <table className="table order-tb">
                <thead>
                    <tr>
                        <th></th>
                        <th>商品名称</th>
                        <th>价格</th>
                        <th>属性</th>
                        <th>数量</th>
                        <th>小计</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td></td>
                        <td>
                            
                        </td>
                        <td>
                            
                        </td>
                        <td></td>
                        <td>
                            
                        </td>
                        <td>
                            
                        </td>
                    </tr>
                </tbody>
            </table>
            <p className="quantity">商品数量：
                件</p>
            <p className="points">应付积分：<i>积分</i></p>
            <a className="adding">提交订单</a>
            <p className="premise">* 须完成投资的用户才能兑换</p>
        </div>
            );
    }
}

function mapStateToProps(state) {
    var orderAddress = state.orderAddress;
    return { orderAddress };
}
    

import { connect } from 'react-redux';
export default connect(mapStateToProps)(Orders);
export default connect(mapStateToProps)(OrderAdding);

