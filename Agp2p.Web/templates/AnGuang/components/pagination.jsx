import React from "react";
import range from "lodash/utility/range"


class Pagination extends React.Component {
	constructor(props) {
        super(props);
        this.state = {};
    }
    genPaginationItem(index) {
    	if (this.props.pageIndex == index) {
    		return <li className="active" key={index}><a href="javascript:;">{index+1}<span className="sr-only">(current)</span></a></li>
    	} else {
    		return <li key={index}><a href="javascript:;" onClick={() => this.props.onPageSelected(index)}>{index+1}</a></li>
    	}
    }
    genOmitPaginationItem(key) {
    	return <li key={key}><a>…</a></li>
    }
    render() {
    	let {pageIndex, pageCount, pageRange} = this.props;
    	let isFirstPage = pageIndex == 0, isLastPage = pageCount == 0 || pageIndex == pageCount - 1;
    	if (pageCount == 0) {
    		return <nav />;
    	}
    	return (
    		<nav>
	    		<ul className="pagination">
		    		<li className={isFirstPage ? "disabled" : ""} key="prev">
		    			<a href="javascript:;" aria-label="Previous" onClick={isFirstPage ? null : () => this.props.onPageSelected(pageIndex-1)}>
			    			<span aria-hidden="true">&laquo;</span>
		    			</a>
	    			</li>
	    			{this.genPaginationItem(0)}
	    			{ 2 < pageIndex - pageRange
	    				? this.genOmitPaginationItem("omit-left")
	    				: (pageIndex - pageRange == 2)
	    					? this.genPaginationItem(1)
	    					: null }
		    		{ range(Math.max(1, pageIndex - pageRange), Math.min(pageCount - 2, pageIndex + pageRange) + 1).map(i => this.genPaginationItem(i)) }
	    			{ pageIndex + pageRange < pageCount - 3
	    				? this.genOmitPaginationItem("omit-right")
	    				: (pageIndex + pageRange == pageCount - 3)
	    					? this.genPaginationItem(pageCount - 2)
	    					: null }
	    			{ pageCount == 1 ? null : this.genPaginationItem(pageCount - 1)}
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
Pagination.defaultProps = { pageRange: 2 }

export default Pagination;