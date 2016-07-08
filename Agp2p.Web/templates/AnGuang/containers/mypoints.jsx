import React from "react";
import "../less/mypoints.less";

import Table from "../components/myPoints-table.jsx";
import Pagination from "../components/pagination.jsx";

class MyPointsRecord extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            selectedPoints: "RECORD",
            startTime: "",
            endTime: "",
            pageIndex: 0,
            onPageLoaded: pageCount => this.setState({ pageCount: pageCount })
        };
    }
    render() {
        let selectRecord = this.state.selectedPoints == "RECORD";
        return(
            <div>
                <div className="pointSum"><span>积分统计</span></div>
                <div className="pointChange">
                    <div><p className="pointsP1">可用积分</p><p className="pointsP2">{this.props.myPoints}</p></div>
                    <div><p className="pointsP1">已用积分</p><p className="pointsP2">0</p></div>
                    <div className="pointChangeImg"><a href="../point.html">积分兑换</a></div>
                </div>

                <div className="point-choose">
                        <span><a href="javascript:" onClick={ev => this.setState({selectedPoints: "RECORD"})}
        className={selectRecord?"active":""}>积分获取记录</a></span>
                        <span><a href="javascript:" onClick={ev => this.setState({selectedPoints: ""})}
        className={selectRecord?"":"active"}>兑换记录</a></span>
                </div>
        {selectRecord   
                    ?                   
                        <Table
                            pageIndex={this.state.pageIndex}
                            startTime={this.state.startTime}
                            endTime={this.state.endTime}
                            onPageLoaded={this.state.onPageLoaded} />                                           
                    : 
                        <table className="pointsRecord-tb">
                        <tr>
                        <td>兑换商品</td><td>属性</td><td>消费积分</td><td>兑换时间</td><td>处理状态</td>
                        </tr>
                        </table>                        
                        }
            <div className="warm-tips"><span>积分说明</span></div>
                    <div className="rechargeTips">
                        <p>1. 积分作为安广融合回馈给用户的虚拟资产，仅供在站内的积分商城适用，不支持转让赠送；</p>
                        <p>2. 积分可以兑换现金券、加息券等虚拟物品以及生活用品、电子产品等实物用品；</p>
                        <p>3. 用户按积分获取规则合法得到的积分永久有效，暂无失效日期。</p>
                    </div>
                    <div className="nav-parent">    <Pagination pageIndex={this.state.pageIndex} pageCount={this.state.pageCount}
                    onPageSelected={pageIndex => this.setState({ pageIndex: pageIndex }) }/>  </div>
            </div>
        );
    }
}

function mapStateToProps(state) {
    return { myPoints: state.userInfo.point };
}
import { connect } from 'react-redux';
export default connect(mapStateToProps)(MyPointsRecord);