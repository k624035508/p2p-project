import React from "react";
import { ajax } from "jquery";
import HorizontalPicker from "../components/horizontal-picker.jsx";
import Pagination from "../components/pagination.jsx";
import "../less/claimstransfer.less";

class ClaimsTransfer extends React.Component{
    constructor(props){
        super(props);
        this.state = {pageIndex:0, pageCount:3};
    }

    render(){
        return(
            <div className="claimsTransferPage">
                <div className="top-wrapper">
                    <div className="blue-container">
                        <p className="whatFor"><a href="javascript:">什么时债权转让？</a></p>
                        <ul className="claims-db list-unstyled list-inline ">
                            <li>
                                <p>成功转出金额</p>
                                <p>0.00 元</p>
                            </li>
                            <li>
                                <p>已转出债权笔数</p>
                                <p>0</p>
                            </li>
                            <li>
                                <p>成功转入金额</p>
                                <p>0.00 元</p>
                            </li>
                            <li>
                                <p>已转入债权笔数</p>
                                <p>0</p>
                            </li>
                        </ul>
                    </div>
                </div>
                <div className="bottom-wrapper">
                    <div className="warm-tips"><span>债权转让</span></div>
                    <HorizontalPicker onTypeChange={newType => this.setState({type: newType}) } enumFullName="Agp2p.Common.Agp2pEnums+MyInvestRadioBtnTypeEnum" />
                    <div className="tb-container">
                        <table className="table claimsTransfer-tb">
                            <thead>
                                <tr>
                                    <th>债权编号</th>
                                    <th>剩余期数</th>
                                    <th>年化利率</th>
                                    <th>转让比例</th>
                                    <th>折让比例</th>
                                    <th>转让手续费</th>
                                    <th>转让价格</th>
                                    <th>申请时间</th>
                                    <th>操作</th>
                                </tr>
                            </thead>
                            <tbody>
                            {/*每页显示9行数据*/}
                                <tr>
                                    <td>001</td>
                                    <td>2</td>
                                    <td>4.2%</td>
                                    <td>50%</td>
                                    <td>90%</td>
                                    <td>100.00</td>
                                    <td>2222.00</td>
                                    <td>2016/3/9</td>
                                    <td>操作</td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                    <Pagination pageIndex={this.state.pageIndex} pageCount={this.state.pageCount}
                                onPageSelected={pageIndex => this.setState({pageIndex: pageIndex})}/>
                </div>
            </div>
        );
    }
}
export default ClaimsTransfer;