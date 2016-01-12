
let mask = (str, keepHeadLen = 2, keepTailLen = 2) => str == null ? null : str.replace(new RegExp('^(.{' + keepHeadLen + '}).*(.{' + keepTailLen + '})$'), '$1**$2');

export default mask;