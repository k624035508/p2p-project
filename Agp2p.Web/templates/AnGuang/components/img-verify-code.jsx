import React from "react";

export default class ImageVerifyCode extends React.Component {
	constructor(props) {
		super(props);
		this.state = { random: 0 };
	}
	render() {
		return <img src={`/tools/verify_code.ashx?r=${this.state.random}&seed=${this.props.seed || 0}`}
			onClick={ev => this.setState({random: Math.random()})}/>;
	}
}