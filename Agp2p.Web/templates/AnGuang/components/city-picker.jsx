import React from "react";
import citylist from "../js/city.min.js";
import indexOf  from "lodash/array/indexOf";
import isEqual  from "lodash/lang/isEqual";

class CityPicker extends React.Component {
    constructor(props) {
        super(props);
        this.state = this.genStateByValue(props.defaultValue || props.value);
    }
    genStateByValue(val) {
        var state = {
            province: citylist.map(pobj => pobj.p),
            city: [],
            area: [],
            currentProvinceIndex: -1,
            currentCityIndex: -1,
            currentAreaIndex: -1
        };
        if (val) {
            var [prov, city, area] = val;
            if (prov) {
                state.currentProvinceIndex = indexOf(state.province, prov);
                state.city = citylist[state.currentProvinceIndex].c.map(c => c.n);
                if (city) {
                    state.currentCityIndex = indexOf(state.city, city);
                    var areas = citylist[state.currentProvinceIndex].c[state.currentCityIndex].a;
                    if (areas) {
                        state.area = areas.map(a => a.s);
                        if (area) {
                            state.currentAreaIndex = indexOf(state.area, area);
                        }
                    }
                }
            }
        }
        return state;
    }
    componentWillReceiveProps(nextProps) {
        if (!isEqual(this.props.value, nextProps.value)) {
            this.setState(this.genStateByValue(nextProps.value));
        }
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
            this.props.onLocationChanged();
        } else {
            newState.city = citylist[selectedIndex].c.map(c => c.n);
            this.setState(newState);
            this.props.onLocationChanged(this.state.province[selectedIndex]);
        }
    }
    onCitySelected(selectedIndex) {
        var newState = {
            currentCityIndex: selectedIndex,
            area: [],
            currentAreaIndex: -1
        };
        var prov = citylist[this.state.currentProvinceIndex];
        if (selectedIndex == -1) {
            this.setState(newState)
            this.props.onLocationChanged(prov.p)
        } else {
            var areas = prov.c[selectedIndex].a;
            if (areas) {
                newState.area = areas.map(a => a.s);
            }
            this.setState(newState);
            this.props.onLocationChanged(prov.p, prov.c[selectedIndex].n)
        }
    }
    onAreaSelected(selectedIndex) {
        var newState = {
            currentAreaIndex: selectedIndex
        }
        this.setState(newState);

        var prov = citylist[this.state.currentProvinceIndex];
        var city = prov.c[this.state.currentCityIndex];
        if (selectedIndex == -1) {
            this.props.onLocationChanged(prov.p, city.n);
        } else {
            var area = city.a[selectedIndex];
            this.props.onLocationChanged(prov.p, city.n, area.s);
        }
    }
    render() {
        return (
            <div className="selectDist">
                <select className="form-control-custom prov" id="province" name="province" disabled={this.props.disabled}
                    value={this.state.currentProvinceIndex} onChange={ev => this.onProvinceSelected(ev.target.value)}>
                    <option value="-1" key={-1}>请选择省</option>
                    {this.state.province.map((p, index) => {
                        return <option value={index} key={index}>{p}</option>
                    })}
                </select>
                <select className="form-control-custom city" id="city" name="city" disabled={this.props.disabled}
                    value={this.state.currentCityIndex} onChange={ev => this.onCitySelected(ev.target.value)}>
                    {this.state.city.length == 0 ? null : <option value="-1" key={-1}>请选择市</option>}
                    {this.state.city.map((c, index) => {
                        return <option value={index} key={index}>{c}</option>
                    })}
                </select>
                <select className="form-control-custom dist" id="area" name="area" disabled={this.props.disabled}
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
export default CityPicker;