import { ajax } from "jquery";
import alert from "../components/tips_alert.js";

export const UPDATE_ADDRESS = "UPDATE_ADDRESS"

export function updateAddress(orderAddress) {
    return {
        type: UPDATE_ADDRESS,
        orderAddress
    };
}

export function fetchAddress() {
    return function (dispatch) {
        let url = USER_CENTER_ASPX_PATH + "/AjaxQueryAddress";
        return ajax({
            type: "get",
            dataType: "json",
            contentType: "application/json",
            url: url,
            data: "",
            success: function(result) {
                let data = JSON.parse(result.d);
                dispatch(updateAddress(data));
            }.bind(this),
            error: function(xhr, status, err) {
                console.error(url, status, err.toString());
            }.bind(this)
        });
    };
}

export function appendAddress(address, area, postalCode, orderName, orderPhone) {
    return function (dispatch) {
        let url = USER_CENTER_ASPX_PATH + "/AjaxAppendAddress";
        return ajax({
            type: "post",
            dataType: "json",
            contentType: "application/json",
            url: url,
            data: JSON.stringify({
                address,
                area,
                postalCode,
                orderName,
                orderPhone
            }),
            success: function (data) {
                dispatch(fetchAddress());
                alert(data.d);
                $("#addressConfirm").modal("hide");
            }.bind(this),
            error: function (xhr, status, err) {
                alert(xhr.responseJSON.d);
                console.error(url, status, err.toString());
            }.bind(this)
        });
    };
}

export function deleteAddress(addressId) {
    return function(dispatch) {
        let url = USER_CENTER_ASPX_PATH + "/AjaxDeleteAddress";
        return ajax({
            type: "post",
            dataType: "json",
            contentType: "application/json",
            url: url,
            data: JSON.stringify({ addressId }),
            success: function(data) {
                dispatch(fetchAddress());
                alert(data.d);
            }.bind(this),
            error: function(xhr, status, err) {
                alert(xhr.responseJSON.d);
                console.error(url, status, err.toString());
            }.bind(this)
        });
    };
}

export function modifyAddress(addressId, address, area, postalCode, orderName, orderPhone) {
    return function(dispatch) {
        let url = USER_CENTER_ASPX_PATH + "/AjaxModifyAddress";
        return ajax({
            type:"post",
            dataType:"json",
            contentType:"application/json",
            url:url,
            data:JSON.stringify({addressId, address, area, postalCode, orderName, orderPhone}),
            success:function(data) {
                dispatch(fetchAddress());
                alert(data.d);
                $("#addressConfirm").modal("hide");
            }.bind(this),
            error:function(xhr, status, err) {
                alert(xhr.responseJSON.d);
                console.error(url, status, err.toString());
            }.bind(this)
        });
    }
}
