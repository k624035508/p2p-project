import React from "reace";
import CityPicker from "../components/city-picker.jsx";
import appendAddress from "../actions/order_address.js";

class AddressEditor extends React.Component {
    constructor(props) {
        super(props);
        this.state = this.genStateByValue(props.defaultValue);
    }


}