import React from "react";
import range from "lodash/utility/range"


export default class Pagination extends React.Component {
	constructor(props) {
        super(props);
        this.state = {};
    }
    render() {
    	let {pageIndex, pageCount} = this.props;
    	let isFirstPage = pageIndex == 0, isLastPage = pageCount == 0 || pageIndex == pageCount - 1;
    	return (
    		<nav>
	    		<ul className="pagination">
		    		<li className={isFirstPage ? "disabled" : ""} key="prev">
		    			<a href="javascript:;" aria-label="Previous" onClick={isFirstPage ? null : () => this.props.onPageSelected(pageIndex-1)}>
			    			<span aria-hidden="true">&laquo;</span>
		    			</a>
	    			</li>
		    		{range(pageCount).map(i => {
		    			if (pageIndex == i) {
		    				return <li className="active" key={i}><a href="javascript:;">{i+1}<span className="sr-only">(current)</span></a></li>
		    			} else {
		    				return <li key={i}><a href="javascript:;" onClick={() => this.props.onPageSelected(i)}>{i+1}</a></li>
		    			}
		    		})}
		    		<li className={isLastPage ? "disabled" : ""} key="next">
		    			<a href="javascript:;" aria-label="Next" onClick={isLastPage ? null : () => this.props.onPageSelected(pageIndex+1)}>
		    				<span aria-hidden="true">&raquo;</span>
	    				</a>
    				</li>
	    		</ul>
			</nav>
		);
    }
}
Pagination.defaultProps = { pageRange: 5 }