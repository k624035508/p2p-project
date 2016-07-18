import React from "react";

import CityPicker from "../components/city-picker.jsx";
import { fetchAddress, deleteAddress} from "../actions/order_address.js";
import AddressEditor from "../components/address-editor.jsx";
import confirm from "../components/tips_confirm.js";

class Orders extends React.Component {
    constructor(props) {
        super(props);
        this.state = { addressInfoId: -1, addressSelectId: -1 };
    }
    componentWillMount() {
        this.props.dispatch(fetchAddress());
    }
    componentDidMount() {
        $(".addName").click(function () {
            $("#addressConfirm").modal();
        });
    }
    modifyAddress(index) {
        $("#addressConfirm").modal();
        this.setState({ addressInfoId: index });
    }
    deleteAddress(id) {
        var { addressId } = id;
        confirm("是否确认删除该地址？",
        () => {
            this.props.dispatch(deleteAddress(addressId));
        });
    }
    render() {
        return (
            <div className="order-wrap">
                <div className="goods-address">
                    <p>选择收货地址 </p>
                    <ul className="list-unstyled list-inline">
                        {this.props.orderAddress.map((a, index) => 
                            <li key={a.addressId} onClick={ev => this.setState({addressSelectId: this.state.addressSelectId == index ? -1 : index})}
                            className={this.state.addressSelectId == index ? "xuanzhong" : null}>
                                <input type="hidden" className="addressIdInfo" value={a.addressId} />
                                <div className="dizhi">
                                    <span className="mingzi">{a.orderName}</span>
                                    <span className="shouji">{a.orderPhone}</span>
                                    <span className="xiugai" onClick={ev => this.modifyAddress(index)}></span>
                                    <span className="shanchu" onClick={ev => this.deleteAddress(a)}></span>
                                </div>
                                <p className="jutidizhi">{a.area + a.orderAddress}</p>
                                <p className="youbian">{a.postalCode}</p>
                            </li>
                        )}
                            <li className="addName" onClick={ev => this.setState({addressInfoId: -1})}>                                
                            </li>
                    </ul>
                </div>
                <AddressEditor onOperationSuccess={() => this.setState({addressInfoId: -1})}
                            value={this.state.addressInfoId == -1 ? null : this.props.orderAddress[this.state.addressInfoId]} />
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

