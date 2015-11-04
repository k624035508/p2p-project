import React from "react";

class ImageVerifyCode extends React.Component {
	constructor(props) {
		super(props);
		this.state = { random: 0 };
	}
	render() {
		return (<img src={`/tools/verify_code.ashx?r=${this.state.random}&seed=${this.props.seed || 0}`}/>);
	}
}
export default ImageVerifyCode;