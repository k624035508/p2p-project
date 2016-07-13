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
                            <li key={index}>
                                <div className="dizhi">
                                    <span className="mingzi">{a.orderName}</span>
                                    <span className="shouji">{a.orderPhone}</span>
                                    <span className="xiugai"></span>
                                    <span className="shanchu" onClick={ev => this.deleteAddress(a.id)}></span>
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

function mapStateToProps(state) {
    var orderAddress = state.orderAddress;
    return { orderAddress };
}
    

import { connect } from 'react-redux';
export default connect(mapStateToProps)(Orders);

