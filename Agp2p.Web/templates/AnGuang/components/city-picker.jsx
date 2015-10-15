import React from "react";
import $ from "jquery";
import { citylist } from "../js/city.min.js";

export default class CityPicker extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            province: citylist.map(pobj => pobj.p),
            city: [],
            area: [],
            currentProvinceIndex: -1,
            currentCityIndex: -1,
            currentAreaIndex: -1
        };
    }
    onProvinceSelected(selectedIndex) {
        if (selectedIndex == -1) {
            this.setState({city: [], currentProvinceIndex: -1})
        } else {
            var cities = citylist[selectedIndex].c.map(c => c.n);
            this.setState({city: cities, currentProvinceIndex: selectedIndex});
        }
    }
    render() {
        return (
            <div className="selectDist">
                <select className="form-control-custom prov" id="province" name="province" onChange={ev => this.onProvinceSelected(ev.target.value)}>
                    <option value="-1">请选择省</option>
                    {this.state.province.map((p, index) => {
                        return <option value={index}>{p}</option>
                    })}
                </select>
                <select className="form-control-custom city" id="province" name="city">
                    <option value="-1">请选择市</option>
                    {this.state.city.map((c, index) => {
                        return <option value={index}>{c}</option>
                    })}
                </select>
                <select className="form-control-custom dist" id="province" name="area">
                    <option value="-1">请先选择区</option>
                </select>
            </div>
        );
    }
}