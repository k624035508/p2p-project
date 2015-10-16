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
        var newState = {
            currentProvinceIndex: selectedIndex,
            city: [],
            currentCityIndex: -1,
            area: [],
            currentAreaIndex: -1
        };
        if (selectedIndex == -1) {
            this.setState(newState)
        } else {
            newState.city = citylist[selectedIndex].c.map(c => c.n);
            this.setState(newState);
        }
    }
    onCitySelected(selectedIndex) {
        var newState = {
            currentCityIndex: selectedIndex,
            area: [],
            currentAreaIndex: -1
        };
        if (selectedIndex == -1) {
            this.setState(newState)
        } else {
            var prov = citylist[this.state.currentProvinceIndex];
            var areas = prov.c[selectedIndex].a;
            if (!areas) { // 直辖市，只有两个选项，这里直接回调
                this.props.onLocationChanged(prov.p, prov.c[selectedIndex].n)
                this.setState(newState);
            } else {
                newState.area = areas.map(a => a.s);
                this.setState(newState);
            }
        }
    }
    onAreaSelected(selectedIndex) {
        var prov = citylist[this.state.currentProvinceIndex];
        var city = prov.c[this.state.currentCityIndex];
        var area = city.a[selectedIndex];
        this.props.onLocationChanged(prov.p, city.n, area.s);
        this.setState({currentAreaIndex: selectedIndex});
    }
    render() {
        return (
            <div className="selectDist">
                <select className="form-control-custom prov" id="province" name="province"
                    value={this.state.currentProvinceIndex} onChange={ev => this.onProvinceSelected(ev.target.value)}>
                    <option value="-1" key={-1}>请选择省</option>
                    {this.state.province.map((p, index) => {
                        return <option value={index} key={index}>{p}</option>
                    })}
                </select>
                <select className="form-control-custom city" id="city" name="city"
                    value={this.state.currentCityIndex} onChange={ev => this.onCitySelected(ev.target.value)}>
                    {this.state.city.length == 0 ? null : <option value="-1" key={-1}>请选择市</option>}
                    {this.state.city.map((c, index) => {
                        return <option value={index} key={index}>{c}</option>
                    })}
                </select>
                <select className="form-control-custom dist" id="area" name="area"
                    value={this.state.currentAreaIndex} onChange={ev => this.onAreaSelected(ev.target.value)}>
                    {this.state.area.length == 0 ? null : <option value="-1" key={-1}>请选择区</option>}
                    {this.state.area.map((a, index) => {
                        return <option value={index} key={index}>{a}</option>
                    })}
                </select>
            </div>
        );
    }
}