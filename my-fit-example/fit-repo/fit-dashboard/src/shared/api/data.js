

function getEUSTotal() {
    const handleResponse = (res) => (res && res.ok) ? res.json() : Promise.reject(res);
    fetch('/api/myDevices?username=eus1').then(handleResponse)
    .then(([ingestions]) => { return ingestions.length})}
        
    

function getPMTotal() {
        fetch('/api/myDevices?username=pm1')
        .then((response) => response.json())
        .then((responseJson) => {
          return responseJson.length;
        })
        }

function getEMPTotal() {
            fetch('/api/myDevices?username=emp1')
            .then((response) => response.json())
            .then((responseJson) => {
              return responseJson.length;
            })
            }

const getStats = (data) => {
    const total = data.length;
    const active = data.filter(row => row.calculatedStatus === ' Active').length;
    const percent = Math.round(active * 100 / total);
    return { total, active, percent };
};

const aggregateData = (data) => {
    const groupedData = data.reduce((acc, row) => {
        acc[row.toCostCodeType] = acc[row.toCostCodeType] || [];
        acc[row.toCostCodeType].push(row);
        return acc;
    }, {});
    return Object.keys(groupedData).map(costCode => {
        const stats = getStats(groupedData[costCode]);
        return { ...stats, costCode };
    });
}

const filterData = (data, { role, costCode }) => {
    const notDisposed = data.filter(row => row.status !== 'Disposed');
    if (role === 'admin') {
        return notDisposed;
    }
    if (role === 'pm') {
        return notDisposed.filter(row => row.toCostCodeType.indexOf('PM') >= 0);
    }
    if (role === 'eus') {
        return notDisposed.filter(row => row.toCostCodeType.indexOf('EUS') >= 0);
    }
    if (role === 'employee') {
        return notDisposed.filter(row => row.toCostCodeType === costCode);
    }
    throw new Error(`unknown role: ${role}`);
};

export const getPhoneStatus = (phone, timeout = 1000 * 60 * 30) => (
    (Date.now() - Date.parse(phone.timeStamp)) < timeout ? ' Active' : ' Inactive'
);

export default (user) => {
    return fetch('/api/deviceInventory')
        .then(res => (res && res.ok) ? res.json() : Promise.reject(res))
        .then(data => filterData(data, user))
        .then(data => data.map(row => ({
            ...row,
            calculatedStatus: getPhoneStatus(row)
        })))
        .then(filteredData => ({
            stats: getStats(filteredData),
            table: aggregateData(filteredData)
        }));
};
