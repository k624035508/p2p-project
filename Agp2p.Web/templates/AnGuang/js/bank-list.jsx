import keys from "lodash/object/keys"

export const classMapping = {
    "中国银行" : "BOCSH",
    "中国工商银行" : "ICBC",
    "中国建设银行" : "CCB",
    "中国农业银行" : "ABC",
    "招商银行" : "CMB",
    "中国邮政储蓄银行" : "PSBC",
	"中国光大银行" : "CEB",
	"中信银行" : "CNCB",
	"浦发银行" : "SPDB",
	"中国民生银行" : "CMBC",
	"广发银行" : "GDB",
	"兴业银行" : "CIB",
	"平安银行" : "PAB",
	"交通银行" : "BOCOM",
	"华夏银行" : "HXB"
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