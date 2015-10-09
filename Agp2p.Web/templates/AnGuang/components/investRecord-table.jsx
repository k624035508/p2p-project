import React from "react";

export default class InvestRecordTable extends React.Component {
    constructor(props) {
        super(props);
        this.state = {data: []};
    }

    render() {
        return (
            <table className="table investRecord-tb" ref="table">
                <thead>
                <tr>
                    <th>项目名称</th>
                    <th>年利率</th>
                    <th>期限</th>
                    <th>投资金额</th>
                    <th>利息</th>
                    <th>状态</th>
                    <th>投资日期</th>
                    <th>投标协议</th>
                </tr>
                </thead>
                <tbody>
                { this.state.data.map(tr =>
                        <tr className="detailRow">
                            <td>XXXXXXXX</td>
                            <td>16%</td>
                            <td>1个月</td>
                            <td>10000</td>
                            <td>133.33</td>
                            <td>投资中</td>
                            <td>2015/10/9 17：17</td>
                            <td><a href="#">查看</a></td>
                        </tr>
                )}
                </tbody>
            </table>);
    }
};
