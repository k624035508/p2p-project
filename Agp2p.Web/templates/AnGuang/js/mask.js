
let mask = str => str == null ? null : str.replace(/(\S{2})\S*(\S{2})/, '$1**$2');

export default mask;