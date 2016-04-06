import keys from "lodash/object/keys"

export const classMapping = {
    "中国银行" : "boc",
    "中国工商银行" : "icbc",
    "中国建设银行" : "ccb",
    "中国农业银行" : "abc",
    "招商银行" : "cmb",
    "中国邮政储蓄银行" : "psbc",
    "中国光大银行" : "ceb",
    "中信银行" : "cncb",
    "浦发银行" : "spdb",
    "中国民生银行" : "cmsb",
    "广发银行" : "cgb",
    "兴业银行" : "cib",
    "平安银行" : "pab",
    "交通银行" : "comm",
    "华夏银行" : "hxb"
};

export const bankList = keys(classMapping);

export const classMappingPingYing = {
    "中国银行" : "zhonghang",
    "中国工商银行" : "gonghang",
    "中国建设银行" : "jianhang",
    "中国农业银行" : "nonghang",
    "招商银行" : "zhaohang",
    "中国邮政储蓄银行" : "youzheng",
	"中国光大银行" : "guangda",
	"中信银行" : "zhongxin",
	"浦发银行" : "pufa",
	"中国民生银行" : "minsheng",
	"广发银行" : "guangfa",
	"兴业银行" : "xingye",
	"平安银行" : "pingan",
	"交通银行" : "jiaohang",
	"华夏银行" : "huaxia"
};